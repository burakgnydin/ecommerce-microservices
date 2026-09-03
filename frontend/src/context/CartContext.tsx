import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react'
import * as cartApi from '../api/cart'
import { ApiError } from '../api/client'
import { getAccessToken } from '../lib/auth'
import type { Cart, Order } from '../api/types'

interface CartContextValue {
  cart: Cart | null
  itemCount: number
  isLoading: boolean
  error: string | null
  refresh: () => Promise<void>
  addItem: (productId: string, quantity: number) => Promise<void>
  updateQuantity: (productId: string, quantity: number) => Promise<void>
  removeItem: (productId: string) => Promise<void>
  checkout: () => Promise<Order>
}

const CartContext = createContext<CartContextValue | null>(null)

export function CartProvider({ children }: { children: ReactNode }) {
  const [cart, setCart] = useState<Cart | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const refresh = useCallback(async () => {
    if (!getAccessToken()) {
      setCart(null)
      return
    }
    setIsLoading(true)
    setError(null)
    try {
      setCart(await cartApi.getCart())
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Sepet yüklenemedi.')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    refresh()
  }, [refresh])

  const addItem = useCallback(async (productId: string, quantity: number) => {
    setCart(await cartApi.addItem(productId, quantity))
  }, [])

  const updateQuantity = useCallback(async (productId: string, quantity: number) => {
    setCart(await cartApi.updateItemQuantity(productId, quantity))
  }, [])

  const removeItem = useCallback(async (productId: string) => {
    setCart(await cartApi.removeItem(productId))
  }, [])

  const checkout = useCallback(async () => {
    const order = await cartApi.checkout()
    setCart(null)
    return order
  }, [])

  const itemCount = cart?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0

  return (
    <CartContext.Provider
      value={{ cart, itemCount, isLoading, error, refresh, addItem, updateQuantity, removeItem, checkout }}
    >
      {children}
    </CartContext.Provider>
  )
}

export function useCart() {
  const ctx = useContext(CartContext)
  if (!ctx) throw new Error('useCart must be used within a CartProvider')
  return ctx
}
