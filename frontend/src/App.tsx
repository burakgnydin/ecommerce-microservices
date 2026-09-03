import { AnimatePresence, motion } from 'framer-motion'
import type { ReactNode } from 'react'
import { Route, Routes, useLocation } from 'react-router-dom'
import AccountPage from './pages/AccountPage'
import AdminPage from './pages/AdminPage'
import CartPage from './pages/CartPage'
import CategoriesPage from './pages/CategoriesPage'
import HomePage from './pages/HomePage'
import LoginPage from './pages/LoginPage'
import NotFoundPage from './pages/NotFoundPage'
import ProductDetailPage from './pages/ProductDetailPage'
import ProductListPage from './pages/ProductListPage'
import RegisterPage from './pages/RegisterPage'

function PageTransition({ children }: { children: ReactNode }) {
  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      transition={{ duration: 0.15, ease: 'easeOut' }}
    >
      {children}
    </motion.div>
  )
}

function App() {
  const location = useLocation()

  return (
    <AnimatePresence mode="wait">
      <Routes location={location} key={location.pathname}>
        <Route path="/" element={<PageTransition><HomePage /></PageTransition>} />
        <Route path="/products" element={<PageTransition><ProductListPage /></PageTransition>} />
        <Route path="/categories" element={<PageTransition><CategoriesPage /></PageTransition>} />
        <Route path="/products/:id" element={<PageTransition><ProductDetailPage /></PageTransition>} />
        <Route path="/cart" element={<PageTransition><CartPage /></PageTransition>} />
        <Route path="/account" element={<PageTransition><AccountPage /></PageTransition>} />
        <Route path="/login" element={<PageTransition><LoginPage /></PageTransition>} />
        <Route path="/register" element={<PageTransition><RegisterPage /></PageTransition>} />
        <Route path="/admin" element={<PageTransition><AdminPage /></PageTransition>} />
        <Route path="*" element={<PageTransition><NotFoundPage /></PageTransition>} />
      </Routes>
    </AnimatePresence>
  )
}

export default App
