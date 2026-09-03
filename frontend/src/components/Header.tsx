import { AnimatePresence, motion } from 'framer-motion'
import { useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { getCategories } from '../api/categories'
import type { Category } from '../api/types'
import { InteractiveHoverButton } from './ui/InteractiveHoverButton'

function CategoriesDropdown() {
  const [isOpen, setIsOpen] = useState(false)
  const [categories, setCategories] = useState<Category[]>([])

  useEffect(() => {
    if (isOpen && categories.length === 0) {
      getCategories()
        .then(setCategories)
        .catch(() => setCategories([]))
    }
  }, [isOpen, categories.length])

  return (
    <div className="relative" onMouseEnter={() => setIsOpen(true)} onMouseLeave={() => setIsOpen(false)}>
      <Link to="/categories" className="text-muted-foreground hover:text-foreground">
        Kategoriler
      </Link>
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: -8 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -8 }}
            transition={{ duration: 0.15, ease: 'easeOut' }}
            className="absolute left-0 top-full z-20 mt-2 w-56 rounded-lg border border-border bg-card p-2 text-card-foreground shadow-md"
          >
            {categories.length === 0 && (
              <p className="px-3 py-2 text-sm text-muted-foreground">Yükleniyor...</p>
            )}
            {categories.map((category) => (
              <Link
                key={category.id}
                to={`/products?categoryId=${category.id}&categoryName=${encodeURIComponent(category.name)}`}
                className="block rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-primary/10 hover:text-foreground"
              >
                {category.name}
              </Link>
            ))}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  )
}

function AuthMenu() {
  const [isOpen, setIsOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  return (
    <div className="relative" ref={containerRef}>
      <InteractiveHoverButton text="Hesabım" onClick={() => setIsOpen((prev) => !prev)} />
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: -8 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -8 }}
            transition={{ duration: 0.15, ease: 'easeOut' }}
            className="absolute right-0 top-full z-20 mt-2 w-48 rounded-lg border border-border bg-card p-2 text-card-foreground shadow-md"
          >
            <Link
              to="/login"
              onClick={() => setIsOpen(false)}
              className="block rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-primary/10 hover:text-foreground"
            >
              Giriş yap
            </Link>
            <Link
              to="/register"
              onClick={() => setIsOpen(false)}
              className="block rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-primary/10 hover:text-foreground"
            >
              Kayıt ol
            </Link>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  )
}

export function Header() {
  return (
    <header className="relative z-10">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-5">
        <Link to="/" className="text-lg font-bold text-foreground">
          E-Ticaret
        </Link>
        <nav className="flex items-center gap-6 text-sm font-medium">
          <Link to="/products" className="text-muted-foreground hover:text-foreground">
            Ürünler
          </Link>
          <CategoriesDropdown />
          <AuthMenu />
        </nav>
      </div>
    </header>
  )
}
