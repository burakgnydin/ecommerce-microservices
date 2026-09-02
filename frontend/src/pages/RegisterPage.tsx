import { type FormEvent, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { login, register } from '../api/auth'
import { translateApiError } from '../api/client'
import { AuthLayout } from '../components/AuthLayout'
import { Button } from '../components/ui/Button'
import { Field } from '../components/ui/Field'
import { storeTokens } from '../lib/auth'

export default function RegisterPage() {
  const navigate = useNavigate()
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError(null)

    if (password !== confirmPassword) {
      setError('Şifreler eşleşmiyor.')
      return
    }

    setIsSubmitting(true)
    try {
      await register({ name, email, password })
      const tokens = await login({ email, password })
      storeTokens(tokens)
      navigate('/')
    } catch (err) {
      setError(translateApiError(err, 'Kayıt oluşturulamadı, tekrar deneyin.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout title="Hesap oluştur" subtitle="Alışverişe başlamak için birkaç bilgi gir">
      <form onSubmit={handleSubmit} className="space-y-4">
        <Field
          id="name"
          label="Ad Soyad"
          autoComplete="name"
          required
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
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
          autoComplete="new-password"
          required
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <Field
          id="confirmPassword"
          label="Şifre (tekrar)"
          type="password"
          autoComplete="new-password"
          required
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
        />
        {error && <p className="text-sm text-destructive">{error}</p>}
        <Button type="submit" className="w-full" disabled={isSubmitting}>
          {isSubmitting ? 'Hesap oluşturuluyor…' : 'Kayıt ol'}
        </Button>
      </form>
      <p className="mt-6 text-center text-sm text-muted-foreground">
        Zaten hesabın var mı?{' '}
        <Link to="/login" className="font-medium text-primary hover:underline">
          Giriş yap
        </Link>
      </p>
    </AuthLayout>
  )
}
