import { motion } from 'framer-motion'
import { useEffect, useState } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router-dom'
import { ApiError } from '../api/client'
import { getProductById } from '../api/products'
import type { Product } from '../api/types'
import { Header } from '../components/Header'
import { ProductDetailSkeleton } from '../components/ProductDetailSkeleton'
import { ProductImage } from '../components/ProductImage'
import { Card } from '../components/ui/Card'

function formatPrice(price: number) {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(price)
}

export default function ProductDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const location = useLocation()
  const canGoBack = location.key !== 'default'
  const [product, setProduct] = useState<Product | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  function goBackToProducts() {
    if (canGoBack) navigate(-1)
    else navigate('/products')
  }

  useEffect(() => {
    if (!id) return
    let cancelled = false
    setIsLoading(true)
    setError(null)

    getProductById(id)
      .then((data) => {
        if (!cancelled) setProduct(data)
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : 'Ürün yüklenemedi.')
        }
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [id])

  return (
    <div className="min-h-screen">
      <Header />
      <main className="mx-auto max-w-3xl px-4 py-10">
        <nav aria-label="Breadcrumb" className="mb-6 text-sm text-muted-foreground">
          <button type="button" onClick={goBackToProducts} className="hover:text-foreground">
            Ürünler
          </button>
          {product?.categoryName && <span className="mx-1.5">/</span>}
          {product?.categoryName && <span>{product.categoryName}</span>}
        </nav>

        {isLoading && <ProductDetailSkeleton />}
        {error && <p className="text-destructive">{error}</p>}

        {!isLoading && !error && product && (
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3, ease: 'easeOut' }}
          >
            <Card className="overflow-hidden p-0">
              <ProductImage product={product} className="h-72 w-full" iconClassName="h-16 w-16" />
              <div className="p-8">
                {product.categoryName && (
                  <span className="mb-3 inline-block rounded-full bg-secondary px-2.5 py-0.5 text-xs font-medium text-secondary-foreground">
                    {product.categoryName}
                  </span>
                )}
                <h1 className="text-3xl font-bold text-foreground">{product.name}</h1>

                <div className="mt-4 flex items-center gap-3">
                  <span className="text-3xl font-bold text-foreground">
                    {formatPrice(product.price)}
                  </span>
                  {product.stock > 0 ? (
                    <span className="rounded-full bg-secondary px-2.5 py-0.5 text-xs font-medium text-secondary-foreground">
                      {product.stock} adet stokta
                    </span>
                  ) : product.allowsPreOrder ? (
                    <span className="rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-medium text-primary">
                      Ön sipariş verilebilir
                    </span>
                  ) : (
                    <span className="rounded-full bg-destructive/10 px-2.5 py-0.5 text-xs font-medium text-destructive">
                      Stokta yok
                    </span>
                  )}
                </div>

                {product.description && (
                  <p className="mt-6 leading-relaxed text-muted-foreground">{product.description}</p>
                )}

                <button
                  type="button"
                  onClick={goBackToProducts}
                  className="mt-8 inline-block text-sm font-medium text-primary hover:underline"
                >
                  ← Ürünlere dön
                </button>
              </div>
            </Card>
          </motion.div>
        )}
      </main>
    </div>
  )
}
