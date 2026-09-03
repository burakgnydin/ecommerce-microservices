import { AnimatePresence, motion } from 'framer-motion'
import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { getCategories } from '../../api/categories'
import type { Category, Product } from '../../api/types'
import { ProductImage } from '../ProductImage'
import { cn } from '../../lib/utils'

interface AccordionItemProps {
  category: Category
  product: Product | undefined
  isActive: boolean
  onMouseEnter: () => void
}

function AccordionItem({ category, product, isActive, onMouseEnter }: AccordionItemProps) {
  return (
    <Link
      to={product ? `/products/${product.id}` : '/products'}
      className={cn(
        'relative h-[420px] shrink-0 overflow-hidden rounded-2xl transition-all duration-700 ease-in-out',
        isActive ? 'w-[280px]' : 'w-[56px]',
      )}
      onMouseEnter={onMouseEnter}
    >
      <AnimatePresence>
        {product ? (
          <motion.div
            key={product.id}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 1.2, ease: 'easeInOut' }}
            className="absolute inset-0"
          >
            <ProductImage product={product} className="h-full w-full" />
          </motion.div>
        ) : (
          <div className="absolute inset-0 bg-muted" />
        )}
      </AnimatePresence>
      <div className="absolute inset-0 bg-black/40" />

      <span
        className={cn(
          'absolute whitespace-nowrap text-lg font-semibold text-white transition-all duration-300 ease-in-out',
          isActive
            ? 'bottom-6 left-1/2 -translate-x-1/2 rotate-0'
            : 'bottom-24 left-1/2 w-auto -translate-x-1/2 rotate-90 text-left',
        )}
      >
        {category.name}
      </span>
    </Link>
  )
}

interface InteractiveImageAccordionProps {
  products: Product[]
}

export function InteractiveImageAccordion({ products }: InteractiveImageAccordionProps) {
  const [categories, setCategories] = useState<Category[]>([])
  const [activeIndex, setActiveIndex] = useState(0)
  const [tick, setTick] = useState(0)

  useEffect(() => {
    let cancelled = false
    getCategories()
      .then((result) => {
        if (!cancelled) setCategories(result)
      })
      .catch(() => {})
    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    const interval = setInterval(() => setTick((current) => current + 1), 4000)
    return () => clearInterval(interval)
  }, [])

  const productsByCategory = useMemo(() => {
    const map = new Map<string, Product[]>()
    for (const product of products) {
      const list = map.get(product.categoryId) ?? []
      list.push(product)
      map.set(product.categoryId, list)
    }
    return map
  }, [products])

  const visibleCategories = categories.slice(0, 5)

  if (visibleCategories.length === 0) return null

  return (
    <div className="flex flex-row items-center justify-center gap-3 overflow-x-auto p-4">
      {visibleCategories.map((category, index) => {
        const categoryProducts = productsByCategory.get(category.id) ?? []
        const product = categoryProducts.length > 0 ? categoryProducts[tick % categoryProducts.length] : undefined
        return (
          <AccordionItem
            key={category.id}
            category={category}
            product={product}
            isActive={index === activeIndex}
            onMouseEnter={() => setActiveIndex(index)}
          />
        )
      })}
    </div>
  )
}
