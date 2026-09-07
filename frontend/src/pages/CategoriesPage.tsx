import { motion } from 'framer-motion'
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getCategories } from '../api/categories'
import { ApiError } from '../api/client'
import { getProducts } from '../api/products'
import type { Category } from '../api/types'
import { Header } from '../components/Header'
import { ArticleCard } from '../components/ui/ArticleCard'
import { ArticleCardSkeleton } from '../components/ui/ArticleCardSkeleton'
import { SearchBar } from '../components/ui/SearchBar'

const fadeUp = {
  hidden: { opacity: 0, y: 16 },
  visible: (i: number) => ({
    opacity: 1,
    y: 0,
    transition: { delay: i * 0.05, duration: 0.5, ease: [0.22, 1, 0.36, 1] as const },
  }),
}

interface CategoryPreview {
  imageUrl: string | null
  count: number
}

export default function CategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([])
  const [previews, setPreviews] = useState<Record<string, CategoryPreview>>({})
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [search, setSearch] = useState('')

  const filteredCategories = categories.filter((category) =>
    category.name.toLocaleLowerCase('tr').includes(search.toLocaleLowerCase('tr')),
  )

  useEffect(() => {
    let cancelled = false
    getCategories()
      .then((categoryList) => {
        if (cancelled) return
        setCategories(categoryList)
        return Promise.all(
          categoryList.map((category) =>
            getProducts(1, 1, category.id).then((data) => [category.id, data] as const),
          ),
        )
      })
      .then((entries) => {
        if (cancelled || !entries) return
        const byCategory: Record<string, CategoryPreview> = {}
        for (const [categoryId, data] of entries) {
          byCategory[categoryId] = { imageUrl: data.items[0]?.imageUrl ?? null, count: data.totalCount }
        }
        setPreviews(byCategory)
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : 'Kategoriler yüklenemedi.')
        }
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [])

  return (
    <div className="min-h-screen">
      <Header />
      <main className="mx-auto max-w-6xl px-4 py-10">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <h1 className="text-2xl font-bold text-foreground">Kategoriler</h1>
          <SearchBar value={search} onChange={setSearch} placeholder="Kategori ara..." className="sm:w-72" />
        </div>

        {isLoading && (
          <div className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }).map((_, i) => (
              <ArticleCardSkeleton key={i} />
            ))}
          </div>
        )}
        {error && <p className="mt-8 text-destructive">{error}</p>}
        {!isLoading && !error && categories.length === 0 && (
          <p className="mt-8 text-muted-foreground">Henüz kategori eklenmemiş.</p>
        )}
        {!isLoading && !error && categories.length > 0 && filteredCategories.length === 0 && (
          <p className="mt-8 text-muted-foreground">Aramanızla eşleşen kategori bulunamadı.</p>
        )}

        {!isLoading && !error && filteredCategories.length > 0 && (
          <div className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {filteredCategories.map((category, i) => (
              <motion.div key={category.id} custom={i} initial="hidden" animate="visible" variants={fadeUp}>
                <Link to={`/products?categoryId=${category.id}&categoryName=${encodeURIComponent(category.name)}`}>
                  <ArticleCard
                    title={category.name}
                    count={previews[category.id]?.count ?? 0}
                    imageUrl={previews[category.id]?.imageUrl ?? null}
                  />
                </Link>
              </motion.div>
            ))}
          </div>
        )}
      </main>
    </div>
  )
}
