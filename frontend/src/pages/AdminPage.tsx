import { type FormEvent, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Pencil, Plus, Trash2 } from 'lucide-react'
import { createCategory, deleteCategory, getCategories, updateCategory } from '../api/categories'
import { ApiError, translateApiError } from '../api/client'
import { getAllOrdersAdmin } from '../api/orders'
import { getAllPaymentsAdmin } from '../api/payments'
import { createProduct, deleteProduct, getProducts, updateProduct } from '../api/products'
import type { AdminUser, Category, Order, Payment, Product, ProductCreateRequest } from '../api/types'
import { getAllUsersAdmin, getMe } from '../api/users'
import { ProductForm } from '../components/admin/ProductForm'
import { Header } from '../components/Header'
import { ProductImage } from '../components/ProductImage'
import { AdminOverviewSlider } from '../components/ui/AdminOverviewSlider'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { ConfirmModal } from '../components/ui/ConfirmModal'
import { Skeleton } from '../components/ui/Skeleton'
import { getAccessToken } from '../lib/auth'
import { smoothScrollToId } from '../lib/utils'

const inputClasses =
  'h-10 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground outline-none focus-visible:ring-2 focus-visible:ring-ring'

function formatPrice(price: number) {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(price)
}

function formatDate(value: string) {
  return new Date(value).toLocaleString('tr-TR')
}

type OrderStatusFilter = 'All' | Order['status']

const ORDER_STATUS_FILTERS: { id: OrderStatusFilter; label: string }[] = [
  { id: 'All', label: 'Tümü' },
  { id: 'Pending', label: 'Beklemede' },
  { id: 'Paid', label: 'Ödendi' },
  { id: 'Cancelled', label: 'İptal edildi' },
]

function orderStatusBadge(status: Order['status']) {
  switch (status) {
    case 'Paid':
      return { label: 'Ödendi', className: 'bg-green-600/10 text-green-600' }
    case 'Cancelled':
      return { label: 'İptal edildi', className: 'bg-destructive/10 text-destructive' }
    default:
      return { label: 'Beklemede', className: 'bg-secondary text-muted-foreground' }
  }
}

function paymentStatusBadge(status: Payment['status'] | undefined) {
  switch (status) {
    case 'Succeeded':
      return { label: 'Başarılı', className: 'bg-green-600/10 text-green-600' }
    case 'Failed':
      return { label: 'Başarısız', className: 'bg-destructive/10 text-destructive' }
    default:
      return { label: 'Ödeme yok', className: 'bg-secondary text-muted-foreground' }
  }
}

