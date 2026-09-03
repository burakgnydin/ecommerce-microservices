import { GrainGradient } from '@paper-design/shaders-react'
import { type FormEvent, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { login, register } from '../api/auth'
import { translateApiError } from '../api/client'
import { useCart } from '../context/CartContext'
import { storeTokens } from '../lib/auth'

interface FieldBoxProps {
  id: string
  label: string
  type?: string
  autoComplete?: string
  required?: boolean
  value: string
  onChange: (value: string) => void
}

function FieldBox({ id, label, type = 'text', autoComplete, required, value, onChange }: FieldBoxProps) {
  const [isFocused, setIsFocused] = useState(false)
  const showFloatingLabel = !isFocused && !value

  return (
    <label
      htmlFor={id}
      className="flex h-14 items-center gap-4 rounded-[10px] border border-border bg-card px-5 text-base leading-none"
    >
      {showFloatingLabel && <span className="shrink-0 text-muted-foreground">{label}</span>}
      <input
        id={id}
        type={type}
        autoComplete={autoComplete}
        required={required}
        value={value}
        aria-label={label}
        onFocus={() => setIsFocused(true)}
        onBlur={() => setIsFocused(false)}
        onChange={(event) => onChange(event.target.value)}
        className="min-w-0 flex-1 truncate bg-transparent text-foreground outline-none"
      />
    </label>
  )
}

export default function RegisterPage() {
  const navigate = useNavigate()
  const { refresh } = useCart()
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
      await refresh()
      navigate('/')
    } catch (err) {
      setError(translateApiError(err, 'Kayıt oluşturulamadı, tekrar deneyin.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="min-h-screen bg-background p-3 text-foreground antialiased">
      <div className="grid min-h-[calc(100vh-1.5rem)] gap-6 lg:grid-cols-[1.06fr_0.94fr]">
        <div className="relative flex min-h-[720px] overflow-hidden rounded-md bg-primary p-8 text-primary-foreground sm:p-12 lg:min-h-0">
          <GrainGradient
            speed={1}
            scale={1}
            rotation={0}
            offsetX={0}
            offsetY={0}
            softness={0.5}
            intensity={0.5}
            noise={0.25}
            shape="corners"
            colors={['#ffffff', '#3891ff', '#0a0a0a', '#3b82f6']}
            colorBack="#00000000"
            className="absolute inset-0 bg-primary"
          />

          <div className="relative z-10 flex h-full w-full flex-col justify-between">
            <h2 className="max-w-[620px] pt-0 text-5xl font-medium tracking-[-0.05em] text-primary-foreground sm:text-6xl lg:pt-16 lg:text-[64px] lg:leading-[0.98] xl:text-[70px]">
              Hızlı başla,
              <br />
              kolayca alışveriş yap
            </h2>
          </div>
        </div>

        <div className="flex min-h-[760px] items-start rounded-md border border-border bg-card px-6 py-12 sm:px-10 lg:min-h-0 lg:px-14 lg:py-28 xl:px-20">
          <div className="mx-auto w-full max-w-[590px]">
            <div>
              <Link to="/" className="text-sm font-medium text-muted-foreground hover:text-foreground">
                ← Ana sayfa
              </Link>
              <h1 className="mt-6 text-3xl font-medium tracking-[-0.04em] sm:text-4xl lg:text-[42px] lg:leading-[1.05] xl:text-[50px]">
                Hesap oluştur
              </h1>
              <p className="mt-3 text-lg leading-snug text-muted-foreground sm:text-xl">
                Alışverişe başlamak için birkaç bilgi gir
              </p>
            </div>

            <form onSubmit={handleSubmit} className="mt-12 space-y-5">
              <FieldBox id="name" label="Ad Soyad" autoComplete="name" required value={name} onChange={setName} />
              <FieldBox id="email" label="E-posta" type="email" autoComplete="email" required value={email} onChange={setEmail} />
              <FieldBox
                id="password"
                label="Şifre"
                type="password"
                autoComplete="new-password"
                required
                value={password}
                onChange={setPassword}
              />
              <FieldBox
                id="confirmPassword"
                label="Şifre (tekrar)"
                type="password"
                autoComplete="new-password"
                required
                value={confirmPassword}
                onChange={setConfirmPassword}
              />

              {error && <p className="text-sm text-destructive">{error}</p>}

              <button
                type="submit"
                disabled={isSubmitting}
                className="mt-9 flex h-12 w-full items-center justify-center rounded-[10px] border border-primary bg-primary text-lg font-medium text-primary-foreground transition-colors hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isSubmitting ? 'Hesap oluşturuluyor…' : 'Kayıt ol'}
              </button>
            </form>

            <p className="mt-6 text-center text-sm text-muted-foreground">
              Zaten hesabın var mı?{' '}
              <Link to="/login" className="font-medium text-primary hover:underline">
                Giriş yap
              </Link>
            </p>
          </div>
        </div>
      </div>
    </section>
  )
}
