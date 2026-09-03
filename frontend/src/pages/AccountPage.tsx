import { type FormEvent, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { logout } from '../api/auth'
import { ApiError, translateApiError } from '../api/client'
import type { UserResponse } from '../api/types'
import { getMe, updateMe } from '../api/users'
import { Header } from '../components/Header'
import { Button } from '../components/ui/Button'
import { Skeleton } from '../components/ui/Skeleton'
import { clearTokens, getAccessToken, getRefreshToken } from '../lib/auth'

export default function AccountPage() {
  const navigate = useNavigate()
  const [user, setUser] = useState<UserResponse | null>(null)
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [formError, setFormError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    if (!getAccessToken()) {
      navigate('/login')
      return
    }

    getMe()
      .then((result) => {
        setUser(result)
        setName(result.name)
        setEmail(result.email)
      })
      .catch((err) => setLoadError(err instanceof ApiError ? err.message : 'Bilgiler yüklenemedi.'))
      .finally(() => setIsLoading(false))
  }, [navigate])

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setFormError(null)
    setSuccessMessage(null)
    setIsSubmitting(true)
    try {
      const result = await updateMe({ name, email })
      setUser(result)
      setName(result.name)
      setEmail(result.email)
      setSuccessMessage('Bilgilerin güncellendi.')
    } catch (err) {
      setFormError(translateApiError(err, 'Bilgiler güncellenemedi, tekrar deneyin.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handleLogout() {
    const refreshToken = getRefreshToken()
    try {
      if (refreshToken) await logout(refreshToken)
    } catch {
      // ignore - clear local session regardless
    }
    clearTokens()
    navigate('/')
  }

  return (
    <div className="min-h-screen">
      <Header />
      <main className="mx-auto max-w-md px-4 py-10">
        <h1 className="text-2xl font-semibold text-foreground">Hesabım</h1>

        {isLoading && (
          <div className="mt-6 space-y-4">
            <Skeleton className="h-11 w-full rounded-md" />
            <Skeleton className="h-11 w-full rounded-md" />
          </div>
        )}

        {!isLoading && loadError && <p className="mt-6 text-destructive">{loadError}</p>}

        {!isLoading && !loadError && user && (
          <>
            <form onSubmit={handleSubmit} className="mt-6 space-y-4">
              <div>
                <label htmlFor="name" className="mb-1 block text-sm font-medium text-muted-foreground">
                  Ad Soyad
                </label>
                <input
                  id="name"
                  type="text"
                  required
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  className="h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground outline-none focus-visible:ring-2 focus-visible:ring-ring"
                />
              </div>

              <div>
                <label htmlFor="email" className="mb-1 block text-sm font-medium text-muted-foreground">
                  E-posta
                </label>
                <input
                  id="email"
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground outline-none focus-visible:ring-2 focus-visible:ring-ring"
                />
              </div>

              <p className="text-sm text-muted-foreground">
                Rol: {user.role === 'Admin' ? 'Yönetici' : 'Müşteri'} · Kayıt tarihi:{' '}
                {new Date(user.createdAt).toLocaleDateString('tr-TR')}
              </p>

              {formError && <p className="text-sm text-destructive">{formError}</p>}
              {successMessage && <p className="text-sm text-green-600">{successMessage}</p>}

              <Button type="submit" disabled={isSubmitting} className="h-11 w-full">
                {isSubmitting ? 'Kaydediliyor...' : 'Bilgileri Güncelle'}
              </Button>
            </form>

            <Button variant="outline" onClick={handleLogout} className="mt-4 h-11 w-full">
              Çıkış yap
            </Button>
          </>
        )}
      </main>
    </div>
  )
}
