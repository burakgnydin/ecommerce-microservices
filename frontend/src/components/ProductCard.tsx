import { motion, useReducedMotion } from 'framer-motion'
import { Minus, Plus } from 'lucide-react'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { ApiError } from '../api/client'
import type { Product } from '../api/types'
import { useCart } from '../context/CartContext'
import { AuthPrompt } from './ui/AuthPrompt'
import { Button } from './ui/Button'
import { ProductImage } from './ProductImage'

interface ProductCardProps {
  product: Product
  isAdmin?: boolean
}

function formatPrice(price: number) {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(price)
}

const cardVariants = {
  rest: { y: 0 },
  hover: { y: -6 },
}

const imageVariants = {
  rest: { scale: 1 },
  hover: { scale: 1.08 },
}

export function ProductCard({ product, isAdmin = false }: ProductCardProps) {
  const shouldReduceMotion = useReducedMotion()
  const isOutOfStock = product.stock <= 0 && !product.allowsPreOrder
  const isPreOrder = product.stock <= 0 && product.allowsPreOrder
  const { addItem } = useCart()
  const [quantity, setQuantity] = useState(1)
  const [isAdding, setIsAdding] = useState(false)
  const [added, setAdded] = useState(false)
  const [addError, setAddError] = useState<string | null>(null)

  async function handleAddToCart(event: React.MouseEvent) {
    event.preventDefault()
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

  return (
    <Link to={`/products/${product.id}`} className="block h-full">
      <motion.div
        initial="rest"
        whileHover="hover"
        variants={shouldReduceMotion ? undefined : cardVariants}
        transition={{ type: 'spring', stiffness: 300, damping: 25 }}
        className="group relative flex h-full flex-col overflow-hidden rounded-xl border border-border bg-card text-card-foreground shadow-sm transition-shadow hover:shadow-lg"
        whileTap={{ scale: 0.99 }}
      >
        <div className="relative h-40 overflow-hidden">
          <motion.div
            className="h-full w-full"
            variants={shouldReduceMotion ? undefined : imageVariants}
            transition={{ type: 'spring', stiffness: 300, damping: 30 }}
          >
            <ProductImage product={product} className="h-full w-full" />
          </motion.div>
          {product.categoryName && (
            <span className="absolute left-3 top-3 rounded-full bg-background/90 px-2.5 py-0.5 text-xs font-medium text-foreground shadow-sm backdrop-blur-sm">
              {product.categoryName}
            </span>
          )}
        </div>

        <div className="flex flex-1 flex-col p-5">
          <h3 className="font-semibold text-foreground">{product.name}</h3>
          {product.description && (
            <p className="mt-1 line-clamp-2 text-sm text-muted-foreground">{product.description}</p>
          )}
          <div className="mt-auto flex items-center justify-between pt-4">
            <span className="text-lg font-bold text-foreground">{formatPrice(product.price)}</span>
            {isOutOfStock ? (
              <span className="text-xs font-medium text-destructive">Stokta yok</span>
            ) : isPreOrder ? (
              <span className="text-xs font-medium text-primary">Ön sipariş</span>
            ) : (
              <span className="text-xs text-muted-foreground">{product.stock} adet</span>
            )}
          </div>
        </div>

        {!isAdmin && (product.stock > 0 || product.allowsPreOrder) && (
          <div className="border-t border-border bg-card p-3">
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={(event) => {
                  event.preventDefault()
                  setQuantity((q) => Math.max(1, q - 1))
                }}
                className="flex h-8 w-8 shrink-0 items-center justify-center rounded-md border border-border text-foreground hover:bg-secondary"
                aria-label="Azalt"
              >
                <Minus className="h-3.5 w-3.5" />
              </button>
              <span className="w-6 shrink-0 text-center text-sm font-medium text-foreground">{quantity}</span>
              <button
                type="button"
                onClick={(event) => {
                  event.preventDefault()
                  setQuantity((q) => q + 1)
                }}
                className="flex h-8 w-8 shrink-0 items-center justify-center rounded-md border border-border text-foreground hover:bg-secondary"
                aria-label="Artır"
              >
                <Plus className="h-3.5 w-3.5" />
              </button>
              <Button type="button" size="sm" className="flex-1" onClick={handleAddToCart} disabled={isAdding}>
                {isAdding ? 'Ekleniyor...' : 'Sepete Ekle'}
              </Button>
            </div>
            {added && <p className="mt-2 text-xs text-success">Sepete eklendi.</p>}
            {addError === 'login' && (
              <AuthPrompt variant="inline" className="mt-2" message="Sepete eklemek için giriş yapmalısın." />
            )}
            {addError && addError !== 'login' && <p className="mt-2 text-xs text-destructive">{addError}</p>}
          </div>
        )}
      </motion.div>
    </Link>
  )
}
