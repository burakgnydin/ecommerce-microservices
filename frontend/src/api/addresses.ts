import { apiFetch } from './client'
import { authHeader } from '../lib/auth'
import type { Address, AddressCreateRequest, AddressUpdateRequest } from './types'

export function getAddresses() {
  return apiFetch<Address[]>('/auth/api/users/me/addresses', { headers: authHeader() })
}

export function createAddress(dto: AddressCreateRequest) {
  return apiFetch<Address>('/auth/api/users/me/addresses', {
    method: 'POST',
    headers: authHeader(),
    body: JSON.stringify(dto),
  })
}

export function updateAddress(id: string, dto: AddressUpdateRequest) {
  return apiFetch<Address>(`/auth/api/users/me/addresses/${id}`, {
    method: 'PUT',
    headers: authHeader(),
    body: JSON.stringify(dto),
  })
}

export function deleteAddress(id: string) {
  return apiFetch<void>(`/auth/api/users/me/addresses/${id}`, {
    method: 'DELETE',
    headers: authHeader(),
  })
}
