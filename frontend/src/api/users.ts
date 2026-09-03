import { apiFetch } from './client'
import { authHeader } from '../lib/auth'
import type { ChangePasswordRequest, UserResponse, UserUpdateRequest } from './types'

export function getMe() {
  return apiFetch<UserResponse>('/auth/api/users/me', {
    headers: authHeader(),
  })
}

export function updateMe(dto: UserUpdateRequest) {
  return apiFetch<UserResponse>('/auth/api/users/me', {
    method: 'PUT',
    headers: authHeader(),
    body: JSON.stringify(dto),
  })
}

export function changePassword(dto: ChangePasswordRequest) {
  return apiFetch<void>('/auth/api/users/me/password', {
    method: 'PUT',
    headers: authHeader(),
    body: JSON.stringify(dto),
  })
}
