import { motion } from 'framer-motion'
import { useMemo } from 'react'
import { Link } from 'react-router-dom'
import type { Product } from '../api/types'
import { ProductImage } from './ProductImage'

interface ProductScreenMockupProps {
  products: Product[]
}

function pickRandom<T>(items: T[], count: number): T[] {
  const shuffled = [...items]
  for (let i = shuffled.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1))
    ;[shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]]
  }
  return shuffled.slice(0, count)
}

export function ProductScreenMockup({ products }: ProductScreenMockupProps) {
  const preview = useMemo(() => pickRandom(products, 4), [products])
  if (preview.length === 0) return null

  return (
    <motion.div
      initial={{ opacity: 0, y: 30 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.7, delay: 0.35, ease: [0.22, 1, 0.36, 1] }}
      className="relative mx-auto mt-14 w-full max-w-2xl"
    >
      <span className="absolute -top-4 right-4 z-10 rounded-full bg-primary px-3 py-1 text-xs font-medium text-primary-foreground shadow-md">
        Hemen incele →
      </span>
      <div className="rounded-2xl bg-foreground p-3 shadow-xl">
        <div className="mb-2.5 flex items-center gap-1.5 px-1">
          <span className="h-2.5 w-2.5 rounded-full bg-white/25" />
          <span className="h-2.5 w-2.5 rounded-full bg-white/25" />
          <span className="h-2.5 w-2.5 rounded-full bg-white/25" />
        </div>
        <div className="grid grid-cols-2 gap-3 rounded-lg bg-card p-4">
          {preview.map((product) => (
            <Link
              key={product.id}
              to={`/products/${product.id}`}
              className="overflow-hidden rounded-lg border border-border transition-colors hover:border-primary"
            >
              <ProductImage product={product} className="h-20 w-full" iconClassName="h-6 w-6" />
              <div className="p-2.5">
                <p className="truncate text-xs font-medium text-card-foreground">{product.name}</p>
              </div>
            </Link>
          ))}
        </div>
      </div>
      <div className="mx-auto h-6 w-16 rounded-b-md bg-foreground/90" />
      <div className="mx-auto h-2 w-40 rounded-full bg-foreground/70" />
    </motion.div>
  )
}
