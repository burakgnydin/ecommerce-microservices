import { apiFetch } from './client'
import { authHeader } from '../lib/auth'
import type { Cart, CheckoutRequest, Order } from './types'

export function getCart() {
  return apiFetch<Cart>('/orders/api/cart', { headers: authHeader() })
}

export function addItem(productId: string, quantity: number) {
  return apiFetch<Cart>('/orders/api/cart/items', {
    method: 'POST',
    headers: authHeader(),
    body: JSON.stringify({ productId, quantity }),
  })
}

export function updateItemQuantity(productId: string, quantity: number) {
  return apiFetch<Cart>(`/orders/api/cart/items/${productId}`, {
    method: 'PUT',
    headers: authHeader(),
    body: JSON.stringify({ quantity }),
  })
}

export function removeItem(productId: string) {
  return apiFetch<Cart>(`/orders/api/cart/items/${productId}`, {
    method: 'DELETE',
    headers: authHeader(),
  })
}

export function checkout(dto: CheckoutRequest) {
  return apiFetch<Order>('/orders/api/cart/checkout', {
    method: 'POST',
    headers: authHeader(),
    body: JSON.stringify(dto),
  })
}
