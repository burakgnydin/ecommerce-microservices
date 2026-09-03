import type { InputHTMLAttributes } from 'react'

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  error?: boolean
}

export function Input({ error = false, className = '', ...props }: InputProps) {
  return (
    <input
      className={`h-10 w-full rounded-md border bg-white px-3 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-offset-0 disabled:cursor-not-allowed disabled:bg-secondary ${
        error
          ? 'border-destructive focus:ring-destructive/20'
          : 'border-input hover:border-muted-foreground focus:border-primary focus:ring-primary/20'
      } ${className}`}
      {...props}
    />
  )
}
