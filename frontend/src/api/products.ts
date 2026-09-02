import { apiFetch } from './client'
import type { PagedResult, Product } from './types'

export function getProducts(pageNumber = 1, pageSize = 20, categoryId?: string, search?: string) {
  const params = new URLSearchParams({ pageNumber: String(pageNumber), pageSize: String(pageSize) })
  if (categoryId) params.set('categoryId', categoryId)
  if (search) params.set('search', search)

  return apiFetch<PagedResult<Product>>(`/products/api/products?${params.toString()}`)
}

export function getProductById(id: string) {
  return apiFetch<Product>(`/products/api/products/${id}`)
}
