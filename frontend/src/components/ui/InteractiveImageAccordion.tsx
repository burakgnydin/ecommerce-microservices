import { AnimatePresence, motion } from 'framer-motion'
import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { getCategories } from '../../api/categories'
import { getProducts } from '../../api/products'
import type { Category, Product } from '../../api/types'
import { ProductImage } from '../ProductImage'
import { Skeleton } from './Skeleton'
import { cn } from '../../lib/utils'

const VISIBLE_COUNT = 5
const CATEGORY_ROTATE_MS = 7000
const IMAGE_ROTATE_MS = 4000
const PRODUCTS_PER_CATEGORY = 6

interface AccordionItemProps {
  category: Category
  product: Product | undefined
  isActive: boolean
  onMouseEnter: () => void
}

function AccordionItem({ category, product, isActive, onMouseEnter }: AccordionItemProps) {
  return (
    <Link
      to={`/products?categoryId=${category.id}&categoryName=${encodeURIComponent(category.name)}`}
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

function AccordionSkeleton() {
  return (
    <div className="flex flex-row items-center justify-center gap-3 overflow-x-auto p-4">
      {Array.from({ length: VISIBLE_COUNT }).map((_, i) => (
        <Skeleton key={i} className={cn('h-[420px] shrink-0 rounded-2xl', i === 0 ? 'w-[280px]' : 'w-[56px]')} />
      ))}
    </div>
  )
}

export function InteractiveImageAccordion() {
  const [categories, setCategories] = useState<Category[]>([])
  const [productsByCategory, setProductsByCategory] = useState<Record<string, Product[]>>({})
  const [windowStart, setWindowStart] = useState(0)
  const [activeIndex, setActiveIndex] = useState(0)
  const [tick, setTick] = useState(0)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    getCategories()
      .then((result) => {
        if (cancelled) return
        setCategories(result)
        return Promise.all(
          result.map((category) =>
            getProducts(1, PRODUCTS_PER_CATEGORY, category.id).then((r) => [category.id, r.items] as const),
          ),
        )
      })
      .then((entries) => {
        if (cancelled || !entries) return
        setProductsByCategory(Object.fromEntries(entries))
      })
      .catch(() => {})
      .finally(() => {
        if (!cancelled) setIsLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    if (categories.length <= VISIBLE_COUNT) return
    const interval = setInterval(() => {
      setWindowStart((current) => (current + VISIBLE_COUNT) % categories.length)
      setActiveIndex(0)
    }, CATEGORY_ROTATE_MS)
    return () => clearInterval(interval)
  }, [categories.length])

  useEffect(() => {
    const interval = setInterval(() => setTick((current) => current + 1), IMAGE_ROTATE_MS)
    return () => clearInterval(interval)
  }, [])

  const visibleCategories = useMemo(() => {
    if (categories.length === 0) return []
    const count = Math.min(VISIBLE_COUNT, categories.length)
    return Array.from({ length: count }, (_, i) => categories[(windowStart + i) % categories.length])
  }, [categories, windowStart])

  if (isLoading) return <AccordionSkeleton />
  if (visibleCategories.length === 0) return null

  return (
    <div className="flex flex-row items-center justify-center gap-3 overflow-x-auto p-4">
      {visibleCategories.map((category, index) => {
        const categoryProducts = productsByCategory[category.id] ?? []
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
