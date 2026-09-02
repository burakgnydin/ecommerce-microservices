import { AnimatePresence, motion } from 'framer-motion'
import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { getProducts } from '../api/products'
import { ApiError } from '../api/client'
import type { PagedResult, Product } from '../api/types'
import { Header } from '../components/Header'
import { ProductCard } from '../components/ProductCard'
import { ProductCardSkeleton } from '../components/ProductCardSkeleton'
import { Button } from '../components/ui/Button'
import { SearchBar } from '../components/ui/SearchBar'

const PAGE_SIZE = 20

export default function ProductListPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const categoryId = searchParams.get('categoryId') ?? undefined
  const categoryName = searchParams.get('categoryName') ?? undefined
  const search = searchParams.get('search') ?? ''
  const [searchInput, setSearchInput] = useState(search)
  const [pageNumber, setPageNumber] = useState(1)
  const [result, setResult] = useState<PagedResult<Product> | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    setPageNumber(1)
  }, [categoryId, search])

  useEffect(() => {
    setSearchInput(search)
  }, [search])

  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchInput === search) return
      setSearchParams((prev) => {
        const next = new URLSearchParams(prev)
        if (searchInput) next.set('search', searchInput)
        else next.delete('search')
        return next
      })
    }, 400)

    return () => clearTimeout(timer)
  }, [searchInput, search, setSearchParams])

  useEffect(() => {
    let cancelled = false
    setIsLoading(true)
    setError(null)

    getProducts(pageNumber, PAGE_SIZE, categoryId, search || undefined)
      .then((data) => {
        if (!cancelled) setResult(data)
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : 'Ürünler yüklenemedi.')
        }
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [pageNumber, categoryId, search])

  return (
    <div className="min-h-screen">
      <Header />
      <main className="mx-auto max-w-6xl px-4 py-10">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <h1 className="text-2xl font-bold text-foreground">
            {categoryName ? categoryName : 'Ürünler'}
          </h1>
          <SearchBar value={searchInput} onChange={setSearchInput} placeholder="Ürün ara..." className="sm:w-72" />
        </div>

        {isLoading && (
          <div className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
            {Array.from({ length: PAGE_SIZE }).map((_, i) => (
              <ProductCardSkeleton key={i} />
            ))}
          </div>
        )}
        {error && <p className="mt-8 text-destructive">{error}</p>}
        {!isLoading && !error && result && result.items.length === 0 && (
          <p className="mt-8 text-muted-foreground">
            {search ? 'Aramanızla eşleşen ürün bulunamadı.' : 'Henüz ürün eklenmemiş.'}
          </p>
        )}

        {!isLoading && !error && result && result.items.length > 0 && (
          <>
            <AnimatePresence mode="wait">
              <motion.div
                key={pageNumber}
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -8 }}
                transition={{ duration: 0.25, ease: 'easeOut' }}
                className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4"
              >
                {result.items.map((product) => (
                  <ProductCard key={product.id} product={product} />
                ))}
              </motion.div>
            </AnimatePresence>

            {result.totalPages > 1 && (
              <div className="mt-10 flex items-center justify-center gap-3">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={pageNumber <= 1}
                  onClick={() => setPageNumber((p) => p - 1)}
                >
                  Önceki
                </Button>
                <span className="text-sm text-muted-foreground">
                  Sayfa {result.pageNumber} / {result.totalPages}
                </span>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={pageNumber >= result.totalPages}
                  onClick={() => setPageNumber((p) => p + 1)}
                >
                  Sonraki
                </Button>
              </div>
            )}
          </>
        )}
      </main>
    </div>
  )
}
