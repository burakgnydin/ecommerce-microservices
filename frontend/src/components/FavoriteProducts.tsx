import { motion } from 'framer-motion'
import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { Star } from 'lucide-react'
import type { Product } from '../api/types'
import { ProductImage } from './ProductImage'
import { ShimmerButton } from './ui/ShimmerButton'

interface FavoriteProductsProps {
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

function formatPrice(price: number) {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(price)
}

export function FavoriteProducts({ products }: FavoriteProductsProps) {
  const favorites = useMemo(() => pickRandom(products, 5), [products])
  const [activeIndex, setActiveIndex] = useState(0)

  useEffect(() => {
    if (favorites.length <= 1) return
    const interval = setInterval(() => {
      setActiveIndex((current) => (current + 1) % favorites.length)
    }, 4500)
    return () => clearInterval(interval)
  }, [favorites.length])

  if (favorites.length === 0) return null

  return (
    <section className="border-t border-border py-16">
      <div className="mx-auto grid max-w-6xl grid-cols-1 gap-12 px-4 md:grid-cols-2 md:gap-16">
        <div className="flex flex-col justify-center">
          <div className="inline-flex w-fit items-center rounded-full bg-primary/10 px-3 py-1 text-sm font-medium text-primary">
            <Star className="mr-1 h-3.5 w-3.5 fill-primary" />
            <span>En sevilenler</span>
          </div>

          <h2 className="mt-6 text-3xl font-bold tracking-tight text-foreground sm:text-4xl">En Sevilen Ürünler</h2>

          <p className="mt-4 max-w-md text-muted-foreground">
            Müşterilerimizin en çok tercih ettiği ürünleri bir araya getirdik. Güncel favorileri kaçırmadan
            alışverişe başla.
          </p>

          <div className="mt-8 flex items-center gap-3">
            {favorites.map((product, index) => (
              <button
                key={product.id}
                type="button"
                onClick={() => setActiveIndex(index)}
                className={`h-2.5 rounded-full transition-all duration-300 ${
                  activeIndex === index ? 'w-10 bg-primary' : 'w-2.5 bg-muted-foreground/30'
                }`}
                aria-label={`${product.name} ürününü göster`}
              />
            ))}
          </div>
        </div>

        <div className="relative min-h-[440px] sm:min-h-[360px]">
          {favorites.map((product, index) => (
            <motion.div
              key={product.id}
              className="absolute inset-0"
              initial={false}
              animate={{
                opacity: activeIndex === index ? 1 : 0,
                x: activeIndex === index ? 0 : 40,
                scale: activeIndex === index ? 1 : 0.96,
              }}
              transition={{ duration: 0.5, ease: 'easeInOut' }}
              style={{ zIndex: activeIndex === index ? 10 : 0, pointerEvents: activeIndex === index ? 'auto' : 'none' }}
            >
              <Link
                to={`/products/${product.id}`}
                className="flex h-full flex-col overflow-hidden rounded-xl border border-border bg-card shadow-lg transition-colors hover:border-primary sm:flex-row"
              >
                <ProductImage product={product} className="h-48 w-full sm:h-auto sm:w-1/2" iconClassName="h-10 w-10" />
                <div className="flex flex-1 flex-col justify-between p-6">
                  <div>
                    {product.categoryName && (
                      <span className="rounded-full bg-secondary px-2.5 py-1 text-xs font-medium text-secondary-foreground">
                        {product.categoryName}
                      </span>
                    )}
                    <h3 className="mt-3 text-xl font-semibold text-card-foreground">{product.name}</h3>
                    {product.description && (
                      <p className="mt-2 line-clamp-3 text-sm text-muted-foreground">{product.description}</p>
                    )}
                  </div>
                  <div className="mt-4 flex items-center justify-between">
                    <span className="text-lg font-bold text-card-foreground">{formatPrice(product.price)}</span>
                    <ShimmerButton className="px-4 py-1.5">
                      <span className="whitespace-pre-wrap text-center text-xs font-medium leading-none tracking-tight">
                        İncele →
                      </span>
                    </ShimmerButton>
                  </div>
                </div>
              </Link>
            </motion.div>
          ))}
        </div>
      </div>
    </section>
  )
}
