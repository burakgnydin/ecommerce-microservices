import { apiFetch } from './client'
import { authHeader } from '../lib/auth'
import type { PagedResult, Product, ProductCreateRequest, ProductUpdateRequest } from './types'

export function getProducts(pageNumber = 1, pageSize = 20, categoryId?: string, search?: string) {
  const params = new URLSearchParams({ pageNumber: String(pageNumber), pageSize: String(pageSize) })
  if (categoryId) params.set('categoryId', categoryId)
  if (search) params.set('search', search)

  return apiFetch<PagedResult<Product>>(`/products/api/products?${params.toString()}`)
}

export function getProductById(id: string) {
  return apiFetch<Product>(`/products/api/products/${id}`)
}

export function createProduct(dto: ProductCreateRequest) {
  return apiFetch<Product>('/products/api/products', {
    method: 'POST',
    headers: authHeader(),
    body: JSON.stringify(dto),
  })
}

export function updateProduct(id: string, dto: ProductUpdateRequest) {
  return apiFetch<Product>(`/products/api/products/${id}`, {
    method: 'PUT',
    headers: authHeader(),
    body: JSON.stringify(dto),
  })
}

export function deleteProduct(id: string) {
  return apiFetch<void>(`/products/api/products/${id}`, {
    method: 'DELETE',
    headers: authHeader(),
  })
}
