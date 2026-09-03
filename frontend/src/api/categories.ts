import { apiFetch } from './client'
import { authHeader } from '../lib/auth'
import type { Category, CategoryCreateRequest, CategoryUpdateRequest } from './types'

export function getCategories() {
  return apiFetch<Category[]>('/products/api/categories')
}

export function createCategory(dto: CategoryCreateRequest) {
  return apiFetch<Category>('/products/api/categories', {
    method: 'POST',
    headers: authHeader(),
    body: JSON.stringify(dto),
  })
}

export function updateCategory(id: string, dto: CategoryUpdateRequest) {
  return apiFetch<Category>(`/products/api/categories/${id}`, {
    method: 'PUT',
    headers: authHeader(),
    body: JSON.stringify(dto),
  })
}

export function deleteCategory(id: string) {
  return apiFetch<void>(`/products/api/categories/${id}`, {
    method: 'DELETE',
    headers: authHeader(),
  })
}
