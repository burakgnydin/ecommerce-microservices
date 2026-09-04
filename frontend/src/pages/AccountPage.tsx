import { type FormEvent, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { CreditCard, KeyRound, MapPin, Package, Pencil, Plus, Trash2, User as UserIcon } from 'lucide-react'
import { logout } from '../api/auth'
import { createAddress, deleteAddress, getAddresses, updateAddress } from '../api/addresses'
import { ApiError, translateApiError } from '../api/client'
import { getMyOrders } from '../api/orders'
import { getMyPayments } from '../api/payments'
import type { Address, Order, Payment, UserResponse } from '../api/types'
import { changePassword, getMe, updateMe } from '../api/users'
import { Header } from '../components/Header'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { ConfirmModal } from '../components/ui/ConfirmModal'
import { OrderTracking, type OrderTrackingStep } from '../components/ui/OrderTracking'
import { Skeleton } from '../components/ui/Skeleton'
import { getAccessToken, setAccessToken } from '../lib/auth'

const inputClasses =
  'h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground outline-none focus-visible:ring-2 focus-visible:ring-ring'

function formatPrice(price: number) {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(price)
}

type Tab = 'profile' | 'security' | 'orders' | 'addresses' | 'payments'

const TABS: { id: Tab; label: string; icon: typeof UserIcon }[] = [
  { id: 'profile', label: 'Profil', icon: UserIcon },
  { id: 'security', label: 'Güvenlik', icon: KeyRound },
  { id: 'orders', label: 'Siparişlerim', icon: Package },
  { id: 'addresses', label: 'Adreslerim', icon: MapPin },
  { id: 'payments', label: 'Ödemelerim', icon: CreditCard },
]

function orderStatusInfo(status: Order['status']) {
  switch (status) {
    case 'Paid':
      return { label: 'Ödendi', className: 'bg-green-600/10 text-green-600' }
    case 'Cancelled':
      return { label: 'İptal edildi', className: 'bg-destructive/10 text-destructive' }
    default:
      return { label: 'Beklemede', className: 'bg-secondary text-muted-foreground' }
  }
}

function buildOrderTrackingSteps(order: Order): OrderTrackingStep[] {
  const placedStep: OrderTrackingStep = {
    name: 'Sipariş Alındı',
    timestamp: new Date(order.createdAt).toLocaleString('tr-TR'),
    status: 'completed',
  }

  switch (order.status) {
    case 'Paid':
      return [placedStep, { name: 'Ödendi', status: 'completed' }]
    case 'Cancelled':
      return [placedStep, { name: 'İptal Edildi', status: 'cancelled' }]
    default:
      return [placedStep, { name: 'Ödeme Bekleniyor', status: 'current' }]
  }
}

export default function AccountPage() {
  const navigate = useNavigate()
  const [user, setUser] = useState<UserResponse | null>(null)
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [formError, setFormError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmNewPassword, setConfirmNewPassword] = useState('')
  const [passwordError, setPasswordError] = useState<string | null>(null)
  const [passwordSuccess, setPasswordSuccess] = useState<string | null>(null)
  const [isChangingPassword, setIsChangingPassword] = useState(false)

  const [activeTab, setActiveTab] = useState<Tab>('profile')

  const [orders, setOrders] = useState<Order[]>([])
  const [ordersLoaded, setOrdersLoaded] = useState(false)
  const [isLoadingOrders, setIsLoadingOrders] = useState(false)
  const [ordersError, setOrdersError] = useState<string | null>(null)

  const [addresses, setAddresses] = useState<Address[]>([])
  const [addressesLoaded, setAddressesLoaded] = useState(false)
  const [isLoadingAddresses, setIsLoadingAddresses] = useState(false)
  const [addressError, setAddressError] = useState<string | null>(null)
  const [isAddingAddress, setIsAddingAddress] = useState(false)
  const [newAddressTitle, setNewAddressTitle] = useState('')
  const [newAddressCity, setNewAddressCity] = useState('')
  const [newAddressDistrict, setNewAddressDistrict] = useState('')
  const [newAddressFullAddress, setNewAddressFullAddress] = useState('')
  const [editingAddressId, setEditingAddressId] = useState<string | null>(null)
  const [editingAddressTitle, setEditingAddressTitle] = useState('')
  const [editingAddressCity, setEditingAddressCity] = useState('')
  const [editingAddressDistrict, setEditingAddressDistrict] = useState('')
  const [editingAddressFullAddress, setEditingAddressFullAddress] = useState('')
  const [isSavingAddress, setIsSavingAddress] = useState(false)
  const [deleteAddressTarget, setDeleteAddressTarget] = useState<Address | null>(null)
  const [isDeletingAddress, setIsDeletingAddress] = useState(false)

  const [payments, setPayments] = useState<Payment[]>([])
  const [paymentsLoaded, setPaymentsLoaded] = useState(false)
  const [isLoadingPayments, setIsLoadingPayments] = useState(false)
  const [paymentsError, setPaymentsError] = useState<string | null>(null)

  const [daysSinceJoin, setDaysSinceJoin] = useState(0)

  useEffect(() => {
    if (!getAccessToken()) {
      navigate('/login')
      return
    }

    getMe()
      .then((result) => {
        if (result.role === 'Admin') {
          navigate('/admin')
          return
        }
        setUser(result)
        setName(result.name)
        setEmail(result.email)
        setDaysSinceJoin(Math.max(0, Math.floor((Date.now() - new Date(result.createdAt).getTime()) / (1000 * 60 * 60 * 24))))
      })
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : 'Bilgiler yüklenemedi.'))
      .finally(() => setIsLoading(false))
  }, [navigate])

  useEffect(() => {
    if (activeTab === 'orders' && !ordersLoaded) {
      setIsLoadingOrders(true)
      getMyOrders()
        .then((result) => {
          setOrders(result)
          setOrdersLoaded(true)
        })
        .catch((err) => setOrdersError(translateApiError(err, 'Siparişler yüklenemedi.')))
        .finally(() => setIsLoadingOrders(false))
    }
    if (activeTab === 'addresses' && !addressesLoaded) {
      setIsLoadingAddresses(true)
      getAddresses()
        .then((result) => {
          setAddresses(result)
          setAddressesLoaded(true)
        })
        .catch((err) => setAddressError(translateApiError(err, 'Adresler yüklenemedi.')))
        .finally(() => setIsLoadingAddresses(false))
    }
    if (activeTab === 'payments' && !paymentsLoaded) {
      setIsLoadingPayments(true)
      getMyPayments()
        .then((result) => {
          setPayments(result)
          setPaymentsLoaded(true)
        })
        .catch((err) => setPaymentsError(translateApiError(err, 'Ödemeler yüklenemedi.')))
        .finally(() => setIsLoadingPayments(false))
    }
  }, [activeTab, ordersLoaded, addressesLoaded, paymentsLoaded])

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setFormError(null)
    setSuccessMessage(null)

    if (user && name === user.name && email === user.email) {
      setSuccessMessage('Bilgilerin güncellendi.')
      return
    }

    setIsSubmitting(true)
    try {
      const result = await updateMe({ name, email })
      setUser(result)
      setName(result.name)
      setEmail(result.email)
      setSuccessMessage('Bilgilerin güncellendi.')
    } catch (err) {
      setFormError(translateApiError(err, 'Bilgiler güncellenemedi, tekrar deneyin.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handlePasswordSubmit(event: FormEvent) {
    event.preventDefault()
    setPasswordError(null)
    setPasswordSuccess(null)

    if (newPassword !== confirmNewPassword) {
      setPasswordError('Yeni şifreler eşleşmiyor.')
      return
    }

    setIsChangingPassword(true)
    try {
      await changePassword({ currentPassword, newPassword })
      setCurrentPassword('')
      setNewPassword('')
      setConfirmNewPassword('')
      setPasswordSuccess('Şifren güncellendi.')
    } catch (err) {
      setPasswordError(translateApiError(err, 'Şifre güncellenemedi, tekrar deneyin.'))
    } finally {
      setIsChangingPassword(false)
    }
  }

  async function handleLogout() {
    try {
      await logout()
    } catch {
      // ignore - clear local session regardless
    }
    setAccessToken(null)
    navigate('/')
  }

  async function handleAddAddress(event: FormEvent) {
    event.preventDefault()
    setAddressError(null)
    setIsSavingAddress(true)
    try {
      const address = await createAddress({
        title: newAddressTitle,
        city: newAddressCity,
        district: newAddressDistrict,
        fullAddress: newAddressFullAddress,
      })
      setAddresses((prev) => [...prev, address])
      setIsAddingAddress(false)
      setNewAddressTitle('')
      setNewAddressCity('')
      setNewAddressDistrict('')
      setNewAddressFullAddress('')
    } catch (err) {
      setAddressError(translateApiError(err, 'Adres eklenemedi.'))
    } finally {
      setIsSavingAddress(false)
    }
  }

  function handleStartEditAddress(address: Address) {
    setAddressError(null)
    setEditingAddressId(address.id)
    setEditingAddressTitle(address.title)
    setEditingAddressCity(address.city)
    setEditingAddressDistrict(address.district)
    setEditingAddressFullAddress(address.fullAddress)
  }

  async function handleSaveAddress(event: FormEvent) {
    event.preventDefault()
    if (!editingAddressId) return
    setAddressError(null)
    setIsSavingAddress(true)
    try {
      const updated = await updateAddress(editingAddressId, {
        title: editingAddressTitle,
        city: editingAddressCity,
        district: editingAddressDistrict,
        fullAddress: editingAddressFullAddress,
      })
      setAddresses((prev) => prev.map((a) => (a.id === updated.id ? updated : a)))
      setEditingAddressId(null)
    } catch (err) {
      setAddressError(translateApiError(err, 'Adres güncellenemedi.'))
    } finally {
      setIsSavingAddress(false)
    }
  }

  async function handleConfirmDeleteAddress() {
    if (!deleteAddressTarget) return
    setAddressError(null)
    setIsDeletingAddress(true)
    try {
      await deleteAddress(deleteAddressTarget.id)
      setAddresses((prev) => prev.filter((a) => a.id !== deleteAddressTarget.id))
      setDeleteAddressTarget(null)
    } catch (err) {
      setAddressError(translateApiError(err, 'Adres silinemedi.'))
      setDeleteAddressTarget(null)
    } finally {
      setIsDeletingAddress(false)
    }
  }

  return (
    <div className="min-h-screen">
      <Header />
      <main className="mx-auto max-w-3xl px-4 py-10">
        {isLoading && (
          <div className="mt-6 space-y-4">
            <Skeleton className="h-24 w-full rounded-lg" />
            <Skeleton className="h-11 w-full rounded-md" />
            <Skeleton className="h-11 w-full rounded-md" />
          </div>
        )}

        {!isLoading && loadError && <p className="mt-6 text-destructive">{loadError}</p>}

        {!isLoading && !loadError && user && (
          <>
            <div className="flex items-center justify-between">
              <h1 className="text-2xl font-semibold text-foreground">Hesabım</h1>
              <Button variant="outline" size="sm" onClick={handleLogout}>
                Çıkış yap
              </Button>
            </div>

            <Card className="mt-6 p-6">
              <p className="text-sm text-muted-foreground">Hoş geldin,</p>
              <p className="text-xl font-semibold text-foreground">{user.name}</p>
              <p className="mt-3 text-lg font-medium text-primary">Aramıza katılalı {daysSinceJoin} gün oldu</p>
              <p className="text-sm text-muted-foreground">
                Kayıt tarihi: {new Date(user.createdAt).toLocaleDateString('tr-TR')}
              </p>
            </Card>

            <div className="mt-8 flex gap-1 overflow-x-auto border-b border-border">
              {TABS.map((tab) => (
                <button
                  key={tab.id}
                  type="button"
                  onClick={() => setActiveTab(tab.id)}
                  className={`flex items-center gap-2 whitespace-nowrap border-b-2 px-4 py-3 text-sm font-medium transition-colors ${
                    activeTab === tab.id
                      ? 'border-primary text-primary'
                      : 'border-transparent text-muted-foreground hover:text-foreground'
                  }`}
                >
                  <tab.icon className="h-4 w-4" />
                  {tab.label}
                </button>
              ))}
            </div>

            {activeTab === 'profile' && (
              <form onSubmit={handleSubmit} className="mt-6 max-w-md space-y-4">
                <div>
                  <label htmlFor="name" className="mb-1 block text-sm font-medium text-muted-foreground">
                    Ad Soyad
                  </label>
                  <input
                    id="name"
                    type="text"
                    required
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    className={inputClasses}
                  />
                </div>

                <div>
                  <label htmlFor="email" className="mb-1 block text-sm font-medium text-muted-foreground">
                    E-posta
                  </label>
                  <input
                    id="email"
                    type="email"
                    required
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className={inputClasses}
                  />
                </div>

                {formError && <p className="text-sm text-destructive">{formError}</p>}
                {successMessage && <p className="text-sm text-green-600">{successMessage}</p>}

                <Button type="submit" disabled={isSubmitting} className="h-11 w-full">
                  {isSubmitting ? 'Kaydediliyor...' : 'Bilgileri Güncelle'}
                </Button>
              </form>
            )}

            {activeTab === 'security' && (
              <form onSubmit={handlePasswordSubmit} className="mt-6 max-w-md space-y-4">
                <div>
                  <label htmlFor="currentPassword" className="mb-1 block text-sm font-medium text-muted-foreground">
                    Mevcut şifre
                  </label>
                  <input
                    id="currentPassword"
                    type="password"
                    autoComplete="current-password"
                    required
                    value={currentPassword}
                    onChange={(e) => setCurrentPassword(e.target.value)}
                    className={inputClasses}
                  />
                </div>

                <div>
                  <label htmlFor="newPassword" className="mb-1 block text-sm font-medium text-muted-foreground">
                    Yeni şifre
                  </label>
                  <input
                    id="newPassword"
                    type="password"
                    autoComplete="new-password"
                    required
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    className={inputClasses}
                  />
                </div>

                <div>
                  <label
                    htmlFor="confirmNewPassword"
                    className="mb-1 block text-sm font-medium text-muted-foreground"
                  >
                    Yeni şifre (tekrar)
                  </label>
                  <input
                    id="confirmNewPassword"
                    type="password"
                    autoComplete="new-password"
                    required
                    value={confirmNewPassword}
                    onChange={(e) => setConfirmNewPassword(e.target.value)}
                    className={inputClasses}
                  />
                </div>

                {passwordError && <p className="text-sm text-destructive">{passwordError}</p>}
                {passwordSuccess && <p className="text-sm text-green-600">{passwordSuccess}</p>}

                <Button type="submit" disabled={isChangingPassword} className="h-11 w-full">
                  {isChangingPassword ? 'Güncelleniyor...' : 'Şifreyi Güncelle'}
                </Button>
              </form>
            )}

            {activeTab === 'orders' && (
              <div className="mt-6 space-y-4">
                {isLoadingOrders && (
                  <>
                    <Skeleton className="h-32 w-full rounded-lg" />
                    <Skeleton className="h-32 w-full rounded-lg" />
                  </>
                )}

                {!isLoadingOrders && ordersError && <p className="text-destructive">{ordersError}</p>}

                {!isLoadingOrders && !ordersError && orders.length === 0 && (
                  <p className="text-muted-foreground">Henüz siparişin yok.</p>
                )}

                {!isLoadingOrders &&
                  !ordersError &&
                  orders.map((order) => {
                    const statusInfo = orderStatusInfo(order.status)
                    return (
                      <Card key={order.id} className="p-4">
                        <div className="flex items-center justify-between">
                          <div>
                            <p className="font-medium text-foreground">Sipariş #{order.id.slice(0, 8)}</p>
                            <p className="text-sm text-muted-foreground">
                              {new Date(order.createdAt).toLocaleDateString('tr-TR')}
                            </p>
                          </div>
                          <span className={`rounded-full px-3 py-1 text-xs font-medium ${statusInfo.className}`}>
                            {statusInfo.label}
                          </span>
                        </div>

                        <ul className="mt-3 space-y-1 text-sm text-muted-foreground">
                          {order.items.map((item) => (
                            <li key={item.productId} className="flex justify-between">
                              <span>
                                {item.productName} × {item.quantity}
                              </span>
                              <span>{formatPrice(item.subtotal)}</span>
                            </li>
                          ))}
                        </ul>

                        <OrderTracking steps={buildOrderTrackingSteps(order)} className="mt-4 border-t border-border pt-4" />

                        <div className="mt-3 flex items-center justify-between border-t border-border pt-3">
                          <span className="text-sm text-muted-foreground">
                            {order.shippingTitle} · {order.shippingFullAddress}, {order.shippingDistrict}/
                            {order.shippingCity}
                          </span>
                          <span className="font-semibold text-foreground">{formatPrice(order.totalAmount)}</span>
                        </div>
                      </Card>
                    )
                  })}
              </div>
            )}

            {activeTab === 'addresses' && (
              <div className="mt-6 max-w-md">
                {isLoadingAddresses && (
                  <div className="space-y-3">
                    <Skeleton className="h-16 w-full rounded-lg" />
                    <Skeleton className="h-16 w-full rounded-lg" />
                  </div>
                )}

                {!isLoadingAddresses && (
                  <div className="space-y-3">
                    {addresses.length === 0 && !isAddingAddress && (
                      <p className="text-muted-foreground">Henüz kayıtlı adresin yok.</p>
                    )}

                    {addresses.map((address) =>
                      editingAddressId === address.id ? (
                        <form
                          key={address.id}
                          onSubmit={handleSaveAddress}
                          className="space-y-3 rounded-lg border border-border p-4"
                        >
                          <input
                            type="text"
                            required
                            placeholder="Başlık (ör. Ev)"
                            value={editingAddressTitle}
                            onChange={(e) => setEditingAddressTitle(e.target.value)}
                            className={inputClasses}
                          />
                          <div className="flex gap-3">
                            <input
                              type="text"
                              required
                              placeholder="İl"
                              value={editingAddressCity}
                              onChange={(e) => setEditingAddressCity(e.target.value)}
                              className={inputClasses}
                            />
                            <input
                              type="text"
                              required
                              placeholder="İlçe"
                              value={editingAddressDistrict}
                              onChange={(e) => setEditingAddressDistrict(e.target.value)}
                              className={inputClasses}
                            />
                          </div>
                          <textarea
                            required
                            placeholder="Açık adres"
                            value={editingAddressFullAddress}
                            onChange={(e) => setEditingAddressFullAddress(e.target.value)}
                            className={`${inputClasses} h-20 resize-none py-2`}
                          />
                          <div className="flex items-center gap-2">
                            <Button type="submit" size="sm" disabled={isSavingAddress}>
                              {isSavingAddress ? 'Kaydediliyor...' : 'Kaydet'}
                            </Button>
                            <Button
                              type="button"
                              size="sm"
                              variant="outline"
                              onClick={() => setEditingAddressId(null)}
                            >
                              Vazgeç
                            </Button>
                          </div>
                        </form>
                      ) : (
                        <div
                          key={address.id}
                          className="flex items-start justify-between gap-3 rounded-lg border border-border p-4"
                        >
                          <div>
                            <p className="font-medium text-foreground">{address.title}</p>
                            <p className="text-sm text-muted-foreground">
                              {address.fullAddress}, {address.district}/{address.city}
                            </p>
                          </div>
                          <div className="flex shrink-0 items-center gap-2">
                            <button
                              type="button"
                              onClick={() => handleStartEditAddress(address)}
                              className="text-muted-foreground hover:text-foreground"
                              aria-label="Düzenle"
                            >
                              <Pencil className="h-4 w-4" />
                            </button>
                            <button
                              type="button"
                              onClick={() => setDeleteAddressTarget(address)}
                              className="text-muted-foreground hover:text-destructive"
                              aria-label="Sil"
                            >
                              <Trash2 className="h-4 w-4" />
                            </button>
                          </div>
                        </div>
                      ),
                    )}

                    {!isAddingAddress && (
                      <button
                        type="button"
                        onClick={() => setIsAddingAddress(true)}
                        className="flex items-center gap-1 text-sm text-primary hover:underline"
                      >
                        <Plus className="h-4 w-4" /> Yeni adres ekle
                      </button>
                    )}

                    {isAddingAddress && (
                      <form onSubmit={handleAddAddress} className="space-y-3 rounded-lg border border-border p-4">
                        <input
                          type="text"
                          required
                          placeholder="Başlık (ör. Ev)"
                          value={newAddressTitle}
                          onChange={(e) => setNewAddressTitle(e.target.value)}
                          className={inputClasses}
                        />
                        <div className="flex gap-3">
                          <input
                            type="text"
                            required
                            placeholder="İl"
                            value={newAddressCity}
                            onChange={(e) => setNewAddressCity(e.target.value)}
                            className={inputClasses}
                          />
                          <input
                            type="text"
                            required
                            placeholder="İlçe"
                            value={newAddressDistrict}
                            onChange={(e) => setNewAddressDistrict(e.target.value)}
                            className={inputClasses}
                          />
                        </div>
                        <textarea
                          required
                          placeholder="Açık adres"
                          value={newAddressFullAddress}
                          onChange={(e) => setNewAddressFullAddress(e.target.value)}
                          className={`${inputClasses} h-20 resize-none py-2`}
                        />
                        <div className="flex items-center gap-2">
                          <Button type="submit" size="sm" disabled={isSavingAddress}>
                            {isSavingAddress ? 'Kaydediliyor...' : 'Adresi kaydet'}
                          </Button>
                          <Button
                            type="button"
                            size="sm"
                            variant="outline"
                            onClick={() => setIsAddingAddress(false)}
                          >
                            Vazgeç
                          </Button>
                        </div>
                      </form>
                    )}
                  </div>
                )}

                {addressError && <p className="mt-3 text-sm text-destructive">{addressError}</p>}
              </div>
            )}

            {activeTab === 'payments' && (
              <div className="mt-6 max-w-md space-y-3">
                {isLoadingPayments && (
                  <>
                    <Skeleton className="h-16 w-full rounded-lg" />
                    <Skeleton className="h-16 w-full rounded-lg" />
                  </>
                )}

                {!isLoadingPayments && paymentsError && <p className="text-destructive">{paymentsError}</p>}

                {!isLoadingPayments && !paymentsError && payments.length === 0 && (
                  <p className="text-muted-foreground">Henüz ödeme geçmişin yok.</p>
                )}

                {!isLoadingPayments &&
                  !paymentsError &&
                  payments.map((payment) => (
                    <Card key={payment.id} className="flex items-center justify-between p-4">
                      <div>
                        <p className="font-medium text-foreground">{payment.maskedCardNumber}</p>
                        <p className="text-sm text-muted-foreground">
                          {new Date(payment.createdAt).toLocaleString('tr-TR')}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="font-semibold text-foreground">{formatPrice(payment.amount)}</p>
                        <p
                          className={`text-sm ${
                            payment.status === 'Succeeded' ? 'text-green-600' : 'text-destructive'
                          }`}
                        >
                          {payment.status === 'Succeeded' ? 'Başarılı' : 'Başarısız'}
                        </p>
                      </div>
                    </Card>
                  ))}
              </div>
            )}
          </>
        )}
      </main>

      <ConfirmModal
        isOpen={deleteAddressTarget !== null}
        title="Adresi sil"
        message={`"${deleteAddressTarget?.title}" adresini silmek istediğine emin misin?`}
        confirmText={isDeletingAddress ? 'Siliniyor...' : 'Sil'}
        isDestructive
        onConfirm={handleConfirmDeleteAddress}
        onCancel={() => setDeleteAddressTarget(null)}
      />
    </div>
  )
}
