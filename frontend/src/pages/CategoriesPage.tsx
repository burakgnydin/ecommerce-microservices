import { motion } from 'framer-motion'
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getCategories } from '../api/categories'
import { ApiError } from '../api/client'
import type { Category } from '../api/types'
import { Header } from '../components/Header'
import { Card } from '../components/ui/Card'
import { SearchBar } from '../components/ui/SearchBar'

const fadeUp = {
  hidden: { opacity: 0, y: 16 },
  visible: (i: number) => ({
    opacity: 1,
    y: 0,
    transition: { delay: i * 0.05, duration: 0.5, ease: [0.22, 1, 0.36, 1] as const },
  }),
}

export default function CategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([])
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [search, setSearch] = useState('')

  const filteredCategories = categories.filter((category) =>
    category.name.toLocaleLowerCase('tr').includes(search.toLocaleLowerCase('tr')),
  )

  useEffect(() => {
    let cancelled = false
    getCategories()
      .then((data) => {
        if (!cancelled) setCategories(data)
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

        {isLoading && <p className="mt-8 text-muted-foreground">Yükleniyor...</p>}
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
                  <Card className="p-6 transition-colors hover:border-primary">
                    <h2 className="text-lg font-semibold text-foreground">{category.name}</h2>
                  </Card>
                </Link>
              </motion.div>
            ))}
          </div>
        )}
      </main>
    </div>
  )
}
