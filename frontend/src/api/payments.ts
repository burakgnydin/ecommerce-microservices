import { apiFetch } from './client'
import { authHeader } from '../lib/auth'
import type { Payment } from './types'

export interface ChargeRequest {
  orderId: string
  cardNumber: string
  expiryMonth: number
  expiryYear: number
  cvv: string
}

export function charge(request: ChargeRequest) {
  return apiFetch<Payment>('/payments/api/payments', {
    method: 'POST',
    headers: authHeader(),
    body: JSON.stringify(request),
  })
}
