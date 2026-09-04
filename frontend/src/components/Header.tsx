import { AnimatePresence, motion } from 'framer-motion'
import { LogOut, ShoppingCart } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { logout } from '../api/auth'
import { getCategories } from '../api/categories'
import { getProductById } from '../api/products'
import type { Category, Product, UserResponse } from '../api/types'
import { getMe } from '../api/users'
import { useCart } from '../context/CartContext'
import { getAccessToken, setAccessToken } from '../lib/auth'
import { Button } from './ui/Button'
import { ConfirmModal } from './ui/ConfirmModal'
import { ProductImage } from './ProductImage'
import { InteractiveHoverButton } from './ui/InteractiveHoverButton'
import { Skeleton } from './ui/Skeleton'

function formatPrice(price: number) {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(price)
}

export function useCurrentUser() {
  const [user, setUser] = useState<UserResponse | null>(null)
  const isLoggedIn = Boolean(getAccessToken())

  useEffect(() => {
    if (!isLoggedIn) {
      setUser(null)
      return
    }
    getMe()
      .then(setUser)
      .catch(() => setUser(null))
  }, [isLoggedIn])

  return { user, isLoggedIn }
}

function CategoriesDropdown() {
  const [isOpen, setIsOpen] = useState(false)
  const [categories, setCategories] = useState<Category[]>([])

  useEffect(() => {
    if (isOpen && categories.length === 0) {
      getCategories()
        .then(setCategories)
        .catch(() => setCategories([]))
    }
  }, [isOpen, categories.length])

  return (
    <div className="relative" onMouseEnter={() => setIsOpen(true)} onMouseLeave={() => setIsOpen(false)}>
      <Link to="/categories" className="text-muted-foreground hover:text-foreground">
        Kategoriler
      </Link>
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: -8 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -8 }}
            transition={{ duration: 0.15, ease: 'easeOut' }}
            className="absolute left-0 top-full z-20 mt-2 w-56 rounded-lg border border-border bg-card p-2 text-card-foreground shadow-md"
          >
            {categories.length === 0 && (
              <p className="px-3 py-2 text-sm text-muted-foreground">Yükleniyor...</p>
            )}
            {categories.map((category) => (
              <Link
                key={category.id}
                to={`/products?categoryId=${category.id}&categoryName=${encodeURIComponent(category.name)}`}
                className="block rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-primary/10 hover:text-foreground"
              >
                {category.name}
              </Link>
            ))}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  )
}

function AuthMenu({ user, isLoggedIn }: { user: UserResponse | null; isLoggedIn: boolean }) {
  const [isOpen, setIsOpen] = useState(false)
  const [showLogoutConfirm, setShowLogoutConfirm] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)
  const navigate = useNavigate()
  const { clearCart } = useCart()

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  if (isLoggedIn) {
    const isAdmin = user?.role === 'Admin'
    const accountPath = isAdmin ? '/admin' : '/account'
    const buttonText = isAdmin ? 'Panel' : 'Hesabım'

    async function handleLogout() {
      try {
        await logout()
      } catch {
        // ignore - clear local session regardless
      }
      setAccessToken(null)
      clearCart()
      setShowLogoutConfirm(false)
      setIsOpen(false)
      navigate('/')
    }

    return (
      <div className="relative" ref={containerRef}>
        <InteractiveHoverButton text={buttonText} onClick={() => setIsOpen((prev) => !prev)} />
        <AnimatePresence>
          {isOpen && (
            <motion.div
              initial={{ opacity: 0, y: -8 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -8 }}
              transition={{ duration: 0.15, ease: 'easeOut' }}
              className="absolute right-0 top-full z-20 mt-2 w-48 rounded-lg border border-border bg-card p-2 text-card-foreground shadow-md"
            >
              <Link
                to={accountPath}
                onClick={() => setIsOpen(false)}
                className="block rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-primary/10 hover:text-foreground"
              >
                {buttonText}
              </Link>
              <button
                type="button"
                onClick={() => setShowLogoutConfirm(true)}
                className="flex w-full items-center gap-2 rounded-md px-3 py-2 text-left text-sm text-muted-foreground hover:bg-primary/10 hover:text-foreground"
              >
                <LogOut className="h-4 w-4" /> Çıkış yap
              </button>
            </motion.div>
          )}
        </AnimatePresence>

        <ConfirmModal
          isOpen={showLogoutConfirm}
          title="Çıkış yap"
          message="Çıkış yapmak istediğine emin misin?"
          confirmText="Çıkış yap"
          isDestructive
          onConfirm={handleLogout}
          onCancel={() => setShowLogoutConfirm(false)}
        />
      </div>
    )
  }

  return (
    <div className="relative" ref={containerRef}>
      <InteractiveHoverButton text="Hesabım" onClick={() => setIsOpen((prev) => !prev)} />
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: -8 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -8 }}
            transition={{ duration: 0.15, ease: 'easeOut' }}
            className="absolute right-0 top-full z-20 mt-2 w-48 rounded-lg border border-border bg-card p-2 text-card-foreground shadow-md"
          >
            <Link
              to="/login"
              onClick={() => setIsOpen(false)}
              className="block rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-primary/10 hover:text-foreground"
            >
              Giriş yap
            </Link>
            <Link
              to="/register"
              onClick={() => setIsOpen(false)}
              className="block rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-primary/10 hover:text-foreground"
            >
              Kayıt ol
            </Link>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  )
}

