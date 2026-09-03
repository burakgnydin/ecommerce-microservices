import { type ChangeEvent, type FormEvent, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Boxes, Gift, Package, PackageCheck, PackageOpen, ShoppingBag, ShoppingCart, Truck, Warehouse } from 'lucide-react'
import { login } from '../api/auth'
import { translateApiError } from '../api/client'
import { AnimatedForm, Ripple, TechOrbitDisplay } from '../components/ui/AnimatedSignIn'
import { useCart } from '../context/CartContext'
import { storeTokens } from '../lib/auth'

function OrbitIcon({ children, pulseDelay = 0 }: { children: React.ReactNode; pulseDelay?: number }) {
  return (
    <motion.div
      className="flex h-full w-full items-center justify-center rounded-full border border-border text-foreground shadow-sm"
      animate={{ backgroundColor: ['#ffffff', '#3b82f6', '#ffffff'] }}
      transition={{ duration: 5, repeat: Infinity, ease: 'easeInOut', delay: pulseDelay }}
    >
      {children}
    </motion.div>
  )
}

const orbitIcons = [
  { component: () => <OrbitIcon pulseDelay={0}><Package className="h-3.5 w-3.5" /></OrbitIcon>, className: 'size-[30px] border-none bg-transparent', duration: 20, delay: 20, radius: 100, path: false },
  { component: () => <OrbitIcon pulseDelay={0.4}><Boxes className="h-3.5 w-3.5" /></OrbitIcon>, className: 'size-[30px] border-none bg-transparent', duration: 20, delay: 10, radius: 100, path: false },
  { component: () => <OrbitIcon pulseDelay={0.8}><Truck className="h-5 w-5" /></OrbitIcon>, className: 'size-[50px] border-none bg-transparent', radius: 210, duration: 20 },
  { component: () => <OrbitIcon pulseDelay={1.2}><ShoppingBag className="h-5 w-5" /></OrbitIcon>, className: 'size-[50px] border-none bg-transparent', radius: 210, duration: 20, delay: 20 },
  { component: () => <OrbitIcon pulseDelay={1.6}><Gift className="h-3.5 w-3.5" /></OrbitIcon>, className: 'size-[30px] border-none bg-transparent', radius: 150, duration: 20, delay: 20, reverse: true },
  { component: () => <OrbitIcon pulseDelay={2}><PackageCheck className="h-3.5 w-3.5" /></OrbitIcon>, className: 'size-[30px] border-none bg-transparent', radius: 150, duration: 20, delay: 10, reverse: true },
  { component: () => <OrbitIcon pulseDelay={2.4}><ShoppingCart className="h-5 w-5" /></OrbitIcon>, className: 'size-[50px] border-none bg-transparent', radius: 270, duration: 20, reverse: true },
  { component: () => <OrbitIcon pulseDelay={2.8}><PackageOpen className="h-5 w-5" /></OrbitIcon>, className: 'size-[50px] border-none bg-transparent', radius: 270, duration: 20, delay: 60, reverse: true },
  { component: () => <OrbitIcon pulseDelay={3.2}><Warehouse className="h-5 w-5" /></OrbitIcon>, className: 'size-[50px] border-none bg-transparent', radius: 320, duration: 20, delay: 20 },
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
