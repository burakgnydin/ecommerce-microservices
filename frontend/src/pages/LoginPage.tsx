import { type ChangeEvent, type FormEvent, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Boxes, Gift, Package, PackageCheck, PackageOpen, ShoppingBag, ShoppingCart, Truck, Warehouse } from 'lucide-react'
import { login } from '../api/auth'
import { translateApiError } from '../api/client'
import { AnimatedForm, Ripple, TechOrbitDisplay } from '../components/ui/AnimatedSignIn'
import { useCart } from '../context/CartContext'
import { storeTokens } from '../lib/auth'

function OrbitIcon({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex h-full w-full items-center justify-center rounded-full border border-border bg-card text-foreground shadow-sm">
      {children}
    </div>
  )
}

const orbitIcons = [
  { component: () => <OrbitIcon><Package className="h-3.5 w-3.5" /></OrbitIcon>, className: 'size-[30px] border-none bg-transparent', duration: 20, delay: 20, radius: 100, path: false },
  { component: () => <OrbitIcon><Boxes className="h-3.5 w-3.5" /></OrbitIcon>, className: 'size-[30px] border-none bg-transparent', duration: 20, delay: 10, radius: 100, path: false },
  { component: () => <OrbitIcon><Truck className="h-5 w-5" /></OrbitIcon>, className: 'size-[50px] border-none bg-transparent', radius: 210, duration: 20 },
  { component: () => <OrbitIcon><ShoppingBag className="h-5 w-5" /></OrbitIcon>, className: 'size-[50px] border-none bg-transparent', radius: 210, duration: 20, delay: 20 },
  { component: () => <OrbitIcon><Gift className="h-3.5 w-3.5" /></OrbitIcon>, className: 'size-[30px] border-none bg-transparent', radius: 150, duration: 20, delay: 20, reverse: true },
  { component: () => <OrbitIcon><PackageCheck className="h-3.5 w-3.5" /></OrbitIcon>, className: 'size-[30px] border-none bg-transparent', radius: 150, duration: 20, delay: 10, reverse: true },
  { component: () => <OrbitIcon><ShoppingCart className="h-5 w-5" /></OrbitIcon>, className: 'size-[50px] border-none bg-transparent', radius: 270, duration: 20, reverse: true },
  { component: () => <OrbitIcon><PackageOpen className="h-5 w-5" /></OrbitIcon>, className: 'size-[50px] border-none bg-transparent', radius: 270, duration: 20, delay: 60, reverse: true },
  { component: () => <OrbitIcon><Warehouse className="h-5 w-5" /></OrbitIcon>, className: 'size-[50px] border-none bg-transparent', radius: 320, duration: 20, delay: 20 },
]

export default function LoginPage() {
  const navigate = useNavigate()
  const { refresh } = useCart()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    setIsSubmitting(true)
    try {
      const tokens = await login({ email, password })
      storeTokens(tokens)
      await refresh()
      navigate('/')
    } catch (err) {
      setError(translateApiError(err, 'Giriş yapılamadı, tekrar deneyin.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="grid min-h-screen grid-cols-1 md:grid-cols-2">
      <div className="relative hidden items-center justify-center overflow-hidden border-r border-border bg-muted md:flex">
        <Ripple />
        <TechOrbitDisplay iconsArray={orbitIcons} text="E-Ticaret" />
      </div>

      <div className="flex flex-col items-center justify-center px-4 py-12">
        <Link to="/" className="mb-8 text-xl font-bold tracking-tight text-foreground" style={{ letterSpacing: '-0.02em' }}>
          E-Ticaret
        </Link>

        <AnimatedForm
          header="Giriş yap"
          subHeader="Hesabına erişmek için bilgilerini gir"
          submitButton="Giriş yap"
          textVariantButton="Hesabın yok mu? Kayıt ol"
          isSubmitting={isSubmitting}
          errorField={error ?? undefined}
          goTo={() => navigate('/register')}
          onSubmit={handleSubmit}
          fields={[
            {
              label: 'E-posta',
              required: true,
              type: 'email',
              placeholder: 'ornek@eposta.com',
              onChange: (event: ChangeEvent<HTMLInputElement>) => setEmail(event.target.value),
            },
            {
              label: 'Şifre',
              required: true,
              type: 'password',
              placeholder: '••••••••',
              onChange: (event: ChangeEvent<HTMLInputElement>) => setPassword(event.target.value),
            },
          ]}
        />
      </div>
    </div>
  )
}
