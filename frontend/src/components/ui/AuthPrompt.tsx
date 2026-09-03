import { Lock } from 'lucide-react'
import { Link } from 'react-router-dom'
import { buttonVariants } from './Button'
import { Card } from './Card'

interface AuthPromptProps {
  message: string
  /** @defaultValue 'block' */
  variant?: 'block' | 'inline'
  className?: string
}

export function AuthPrompt({ message, variant = 'block', className = '' }: AuthPromptProps) {
  if (variant === 'inline') {
    return (
      <div className={`flex flex-wrap items-center gap-3 rounded-lg border border-border bg-card px-4 py-3 ${className}`}>
        <Lock className="h-4 w-4 shrink-0 text-muted-foreground" />
        <p className="flex-1 text-sm text-muted-foreground">{message}</p>
        <div className="flex items-center gap-2">
          <Link to="/login" className={buttonVariants({ size: 'sm' })}>
            Giriş yap
          </Link>
          <Link to="/register" className={buttonVariants({ variant: 'outline', size: 'sm' })}>
            Kayıt ol
          </Link>
        </div>
      </div>
    )
  }

  return (
    <Card className={`flex flex-col items-center gap-3 p-10 text-center ${className}`}>
      <div className="flex h-12 w-12 items-center justify-center rounded-full bg-primary/10 text-primary">
        <Lock className="h-6 w-6" />
      </div>
      <p className="text-muted-foreground">{message}</p>
      <div className="mt-1 flex items-center gap-3">
        <Link to="/login" className={buttonVariants({ size: 'sm' })}>
          Giriş yap
        </Link>
        <Link to="/register" className={buttonVariants({ variant: 'outline', size: 'sm' })}>
          Kayıt ol
        </Link>
      </div>
    </Card>
  )
}
