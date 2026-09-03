import { motion } from 'framer-motion'
import { Minus, Plus, Trash2 } from 'lucide-react'
import { type FormEvent, useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { createAddress, getAddresses } from '../api/addresses'
import { ApiError, translateApiError } from '../api/client'
import { getProductById } from '../api/products'
import type { Address, Order, Payment, Product } from '../api/types'
import { CardPaymentForm } from '../components/CardPaymentForm'
import { Header } from '../components/Header'
import { ProductImage } from '../components/ProductImage'
import { AuthPrompt } from '../components/ui/AuthPrompt'
import { Skeleton } from '../components/ui/Skeleton'
import { Button } from '../components/ui/Button'
import { OrderConfirmationCard } from '../components/ui/OrderConfirmationCard'
import { getAccessToken } from '../lib/auth'
import { useCart } from '../context/CartContext'

function formatPrice(price: number) {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(price)
}

const inputClasses =
  'h-10 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground outline-none focus-visible:ring-2 focus-visible:ring-ring'

export default function CartPage() {
  const { cart, isLoading, error, updateQuantity, removeItem, checkout, clearCart } = useCart()
  const navigate = useNavigate()
  const [products, setProducts] = useState<Record<string, Product>>({})
  const [isEnriching, setIsEnriching] = useState(false)
  const [order, setOrder] = useState<Order | null>(null)
  const [payment, setPayment] = useState<Payment | null>(null)
  const [checkoutError, setCheckoutError] = useState<string | null>(null)
  const [isCheckingOut, setIsCheckingOut] = useState(false)

  const [step, setStep] = useState<'cart' | 'address'>('cart')
  const [addresses, setAddresses] = useState<Address[]>([])
  const [isLoadingAddresses, setIsLoadingAddresses] = useState(false)
  const [addressError, setAddressError] = useState<string | null>(null)
  const [selectedAddressId, setSelectedAddressId] = useState<string | null>(null)

  const [isAddingAddress, setIsAddingAddress] = useState(false)
  const [newAddressTitle, setNewAddressTitle] = useState('')
  const [newAddressCity, setNewAddressCity] = useState('')
  const [newAddressDistrict, setNewAddressDistrict] = useState('')
  const [newAddressFullAddress, setNewAddressFullAddress] = useState('')
  const [isSavingAddress, setIsSavingAddress] = useState(false)

  const isLoggedIn = Boolean(getAccessToken())

  useEffect(() => {
    if (!cart || cart.items.length === 0) return
    const missing = cart.items.filter((item) => !products[item.productId])
    if (missing.length === 0) return

    setIsEnriching(true)
    Promise.all(missing.map((item) => getProductById(item.productId)))
      .then((fetched) => {
        setProducts((prev) => {
          const next = { ...prev }
          for (const product of fetched) next[product.id] = product
          return next
        })
      })
      .catch(() => {})
      .finally(() => setIsEnriching(false))
  }, [cart, products])

  function handleGoToAddressStep() {
    setCheckoutError(null)
    setStep('address')
    if (addresses.length === 0) {
      setIsLoadingAddresses(true)
      setAddressError(null)
      getAddresses()
        .then((result) => {
          setAddresses(result)
          if (result.length > 0) setSelectedAddressId(result[0].id)
        })
        .catch((err) => setAddressError(translateApiError(err, 'Adresler yüklenemedi.')))
        .finally(() => setIsLoadingAddresses(false))
    }
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
      setSelectedAddressId(address.id)
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

  async function handleCheckout() {
    const address = addresses.find((a) => a.id === selectedAddressId)
    if (!address) return
    setCheckoutError(null)
    setIsCheckingOut(true)
    try {
      setOrder(
        await checkout({
          shippingTitle: address.title,
          shippingCity: address.city,
          shippingDistrict: address.district,
          shippingFullAddress: address.fullAddress,
        }),
      )
    } catch (err) {
      setCheckoutError(err instanceof ApiError ? err.message : 'Sepet onaylanamadı.')
    } finally {
      setIsCheckingOut(false)
    }
  }

  function handlePaymentSuccess(paymentResult: Payment) {
    clearCart()
    setPayment(paymentResult)
  }

  if (order && payment) {
    return (
      <div className="min-h-screen">
        <Header />
        <main className="mx-auto flex max-w-2xl justify-center px-4 py-16">
          <OrderConfirmationCard
            orderId={payment.orderId}
            paymentMethod={payment.maskedCardNumber}
            dateTime={new Date(payment.createdAt).toLocaleString('tr-TR')}
            totalAmount={formatPrice(payment.amount)}
            buttonText="Alışverişe devam et"
            onGoToAccount={() => navigate('/products')}
          />
        </main>
      </div>
    )
  }

  if (order) {
    return (
      <div className="min-h-screen">
        <Header />
        <main className="mx-auto max-w-md px-4 py-16">
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3, ease: 'easeOut' }}
            className="rounded-xl border border-border bg-card p-8"
          >
            <h1 className="text-2xl font-semibold text-foreground">Ödemeni tamamla</h1>
            <p className="mt-2 text-sm text-muted-foreground">Sipariş No: {order.id}</p>
            <p className="mt-4 text-lg font-bold text-foreground">{formatPrice(order.totalAmount)}</p>
            <div className="mt-6">
              <CardPaymentForm orderId={order.id} onSuccess={handlePaymentSuccess} />
            </div>
          </motion.div>
        </main>
      </div>
    )
  }

  return (
    <div className="min-h-screen">
      <Header />
      <main className="mx-auto max-w-3xl px-4 py-10">
        <h1 className="text-2xl font-semibold text-foreground">Sepetim</h1>

        {!isLoggedIn && <AuthPrompt className="mt-6" message="Sepetini görmek için giriş yapmalısın." />}

        {isLoggedIn && error && <p className="mt-6 text-destructive">{error}</p>}

        {isLoggedIn && !error && isLoading && (
          <div className="mt-6 space-y-4">
            <Skeleton className="h-24 w-full rounded-lg" />
            <Skeleton className="h-24 w-full rounded-lg" />
          </div>
        )}

        {isLoggedIn && !error && !isLoading && cart && cart.items.length === 0 && (
          <p className="mt-6 text-muted-foreground">
            Sepetin boş.{' '}
            <Link to="/products" className="text-primary hover:underline">
              Ürünlere göz at
            </Link>
            .
          </p>
        )}

        {isLoggedIn && !error && !isLoading && cart && cart.items.length > 0 && step === 'cart' && (
          <>
            <div className="mt-6 divide-y divide-border">
              {cart.items.map((item) => {
                const product = products[item.productId]
                return (
                  <div key={item.productId} className="flex flex-wrap items-center gap-4 py-4">
                    <Link to={`/products/${item.productId}`} className="flex min-w-[160px] flex-1 items-center gap-4">
                      {product ? (
                        <ProductImage product={product} className="h-16 w-16 rounded-lg" iconClassName="h-6 w-6" />
                      ) : (
                        <Skeleton className="h-16 w-16 rounded-lg" />
                      )}

                      <div className="flex-1">
                        {product ? (
                          <>
                            <p className="font-medium text-foreground hover:underline">{product.name}</p>
                            <p className="text-sm text-muted-foreground">{formatPrice(product.price)}</p>
                          </>
                        ) : (
                          <Skeleton className="h-5 w-32" />
                        )}
                      </div>
                    </Link>

                    <div className="flex items-center gap-4">
                      <div className="flex items-center gap-2">
                        <button
                          type="button"
                          onClick={() =>
                            item.quantity > 1
                              ? updateQuantity(item.productId, item.quantity - 1)
                              : removeItem(item.productId)
                          }
                          className="flex h-7 w-7 items-center justify-center rounded-md border border-border text-foreground hover:bg-secondary"
                          aria-label="Azalt"
                        >
                          <Minus className="h-3.5 w-3.5" />
                        </button>
                        <span className="w-6 text-center text-sm font-medium text-foreground">{item.quantity}</span>
                        <button
                          type="button"
                          onClick={() => updateQuantity(item.productId, item.quantity + 1)}
                          className="flex h-7 w-7 items-center justify-center rounded-md border border-border text-foreground hover:bg-secondary"
                          aria-label="Artır"
                        >
                          <Plus className="h-3.5 w-3.5" />
                        </button>
                      </div>

                      <p className="w-20 text-right font-medium text-foreground">
                        {product ? formatPrice(product.price * item.quantity) : ''}
                      </p>

                      <button
                        type="button"
                        onClick={() => removeItem(item.productId)}
                        className="text-muted-foreground hover:text-destructive"
                        aria-label="Kaldır"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  </div>
                )
              })}
            </div>

            <div className="mt-6 flex items-center justify-between border-t border-border pt-6">
              <span className="text-lg font-semibold text-foreground">
                Toplam:{' '}
                {formatPrice(
                  cart.items.reduce((sum, item) => sum + (products[item.productId]?.price ?? 0) * item.quantity, 0),
                )}
              </span>
              <Button onClick={handleGoToAddressStep} disabled={isEnriching}>
                Sepeti Onayla
              </Button>
            </div>
          </>
        )}

        {isLoggedIn && !error && !isLoading && cart && cart.items.length > 0 && step === 'address' && (
          <div className="mt-6">
            <button
              type="button"
              onClick={() => setStep('cart')}
              className="text-sm text-muted-foreground hover:text-foreground"
            >
              ← Sepete dön
            </button>

            <h2 className="mt-4 text-lg font-semibold text-foreground">Teslimat adresi</h2>

            {isLoadingAddresses && (
              <div className="mt-4 space-y-3">
                <Skeleton className="h-16 w-full rounded-lg" />
                <Skeleton className="h-16 w-full rounded-lg" />
              </div>
            )}

            {!isLoadingAddresses && (
              <div className="mt-4 space-y-3">
                {addresses.map((address) => (
                  <label
                    key={address.id}
                    className={`flex cursor-pointer items-start gap-3 rounded-lg border p-4 ${
                      selectedAddressId === address.id ? 'border-primary bg-primary/5' : 'border-border'
                    }`}
                  >
                    <input
                      type="radio"
                      name="address"
                      className="mt-1"
                      checked={selectedAddressId === address.id}
                      onChange={() => setSelectedAddressId(address.id)}
                    />
                    <div>
                      <p className="font-medium text-foreground">{address.title}</p>
                      <p className="text-sm text-muted-foreground">
                        {address.fullAddress}, {address.district}/{address.city}
                      </p>
                    </div>
                  </label>
                ))}

                {!isAddingAddress && (
                  <button
                    type="button"
                    onClick={() => setIsAddingAddress(true)}
                    className="text-sm text-primary hover:underline"
                  >
                    + Yeni adres ekle
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
                      <Button type="button" size="sm" variant="outline" onClick={() => setIsAddingAddress(false)}>
                        Vazgeç
                      </Button>
                    </div>
                  </form>
                )}
              </div>
            )}

            {addressError && <p className="mt-3 text-destructive">{addressError}</p>}

            <div className="mt-6 flex items-center justify-between border-t border-border pt-6">
              <span className="text-lg font-semibold text-foreground">
                Toplam:{' '}
                {formatPrice(
                  cart.items.reduce((sum, item) => sum + (products[item.productId]?.price ?? 0) * item.quantity, 0),
                )}
              </span>
              <Button onClick={handleCheckout} disabled={!selectedAddressId || isCheckingOut}>
                {isCheckingOut ? 'İşleniyor...' : 'Ödemeye Geç'}
              </Button>
            </div>

            {checkoutError && <p className="mt-3 text-destructive">{checkoutError}</p>}
          </div>
        )}
      </main>
    </div>
  )
}