function CartPreview() {
  const { cart, itemCount } = useCart()
  const [isOpen, setIsOpen] = useState(false)
  const [products, setProducts] = useState<Record<string, Product>>({})
  const navigate = useNavigate()

  useEffect(() => {
    if (!isOpen || !cart || cart.items.length === 0) return
    const missing = cart.items.filter((item) => !products[item.productId])
    if (missing.length === 0) return

    Promise.all(missing.map((item) => getProductById(item.productId)))
      .then((fetched) => {
        setProducts((prev) => {
          const next = { ...prev }
          for (const product of fetched) next[product.id] = product
          return next
        })
      })
      .catch(() => {})
  }, [isOpen, cart, products])

  const total =
    cart?.items.reduce((sum, item) => sum + (products[item.productId]?.price ?? 0) * item.quantity, 0) ?? 0

  return (
    <div className="relative" onMouseEnter={() => setIsOpen(true)} onMouseLeave={() => setIsOpen(false)}>
      <Link to="/cart" className="relative text-muted-foreground hover:text-foreground" aria-label="Sepetim">
        <ShoppingCart className="h-5 w-5" />
        {itemCount > 0 && (
          <span className="absolute -right-2 -top-2 flex h-4 w-4 items-center justify-center rounded-full bg-primary text-[10px] font-medium text-primary-foreground">
            {itemCount}
          </span>
        )}
      </Link>
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: -8 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -8 }}
            transition={{ duration: 0.15, ease: 'easeOut' }}
            className="absolute right-0 top-full z-20 mt-2 w-72 rounded-lg border border-border bg-card p-3 text-card-foreground shadow-md"
          >
            {!cart || cart.items.length === 0 ? (
              <p className="px-2 py-4 text-center text-sm text-muted-foreground">Sepetin boş</p>
            ) : (
              <>
                <div className="max-h-64 space-y-3 overflow-y-auto">
                  {cart.items.map((item) => {
                    const product = products[item.productId]
                    return (
                      <Link key={item.productId} to={`/products/${item.productId}`} className="flex items-center gap-3">
                        {product ? (
                          <ProductImage product={product} className="h-10 w-10 rounded-md" iconClassName="h-4 w-4" />
                        ) : (
                          <Skeleton className="h-10 w-10 rounded-md" />
                        )}
                        <div className="min-w-0 flex-1">
                          {product ? (
                            <p className="truncate text-sm text-foreground hover:underline">{product.name}</p>
                          ) : (
                            <Skeleton className="h-4 w-24" />
                          )}
                          <p className="text-xs text-muted-foreground">{item.quantity} adet</p>
                        </div>
                        <span className="text-sm font-medium text-foreground">
                          {product ? formatPrice(product.price * item.quantity) : ''}
                        </span>
                      </Link>
                    )
                  })}
                </div>
                <div className="mt-3 flex items-center justify-between border-t border-border pt-3">
                  <span className="text-sm font-semibold text-foreground">Toplam:</span>
                  <span className="text-sm font-semibold text-foreground">{formatPrice(total)}</span>
                </div>
                <Button size="sm" className="mt-3 w-full" onClick={() => navigate('/cart')}>
                  Sepete git
                </Button>
              </>
            )}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  )
}

export function Header() {
  const { user, isLoggedIn } = useCurrentUser()
  const isAdmin = isLoggedIn && user?.role === 'Admin'

  return (
    <header className="relative z-[35]">
      <div className="flex w-full flex-wrap items-center justify-center gap-x-6 gap-y-3 px-6 py-5 sm:justify-between">
        <Link to="/" className="text-lg font-bold text-foreground">
          E-Ticaret
        </Link>
        <nav className="flex flex-wrap items-center justify-center gap-3 text-sm font-medium sm:gap-6">
          <Link to="/products" className="text-muted-foreground hover:text-foreground">
            Ürünler
          </Link>
          <CategoriesDropdown />
          {!isAdmin && <CartPreview />}
          <AuthMenu user={user} isLoggedIn={isLoggedIn} />
        </nav>
      </div>
    </header>
  )
}
