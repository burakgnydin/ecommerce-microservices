import { apiFetch } from './client'
import type { Category } from './types'

export function getCategories() {
  return apiFetch<Category[]>('/products/api/categories')
}
