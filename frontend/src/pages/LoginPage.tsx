import { type FormEvent, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { login } from '../api/auth'
import { translateApiError } from '../api/client'
import { AuthLayout } from '../components/AuthLayout'
import { Button } from '../components/ui/Button'
import { Field } from '../components/ui/Field'
import { storeTokens } from '../lib/auth'

export default function LoginPage() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError(null)
    setIsSubmitting(true)
    try {
      const tokens = await login({ email, password })
      storeTokens(tokens)
      navigate('/')
    } catch (err) {
      setError(translateApiError(err, 'Giriş yapılamadı, tekrar deneyin.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout title="Giriş yap" subtitle="Hesabına erişmek için bilgilerini gir">
      <form onSubmit={handleSubmit} className="space-y-4">
        <Field
          id="email"
          label="E-posta"
          type="email"
          autoComplete="email"
          required
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
        <Field
          id="password"
          label="Şifre"
          type="password"
          autoComplete="current-password"
          required
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        {error && <p className="text-sm text-destructive">{error}</p>}
        <Button type="submit" className="w-full" disabled={isSubmitting}>
          {isSubmitting ? 'Giriş yapılıyor…' : 'Giriş yap'}
        </Button>
      </form>
      <p className="mt-6 text-center text-sm text-muted-foreground">
        Hesabın yok mu?{' '}
        <Link to="/register" className="font-medium text-primary hover:underline">
          Kayıt ol
        </Link>
      </p>
    </AuthLayout>
  )
}
