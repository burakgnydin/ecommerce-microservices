import { motion } from 'framer-motion'
import { ChevronRight, Minus, PackageCheck, PackageX, Plus, Tag } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router-dom'
import { ApiError } from '../api/client'
import { getProductById } from '../api/products'
import type { Product } from '../api/types'
import { Header, useCurrentUser } from '../components/Header'
import { ProductDetailSkeleton } from '../components/ProductDetailSkeleton'
import { ProductImage } from '../components/ProductImage'
import { AuthPrompt } from '../components/ui/AuthPrompt'
import { Button } from '../components/ui/Button'
import { ShimmerButton } from '../components/ui/ShimmerButton'
import { useCart } from '../context/CartContext'

function formatPrice(price: number) {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(price)
}

export default function ProductDetailPage() {
  const { user } = useCurrentUser()
  const isAdmin = user?.role === 'Admin'
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const location = useLocation()
  const canGoBack = location.key !== 'default'
  const [product, setProduct] = useState<Product | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const { addItem } = useCart()
  const [quantity, setQuantity] = useState(1)
  const [isAdding, setIsAdding] = useState(false)
  const [addError, setAddError] = useState<string | null>(null)
  const [added, setAdded] = useState(false)

  function goBackToProducts() {
    if (canGoBack) navigate(-1)
    else navigate('/products')
  }

  async function handleAddToCart() {
    if (!product) return
    setAddError(null)
    setAdded(false)
    setIsAdding(true)
    try {
      await addItem(product.id, quantity)
      setAdded(true)
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) {
        setAddError('login')
      } else {
        setAddError(err instanceof ApiError ? err.message : 'Sepete eklenemedi.')
      }
    } finally {
      setIsAdding(false)
    }
  }

  useEffect(() => {
    if (!id) return
    let cancelled = false
    setIsLoading(true)
    setError(null)

    getProductById(id)
      .then((data) => {
        if (!cancelled) setProduct(data)
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : 'Ürün yüklenemedi.')
        }
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [id])

  return (
    <div className="min-h-screen">
      <Header />
      <main className="mx-auto max-w-5xl px-4 py-10">
        <nav aria-label="Breadcrumb" className="mb-6 flex items-center text-sm text-muted-foreground">
          <button type="button" onClick={goBackToProducts} className="hover:text-foreground">
            Ürünler
          </button>
          {product?.categoryName && (
            <>
              <ChevronRight className="mx-1 h-4 w-4" />
              <span>{product.categoryName}</span>
            </>
          )}
        </nav>

        {isLoading && <ProductDetailSkeleton />}
        {error && <p className="text-destructive">{error}</p>}

        {!isLoading && !error && product && (
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3, ease: 'easeOut' }}
            className="grid grid-cols-1 gap-8 md:grid-cols-2 md:gap-12"
          >
            <div className="overflow-hidden rounded-xl border border-border">
              <ProductImage product={product} className="aspect-square w-full" iconClassName="h-16 w-16" />
            </div>

            <div className="flex flex-col">
              <h1 className="text-3xl font-bold tracking-tight text-foreground md:text-4xl">{product.name}</h1>

              <div className="mt-2">
                <span className="text-3xl font-bold text-foreground">{formatPrice(product.price)}</span>
              </div>

              <div className="mt-5 flex flex-wrap gap-2">
                {product.categoryName && (
                  <span className="inline-flex items-center gap-1.5 rounded-full bg-secondary px-3 py-1 text-xs font-medium text-secondary-foreground">
                    <Tag className="h-3.5 w-3.5" />
                    {product.categoryName}
                  </span>
                )}
                {product.stock > 0 ? (
                  <span className="inline-flex items-center gap-1.5 rounded-full bg-secondary px-3 py-1 text-xs font-medium text-secondary-foreground">
                    <PackageCheck className="h-3.5 w-3.5" />
                    {product.stock} adet stokta
                  </span>
                ) : product.allowsPreOrder ? (
                  <span className="inline-flex items-center gap-1.5 rounded-full bg-primary/10 px-3 py-1 text-xs font-medium text-primary">
                    <PackageCheck className="h-3.5 w-3.5" />
                    Ön sipariş verilebilir
                  </span>
                ) : (
                  <span className="inline-flex items-center gap-1.5 rounded-full bg-destructive/10 px-3 py-1 text-xs font-medium text-destructive">
                    <PackageX className="h-3.5 w-3.5" />
                    Stokta yok
                  </span>
                )}
              </div>

              {product.description && (
                <p className="mt-6 leading-relaxed text-muted-foreground">{product.description}</p>
              )}

              {!isAdmin && (product.stock > 0 || product.allowsPreOrder) && (
                <div className="mt-6 flex items-center gap-3">
                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      onClick={() => setQuantity((q) => Math.max(1, q - 1))}
                      className="flex h-9 w-9 items-center justify-center rounded-md border border-border text-foreground hover:bg-secondary"
                      aria-label="Azalt"
                    >
                      <Minus className="h-4 w-4" />
                    </button>
                    <span className="w-6 text-center font-medium text-foreground">{quantity}</span>
                    <button
                      type="button"
                      onClick={() => setQuantity((q) => q + 1)}
                      className="flex h-9 w-9 items-center justify-center rounded-md border border-border text-foreground hover:bg-secondary"
                      aria-label="Artır"
                    >
                      <Plus className="h-4 w-4" />
                    </button>
                  </div>
                  <Button type="button" onClick={handleAddToCart} disabled={isAdding}>
                    {isAdding ? 'Ekleniyor...' : 'Sepete Ekle'}
                  </Button>
                </div>
              )}

              {!isAdmin && added && <p className="mt-2 text-sm text-success">Sepete eklendi.</p>}
              {!isAdmin && addError === 'login' && (
                <AuthPrompt variant="inline" className="mt-3" message="Sepete eklemek için giriş yapmalısın." />
              )}
              {!isAdmin && addError && addError !== 'login' && <p className="mt-2 text-sm text-destructive">{addError}</p>}

              <ShimmerButton type="button" onClick={goBackToProducts} className="mt-8 px-4 py-1.5">
                <span className="whitespace-pre-wrap text-center text-xs font-medium leading-none tracking-tight">
                  ← Ürünlere dön
                </span>
              </ShimmerButton>
            </div>
          </motion.div>
        )}
      </main>
    </div>
  )
}
