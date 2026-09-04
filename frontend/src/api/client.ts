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

export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  if (!response.ok) {
    const problem = await response.json().catch(() => null)
    throw new ApiError(response.status, problem?.detail ?? problem?.title ?? response.statusText)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}
