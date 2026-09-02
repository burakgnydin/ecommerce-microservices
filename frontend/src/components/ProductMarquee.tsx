import { Link } from 'react-router-dom'
import type { Product } from '../api/types'
import { ProductImage } from './ProductImage'

interface ProductMarqueeProps {
  products: Product[]
}

export function ProductMarquee({ products }: ProductMarqueeProps) {
  if (products.length === 0) return null

  const track = [...products, ...products]

  return (
    <div className="mt-16 overflow-hidden border-t border-border pt-10">
      <h2 className="mb-6 text-sm font-medium text-muted-foreground">Vitrinden</h2>
      <div
        className="flex w-max gap-4 animate-marquee"
        style={{ animationDuration: `${products.length * 4}s` }}
      >

        {track.map((product, i) => (
          <Link
            key={`${product.id}-${i}`}
            to={`/products/${product.id}`}
            className="flex w-40 shrink-0 flex-col overflow-hidden rounded-lg border border-border bg-card"
          >
            <ProductImage product={product} className="h-24 w-full" iconClassName="h-6 w-6" />
            <span className="truncate p-2.5 text-xs font-medium text-foreground">{product.name}</span>
          </Link>
        ))}
      </div>
    </div>
  )
}
