import { apiFetch } from './client'
import type { LoginRequest, LoginResponse, RegisterRequest, UserResponse } from './types'

export function register(dto: RegisterRequest) {
  return apiFetch<UserResponse>('/auth/api/auth/register', {
    method: 'POST',
    body: JSON.stringify(dto),
  })
}

export function login(dto: LoginRequest) {
  return apiFetch<LoginResponse>('/auth/api/auth/login', {
    method: 'POST',
    body: JSON.stringify(dto),
  })
}

export function refresh() {
  return apiFetch<LoginResponse>('/auth/api/auth/refresh', {
    method: 'POST',
  })
}

export function logout() {
  return apiFetch<void>('/auth/api/auth/logout', {
    method: 'POST',
  })
}
