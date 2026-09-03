import type { ButtonHTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

type Variant = 'default' | 'secondary' | 'outline' | 'ghost' | 'destructive' | 'link'
type Size = 'sm' | 'default' | 'lg' | 'icon'

const variantClasses: Record<Variant, string> = {
  default: 'bg-primary text-primary-foreground hover:bg-primary-hover',
  secondary: 'bg-secondary text-secondary-foreground hover:bg-secondary/80',
  outline: 'border border-border bg-transparent text-foreground hover:bg-secondary',
  ghost: 'bg-transparent text-foreground hover:bg-secondary',
  destructive: 'bg-destructive text-destructive-foreground hover:bg-destructive/90',
  link: 'bg-transparent text-primary underline-offset-4 hover:underline',
}

const sizeClasses: Record<Size, string> = {
  sm: 'h-8 px-3 text-sm',
  default: 'h-10 px-4 text-sm',
  lg: 'h-12 px-6 text-base',
  icon: 'h-8 w-8',
}

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant
  size?: Size
}

export function buttonVariants({
  variant = 'default',
  size = 'default',
  className = '',
}: { variant?: Variant; size?: Size; className?: string } = {}) {
  return cn(
    'inline-flex items-center justify-center gap-2 rounded-md font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50',
    variantClasses[variant],
    sizeClasses[size],
    className,
  )
}

export function Button({
  variant = 'default',
  size = 'default',
  className = '',
  ...props
}: ButtonProps) {
  return <button className={buttonVariants({ variant, size, className })} {...props} />
}
