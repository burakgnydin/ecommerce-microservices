import { motion } from 'framer-motion'
import { Minus, Plus, Trash2 } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getProductById } from '../api/products'
import { ApiError } from '../api/client'
import type { Order, Product } from '../api/types'
import { Header } from '../components/Header'
import { ProductImage } from '../components/ProductImage'
import { Skeleton } from '../components/ui/Skeleton'
import { Button } from '../components/ui/Button'
import { getAccessToken } from '../lib/auth'
import { useCart } from '../context/CartContext'

function formatPrice(price: number) {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(price)
}

export default function CartPage() {
  const { cart, isLoading, error, updateQuantity, removeItem, checkout } = useCart()
  const [products, setProducts] = useState<Record<string, Product>>({})
  const [isEnriching, setIsEnriching] = useState(false)
  const [order, setOrder] = useState<Order | null>(null)
  const [checkoutError, setCheckoutError] = useState<string | null>(null)
  const [isCheckingOut, setIsCheckingOut] = useState(false)

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

  async function handleCheckout() {
    setCheckoutError(null)
    setIsCheckingOut(true)
    try {
      setOrder(await checkout())
    } catch (err) {
      setCheckoutError(err instanceof ApiError ? err.message : 'Sepet onaylanamadı.')
    } finally {
      setIsCheckingOut(false)
    }
  }

  if (order) {
    return (
      <div className="min-h-screen">
        <Header />
        <main className="mx-auto max-w-2xl px-4 py-16 text-center">
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3, ease: 'easeOut' }}
            className="rounded-xl border border-border bg-card p-8"
          >
            <h1 className="text-2xl font-semibold text-foreground">Siparişin oluşturuldu</h1>
            <p className="mt-2 text-sm text-muted-foreground">Sipariş No: {order.id}</p>
            <p className="mt-1 text-sm text-muted-foreground">Durum: Beklemede</p>
            <p className="mt-4 text-lg font-bold text-foreground">{formatPrice(order.totalAmount)}</p>
            <Link to="/products" className="mt-6 inline-block">
              <Button>Alışverişe devam et</Button>
            </Link>
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

        {!isLoggedIn && (
          <p className="mt-6 text-muted-foreground">
            Sepetini görmek için{' '}
            <Link to="/login" className="text-primary hover:underline">
              giriş yapmalısın
            </Link>
            .
          </p>
        )}

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

        {isLoggedIn && !error && !isLoading && cart && cart.items.length > 0 && (
          <>
            <div className="mt-6 divide-y divide-border">
              {cart.items.map((item) => {
                const product = products[item.productId]
                return (
                  <div key={item.productId} className="flex items-center gap-4 py-4">
                    {product ? (
                      <ProductImage product={product} className="h-16 w-16 rounded-lg" iconClassName="h-6 w-6" />
                    ) : (
                      <Skeleton className="h-16 w-16 rounded-lg" />
                    )}

                    <div className="flex-1">
                      {product ? (
                        <>
                          <p className="font-medium text-foreground">{product.name}</p>
                          <p className="text-sm text-muted-foreground">{formatPrice(product.price)}</p>
                        </>
                      ) : (
                        <Skeleton className="h-5 w-32" />
                      )}
                    </div>

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

                    <p className="w-24 text-right font-medium text-foreground">
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
              <Button onClick={handleCheckout} disabled={isCheckingOut || isEnriching}>
                {isCheckingOut ? 'İşleniyor...' : 'Sepeti Onayla'}
              </Button>
            </div>

            {checkoutError && <p className="mt-3 text-destructive">{checkoutError}</p>}
          </>
        )}
      </main>
    </div>
  )
}
