import { AnimatePresence, motion } from 'framer-motion'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import type { Product } from '../api/types'
import { ProductImage } from './ProductImage'

interface ProductMarqueeProps {
  products: Product[]
}

function formatPrice(price: number) {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(price)
}

function MarqueeItem({ product }: { product: Product }) {
  const [isHovered, setIsHovered] = useState(false)

  return (
    <div
      className="relative"
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      <Link
        to={`/products/${product.id}`}
        className="flex w-40 shrink-0 flex-col overflow-hidden rounded-lg border border-border bg-card"
      >
        <ProductImage product={product} className="h-24 w-full" iconClassName="h-6 w-6" />
        <span className="truncate p-2.5 text-xs font-medium text-foreground">{product.name}</span>
      </Link>
      <AnimatePresence>
        {isHovered && (
          <motion.div
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: 8 }}
            transition={{ duration: 0.15, ease: 'easeOut' }}
            className="absolute bottom-full left-0 z-20 mb-2 w-40 overflow-hidden rounded-lg border border-border bg-card shadow-md"
          >
            <Link to={`/products/${product.id}`} className="flex flex-col">
              <ProductImage product={product} className="h-32 w-full" iconClassName="h-8 w-8" />
              <div className="p-2.5">
                <p className="truncate text-xs font-medium text-foreground">{product.name}</p>
                <p className="mt-1 text-xs font-semibold text-foreground">{formatPrice(product.price)}</p>
              </div>
            </Link>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  )
}

export function ProductMarquee({ products }: ProductMarqueeProps) {
  if (products.length === 0) return null

  const track = [...products, ...products]

  return (
    <div className="mt-16 border-t border-border pt-10">
      <h2 className="mb-6 text-sm font-medium text-muted-foreground">Vitrinden</h2>
      <div className="overflow-x-clip overflow-y-visible pt-2">
        <motion.div
          className="flex w-max gap-4"
          animate={{ x: ['0%', '-50%'] }}
          transition={{ duration: products.length * 4, repeat: Infinity, ease: 'linear' }}
        >
          {track.map((product, i) => (
            <MarqueeItem key={`${product.id}-${i}`} product={product} />
          ))}
        </motion.div>
      </div>
    </div>
  )
}
