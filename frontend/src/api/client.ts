import { setAccessToken } from '../lib/auth'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5100'

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

const KNOWN_ERROR_MESSAGES: Record<string, string> = {
  'Invalid email or password.': 'E-posta veya şifre hatalı.',
}

export function translateApiError(err: unknown, fallback: string): string {
  if (err instanceof ApiError) {
    if (KNOWN_ERROR_MESSAGES[err.message]) return KNOWN_ERROR_MESSAGES[err.message]
    if (err.message.includes('A user with email') && err.message.includes('already exists'))
      return 'Bu e-posta adresi zaten kayıtlı.'
    return err.message
  }
  return fallback
}

// The access token lives in memory only and is refreshed once at app startup, so a session left
// open longer than its lifetime (see AuthService AccessTokenExpirationMinutes) hits a 401 on the
// next authenticated call - most visibly during checkout, since payment is the slowest step. On a
// 401 for an authenticated request, refresh the token once via the HttpOnly cookie and retry.
let refreshPromise: Promise<string | null> | null = null

function refreshAccessToken(): Promise<string | null> {
  if (!refreshPromise) {
    refreshPromise = fetch(`${API_BASE_URL}/auth/api/auth/refresh`, {
      method: 'POST',
      credentials: 'include',
    })
      .then((res) => (res.ok ? res.json() : null))
      .then((data) => {
        const token = data?.accessToken ?? null
        setAccessToken(token)
        return token
      })
      .catch(() => null)
      .finally(() => {
        refreshPromise = null
      })
  }
  return refreshPromise
}

export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const doFetch = (headers?: RequestInit['headers']) =>
    fetch(`${API_BASE_URL}${path}`, {
      ...options,
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
        ...headers,
      },
    })

  let response = await doFetch()

  const hasAuthHeader = 'Authorization' in (options.headers as Record<string, string> | undefined ?? {})
  if (response.status === 401 && hasAuthHeader) {
    const newToken = await refreshAccessToken()
    if (newToken) {
      response = await doFetch({ Authorization: `Bearer ${newToken}` })
    }
  }

  if (!response.ok) {
    const problem = await response.json().catch(() => null)
    throw new ApiError(response.status, problem?.detail ?? problem?.title ?? response.statusText)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}
