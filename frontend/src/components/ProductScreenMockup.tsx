import { useMemo } from 'react'
import { Link } from 'react-router-dom'
import type { Product } from '../api/types'
import { ContainerScroll } from './ui/ContainerScroll'
import { ProductImage } from './ProductImage'
import { ShimmerButton } from './ui/ShimmerButton'
import { ShimmerText } from './ui/ShimmerText'

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
  const preview = useMemo(() => pickRandom(products, 9), [products])
  const columns = useMemo(
    () => [preview.slice(0, 3), preview.slice(3, 6), preview.slice(6, 9)].filter((column) => column.length > 0),
    [preview],
  )
  if (preview.length === 0) return null

  return (
    <ContainerScroll
      titleComponent={
        <>
          <h2 className="text-2xl font-semibold text-foreground sm:text-3xl">
            Öne çıkan <ShimmerText>ürünleri</ShimmerText> keşfet
          </h2>
          <p className="mt-2 text-muted-foreground">Kaydırdıkça vitrindeki ürünlerimizi yakından incele.</p>
          <Link to="/products" className="mt-3 inline-block">
            <ShimmerButton className="px-4 py-1.5">
              <span className="whitespace-pre-wrap text-center text-xs font-medium leading-none tracking-tight">
                Tümünü incele →
              </span>
            </ShimmerButton>
          </Link>
        </>
      }
    >
      <div className="grid h-full grid-cols-2 gap-3 sm:grid-cols-3">
        {columns.map((column, columnIndex) => (
          <div key={columnIndex} className="h-full overflow-hidden">
            <div
              className="flex flex-col gap-3 animate-marquee-vertical"
              style={{ animationDuration: `${column.length * 6}s`, animationDelay: `${columnIndex * -2}s` }}
            >
              {[...column, ...column].map((product, i) => (
                <Link
                  key={`${product.id}-${i}`}
                  to={`/products/${product.id}`}
                  className="flex shrink-0 flex-col overflow-hidden rounded-lg border border-border transition-colors hover:border-primary"
                >
                  <ProductImage product={product} className="h-24 w-full sm:h-28" iconClassName="h-5 w-5" />
                  <div className="p-2">
                    <p className="truncate text-xs font-medium text-card-foreground">{product.name}</p>
                  </div>
                </Link>
              ))}
            </div>
          </div>
        ))}
      </div>
    </ContainerScroll>
  )
}
