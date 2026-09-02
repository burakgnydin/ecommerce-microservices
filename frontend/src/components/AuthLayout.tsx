import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { Card } from './ui/Card'

interface AuthLayoutProps {
  title: string
  subtitle: string
  children: ReactNode
}

export function AuthLayout({ title, subtitle, children }: AuthLayoutProps) {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center px-4">
      <Link
        to="/"
        className="mb-6 text-xl font-bold tracking-tight text-foreground"
        style={{ letterSpacing: '-0.02em' }}
      >
        E-Ticaret
      </Link>
      <Card className="w-full max-w-sm overflow-hidden p-8">
        <div className="-mx-8 -mt-8 mb-6 h-1 bg-gradient-to-r from-primary to-primary/40" />
        <h1 className="text-2xl font-semibold tracking-tight text-foreground">{title}</h1>
        <p className="mt-1 text-sm text-muted-foreground">{subtitle}</p>
        <div className="mt-6">{children}</div>
      </Card>
    </div>
  )
}
