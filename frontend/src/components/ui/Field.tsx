import type { InputHTMLAttributes } from 'react'
import { Input } from './Input'

interface FieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string
  error?: string
}

export function Field({ label, error, id, ...props }: FieldProps) {
  return (
    <div>
      <label htmlFor={id} className="mb-1.5 block text-sm font-medium text-foreground">
        {label}
      </label>
      <Input id={id} error={Boolean(error)} {...props} />
      {error && <p className="mt-1.5 text-sm text-destructive">{error}</p>}
    </div>
  )
}
