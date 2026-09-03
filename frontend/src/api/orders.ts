import { apiFetch } from './client'
import { authHeader } from '../lib/auth'
import type { Order } from './types'

export function getMyOrders() {
  return apiFetch<Order[]>('/orders/api/orders', { headers: authHeader() })
}

export function getAllOrdersAdmin() {
  return apiFetch<Order[]>('/orders/api/orders/admin/all', { headers: authHeader() })
}
