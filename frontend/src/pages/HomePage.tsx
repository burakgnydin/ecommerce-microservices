import { motion } from 'framer-motion'
import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { getProducts } from '../api/products'
import type { Product } from '../api/types'
import { Header } from '../components/Header'
import { ProductMarquee } from '../components/ProductMarquee'
import { ProductScreenMockup } from '../components/ProductScreenMockup'
import { InteractiveHoverButton } from '../components/ui/InteractiveHoverButton'
import { SearchBar } from '../components/ui/SearchBar'

const fadeUp = {
  hidden: { opacity: 0, y: 16 },
  visible: (i: number) => ({
    opacity: 1,
    y: 0,
    transition: { delay: i * 0.12, duration: 0.6, ease: [0.22, 1, 0.36, 1] as const },
  }),
}

export default function HomePage() {
  const [products, setProducts] = useState<Product[]>([])
  const [search, setSearch] = useState('')
  const navigate = useNavigate()

  useEffect(() => {
    let cancelled = false
    getProducts(1, 48)
      .then((data) => {
        if (!cancelled) setProducts(data.items)
      })
      .catch(() => {})
    return () => {
      cancelled = true
    }
  }, [])

  return (
    <div className="min-h-screen">
      <Header />
      <main className="mx-auto flex max-w-3xl flex-col items-center px-4 pb-16 pt-10 text-center">
        <motion.h1
          custom={0}
          initial="hidden"
          animate="visible"
          variants={fadeUp}
          className="font-serif text-4xl leading-tight text-foreground sm:text-5xl"
        >
          Alışverişin <em className="italic">Sade</em> Hali
        </motion.h1>
        <motion.p
          custom={1}
          initial="hidden"
          animate="visible"
          variants={fadeUp}
          className="mt-4 max-w-md text-base text-muted-foreground"
        >
          Güncel ürünleri keşfet, hesabını oluştur ve alışverişe birkaç tıkla başla.
        </motion.p>
        <motion.div
          custom={2}
          initial="hidden"
          animate="visible"
          variants={fadeUp}
          className="mt-6 w-full max-w-sm"
        >
          <SearchBar
            value={search}
            onChange={setSearch}
            onSubmit={(value) => navigate(value ? `/products?search=${encodeURIComponent(value)}` : '/products')}
            placeholder="Ne arıyorsun?"
          />
        </motion.div>
        <motion.div custom={3} initial="hidden" animate="visible" variants={fadeUp} className="mt-4">
          <Link to="/products">
            <InteractiveHoverButton text="Ürünleri Keşfet" className="w-48 h-12 text-base" />
          </Link>
        </motion.div>

        <ProductScreenMockup products={products} />
      </main>

      <div className="mx-auto max-w-6xl px-4 pb-16">
        <ProductMarquee products={products} />
      </div>
    </div>
  )
}