export default function AdminPage() {
  const navigate = useNavigate()
  const [isAuthorized, setIsAuthorized] = useState(false)
  const [isCheckingAuth, setIsCheckingAuth] = useState(true)

  const [categories, setCategories] = useState<Category[]>([])
  const [products, setProducts] = useState<Product[]>([])
  const [isLoadingData, setIsLoadingData] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const [newCategoryName, setNewCategoryName] = useState('')
  const [isAddingCategory, setIsAddingCategory] = useState(false)
  const [editingCategoryId, setEditingCategoryId] = useState<string | null>(null)
  const [editingCategoryName, setEditingCategoryName] = useState('')
  const [isSavingCategory, setIsSavingCategory] = useState(false)
  const [categoryError, setCategoryError] = useState<string | null>(null)
  const [deleteCategoryTarget, setDeleteCategoryTarget] = useState<Category | null>(null)
  const [isDeletingCategory, setIsDeletingCategory] = useState(false)

  const [productFormMode, setProductFormMode] = useState<'create' | 'edit' | null>(null)
  const [editingProduct, setEditingProduct] = useState<Product | null>(null)
  const [isSavingProduct, setIsSavingProduct] = useState(false)
  const [productError, setProductError] = useState<string | null>(null)
  const [deleteProductTarget, setDeleteProductTarget] = useState<Product | null>(null)
  const [isDeletingProduct, setIsDeletingProduct] = useState(false)

  const [orders, setOrders] = useState<Order[]>([])
  const [payments, setPayments] = useState<Payment[]>([])
  const [adminUsers, setAdminUsers] = useState<AdminUser[]>([])
  const [isLoadingOverview, setIsLoadingOverview] = useState(true)
  const [overviewError, setOverviewError] = useState<string | null>(null)
  const [orderStatusFilter, setOrderStatusFilter] = useState<OrderStatusFilter>('All')

  useEffect(() => {
    if (!getAccessToken()) {
      navigate('/login')
      return
    }

    getMe()
      .then((user) => {
        if (user.role !== 'Admin') {
          navigate('/')
          return
        }
        setIsAuthorized(true)
      })
      .catch(() => navigate('/'))
      .finally(() => setIsCheckingAuth(false))
  }, [navigate])

  function loadData() {
    setIsLoadingData(true)
    setLoadError(null)
    Promise.all([getCategories(), getProducts(1, 100)])
      .then(([categoriesResult, productsResult]) => {
        setCategories(categoriesResult)
        setProducts(productsResult.items)
      })
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : 'Veriler yüklenemedi.'))
      .finally(() => setIsLoadingData(false))
  }

  useEffect(() => {
    if (isAuthorized) loadData()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isAuthorized])

  useEffect(() => {
    if (!isAuthorized) return
    setIsLoadingOverview(true)
    setOverviewError(null)
    Promise.all([getAllOrdersAdmin(), getAllPaymentsAdmin(), getAllUsersAdmin()])
      .then(([ordersResult, paymentsResult, usersResult]) => {
        setOrders(ordersResult)
        setPayments(paymentsResult)
        setAdminUsers(usersResult)
      })
      .catch((err) => setOverviewError(err instanceof ApiError ? err.message : 'Sipariş ve ödeme verileri yüklenemedi.'))
      .finally(() => setIsLoadingOverview(false))
  }, [isAuthorized])

  const usersById = useMemo(() => new Map(adminUsers.map((u) => [u.id, u])), [adminUsers])

  const latestPaymentByOrderId = useMemo(() => {
    const map = new Map<string, Payment>()
    for (const payment of payments) {
      const existing = map.get(payment.orderId)
      if (!existing || new Date(payment.createdAt) > new Date(existing.createdAt)) {
        map.set(payment.orderId, payment)
      }
    }
    return map
  }, [payments])

  const filteredOrders = useMemo(
    () => (orderStatusFilter === 'All' ? orders : orders.filter((o) => o.status === orderStatusFilter)),
    [orders, orderStatusFilter],
  )

  async function handleAddCategory(event: FormEvent) {
    event.preventDefault()
    setCategoryError(null)
    setIsAddingCategory(true)
    try {
      const category = await createCategory({ name: newCategoryName })
      setCategories((prev) => [...prev, category].sort((a, b) => a.name.localeCompare(b.name)))
      setNewCategoryName('')
    } catch (err) {
      setCategoryError(translateApiError(err, 'Kategori eklenemedi, tekrar deneyin.'))
    } finally {
      setIsAddingCategory(false)
    }
  }

  function handleStartEditCategory(category: Category) {
    setCategoryError(null)
    setEditingCategoryId(category.id)
    setEditingCategoryName(category.name)
  }

  async function handleSaveCategory(event: FormEvent) {
    event.preventDefault()
    if (!editingCategoryId) return
    setCategoryError(null)
    setIsSavingCategory(true)
    try {
      const updated = await updateCategory(editingCategoryId, { name: editingCategoryName })
      setCategories((prev) =>
        prev.map((c) => (c.id === updated.id ? updated : c)).sort((a, b) => a.name.localeCompare(b.name)),
      )
      setEditingCategoryId(null)
    } catch (err) {
      setCategoryError(translateApiError(err, 'Kategori güncellenemedi, tekrar deneyin.'))
    } finally {
      setIsSavingCategory(false)
    }
  }

  async function handleConfirmDeleteCategory() {
    if (!deleteCategoryTarget) return
    setCategoryError(null)
    setIsDeletingCategory(true)
    try {
      await deleteCategory(deleteCategoryTarget.id)
      setCategories((prev) => prev.filter((c) => c.id !== deleteCategoryTarget.id))
      setDeleteCategoryTarget(null)
    } catch (err) {
      setCategoryError(translateApiError(err, 'Kategori silinemedi, tekrar deneyin.'))
      setDeleteCategoryTarget(null)
    } finally {
      setIsDeletingCategory(false)
    }
  }

  async function handleProductSubmit(dto: ProductCreateRequest) {
    setProductError(null)
    setIsSavingProduct(true)
    try {
      if (productFormMode === 'edit' && editingProduct) {
        await updateProduct(editingProduct.id, dto)
      } else {
        await createProduct(dto)
      }
      setProductFormMode(null)
      setEditingProduct(null)
      loadData()
    } catch (err) {
      setProductError(translateApiError(err, 'Ürün kaydedilemedi, tekrar deneyin.'))
    } finally {
      setIsSavingProduct(false)
    }
  }

  async function handleConfirmDeleteProduct() {
    if (!deleteProductTarget) return
    setProductError(null)
    setIsDeletingProduct(true)
    try {
      await deleteProduct(deleteProductTarget.id)
      setProducts((prev) => prev.filter((p) => p.id !== deleteProductTarget.id))
      setDeleteProductTarget(null)
    } catch (err) {
      setProductError(translateApiError(err, 'Ürün silinemedi, tekrar deneyin.'))
      setDeleteProductTarget(null)
    } finally {
      setIsDeletingProduct(false)
    }
  }

  if (isCheckingAuth || !isAuthorized) {
    return (
      <div className="min-h-screen">
        <Header />
        <main className="mx-auto max-w-5xl px-4 py-10">
          <Skeleton className="h-8 w-64" />
        </main>
      </div>
    )
  }

  return (
    <div className="min-h-screen">
      <Header />
      <main className="mx-auto max-w-5xl px-4 py-10">
        <h1 className="text-3xl font-bold tracking-tight text-foreground">Yönetim Paneli</h1>
        <p className="mt-2 text-muted-foreground">Mağazanı buradan yönet.</p>

        <AdminOverviewSlider />

        {loadError && <p className="mt-6 text-destructive">{loadError}</p>}

        {!loadError && isLoadingData && (
          <div className="mt-8 space-y-4">
            <Skeleton className="h-32 w-full rounded-lg" />
            <Skeleton className="h-32 w-full rounded-lg" />
          </div>
        )}

        {!loadError && !isLoadingData && (
          <div className="mt-8 grid gap-6">
            <Card id="admin-section-categories" className="p-6">
              <h2 className="text-lg font-semibold text-foreground">Kategoriler</h2>

              <ul className="mt-4 divide-y divide-border">
                {categories.map((category) => (
                  <li key={category.id} className="flex items-center gap-3 py-3">
                    {editingCategoryId === category.id ? (
                      <form onSubmit={handleSaveCategory} className="flex flex-1 items-center gap-3">
                        <input
                          type="text"
                          required
                          value={editingCategoryName}
                          onChange={(e) => setEditingCategoryName(e.target.value)}
                          className={inputClasses}
                        />
                        <Button type="submit" size="sm" disabled={isSavingCategory}>
                          Kaydet
                        </Button>
                        <Button type="button" size="sm" variant="outline" onClick={() => setEditingCategoryId(null)}>
                          Vazgeç
                        </Button>
                      </form>
                    ) : (
                      <>
                        <span className="flex-1 text-sm text-foreground">{category.name}</span>
                        <button
                          type="button"
                          onClick={() => handleStartEditCategory(category)}
                          className="text-muted-foreground hover:text-foreground"
                          aria-label="Düzenle"
                        >
                          <Pencil className="h-4 w-4" />
                        </button>
                        <button
                          type="button"
                          onClick={() => setDeleteCategoryTarget(category)}
                          className="text-muted-foreground hover:text-destructive"
                          aria-label="Sil"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </>
                    )}
                  </li>
                ))}
              </ul>

              <form onSubmit={handleAddCategory} className="mt-4 flex items-center gap-3">
                <input
                  type="text"
                  required
                  placeholder="Yeni kategori adı"
                  value={newCategoryName}
                  onChange={(e) => setNewCategoryName(e.target.value)}
                  className={inputClasses}
                />
                <Button type="submit" size="sm" disabled={isAddingCategory}>
                  <Plus className="h-4 w-4" /> Ekle
                </Button>
              </form>

              {categoryError && <p className="mt-3 text-sm text-destructive">{categoryError}</p>}
            </Card>

            <Card id="admin-section-products" className="p-6">
              <div className="flex items-center justify-between">
                <h2 className="text-lg font-semibold text-foreground">Ürünler</h2>
                {productFormMode === null && (
                  <Button
                    size="sm"
                    onClick={() => {
                      setProductError(null)
                      setEditingProduct(null)
                      setProductFormMode('create')
                    }}
                  >
                    <Plus className="h-4 w-4" /> Yeni Ürün Ekle
                  </Button>
                )}
              </div>

              {productFormMode !== null && (
                <div className="mt-4">
                  <ProductForm
                    initialValue={editingProduct ?? undefined}
                    categories={categories}
                    isSubmitting={isSavingProduct}
                    onSubmit={handleProductSubmit}
                    onCancel={() => {
                      setProductFormMode(null)
                      setEditingProduct(null)
                    }}
                  />
                </div>
              )}

              {productError && <p className="mt-3 text-sm text-destructive">{productError}</p>}

              <ul className="mt-4 divide-y divide-border">
                {products.map((product) => (
                  <li key={product.id} className="flex items-center gap-4 py-3">
                    <ProductImage product={product} className="h-12 w-12 rounded-md" iconClassName="h-5 w-5" />
                    <div className="flex-1">
                      <p className="text-sm font-medium text-foreground">{product.name}</p>
                      <p className="text-xs text-muted-foreground">
                        {product.categoryName ?? categories.find((c) => c.id === product.categoryId)?.name} · Stok: {product.stock}
                      </p>
                    </div>
                    <span className="text-sm font-medium text-foreground">{formatPrice(product.price)}</span>
                    <button
                      type="button"
                      onClick={() => {
                        setProductError(null)
                        setEditingProduct(product)
                        setProductFormMode('edit')
                        smoothScrollToId('admin-section-products')
                      }}
                      className="text-muted-foreground hover:text-foreground"
                      aria-label="Düzenle"
                    >
                      <Pencil className="h-4 w-4" />
                    </button>
                    <button
                      type="button"
                      onClick={() => setDeleteProductTarget(product)}
                      className="text-muted-foreground hover:text-destructive"
                      aria-label="Sil"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </li>
                ))}
              </ul>
            </Card>

            <Card id="admin-section-orders" className="p-6">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <h2 className="text-lg font-semibold text-foreground">Sipariş ve Ödeme Genel Bakışı</h2>
                <div className="flex flex-wrap gap-2">
                  {ORDER_STATUS_FILTERS.map((filter) => (
                    <button
                      key={filter.id}
                      type="button"
                      onClick={() => setOrderStatusFilter(filter.id)}
                      className={`rounded-full px-3 py-1 text-xs font-medium transition-colors ${
                        orderStatusFilter === filter.id
                          ? 'bg-primary text-primary-foreground'
                          : 'bg-secondary text-secondary-foreground hover:bg-secondary/80'
                      }`}
                    >
                      {filter.label}
                    </button>
                  ))}
                </div>
              </div>

              {overviewError && <p className="mt-4 text-sm text-destructive">{overviewError}</p>}

              {!overviewError && isLoadingOverview && (
                <div className="mt-4 space-y-2">
                  <Skeleton className="h-10 w-full rounded-md" />
                  <Skeleton className="h-10 w-full rounded-md" />
                  <Skeleton className="h-10 w-full rounded-md" />
                </div>
              )}

              {!overviewError && !isLoadingOverview && filteredOrders.length === 0 && (
                <p className="mt-4 text-sm text-muted-foreground">Bu filtreyle eşleşen sipariş yok.</p>
              )}

              {!overviewError && !isLoadingOverview && filteredOrders.length > 0 && (
                <div className="mt-4 overflow-x-auto">
                  <table className="w-full min-w-[720px] text-left text-sm">
                    <thead>
                      <tr className="border-b border-border text-xs text-muted-foreground">
                        <th className="pb-2 pr-4 font-medium">Sipariş</th>
                        <th className="pb-2 pr-4 font-medium">Müşteri</th>
                        <th className="pb-2 pr-4 font-medium">Durum</th>
                        <th className="pb-2 pr-4 font-medium">Ödeme</th>
                        <th className="pb-2 pr-4 font-medium">Tutar</th>
                        <th className="pb-2 font-medium">Tarih</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {filteredOrders.map((order) => {
                        const customer = usersById.get(order.userId)
                        const payment = latestPaymentByOrderId.get(order.id)
                        const status = orderStatusBadge(order.status)
                        const paymentStatus = paymentStatusBadge(payment?.status)
                        return (
                          <tr key={order.id}>
                            <td className="py-3 pr-4 font-mono text-xs text-muted-foreground">{order.id.slice(0, 8)}</td>
                            <td className="py-3 pr-4">
                              <p className="font-medium text-foreground">{customer?.name ?? 'Bilinmiyor'}</p>
                              <p className="text-xs text-muted-foreground">{customer?.email ?? order.userId}</p>
                            </td>
                            <td className="py-3 pr-4">
                              <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${status.className}`}>
                                {status.label}
                              </span>
                            </td>
                            <td className="py-3 pr-4">
                              <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${paymentStatus.className}`}>
                                {paymentStatus.label}
                              </span>
                              {payment && <p className="mt-1 text-xs text-muted-foreground">{payment.maskedCardNumber}</p>}
                            </td>
                            <td className="py-3 pr-4 font-medium text-foreground">{formatPrice(order.totalAmount)}</td>
                            <td className="py-3 text-xs text-muted-foreground">{formatDate(order.createdAt)}</td>
                          </tr>
                        )
                      })}
                    </tbody>
                  </table>
                </div>
              )}
            </Card>
          </div>
        )}
      </main>

      <ConfirmModal
        isOpen={deleteCategoryTarget !== null}
        title="Kategoriyi sil"
        message={`"${deleteCategoryTarget?.name}" kategorisini silmek istediğine emin misin?`}
        confirmText={isDeletingCategory ? 'Siliniyor...' : 'Sil'}
        isDestructive
        onConfirm={handleConfirmDeleteCategory}
        onCancel={() => setDeleteCategoryTarget(null)}
      />

      <ConfirmModal
        isOpen={deleteProductTarget !== null}
        title="Ürünü sil"
        message={`"${deleteProductTarget?.name}" ürününü silmek istediğine emin misin?`}
        confirmText={isDeletingProduct ? 'Siliniyor...' : 'Sil'}
        isDestructive
        onConfirm={handleConfirmDeleteProduct}
        onCancel={() => setDeleteProductTarget(null)}
      />
    </div>
  )
}
