import { type HTMLAttributes, useCallback, useEffect, useState } from 'react'
import { X } from 'lucide-react'
import { cn } from '@/lib/utils'
import { buttonVariants } from '@/components/ui/Button'

interface BannerProps extends HTMLAttributes<HTMLDivElement> {
  /** @defaultValue 'normal' */
  variant?: 'rainbow' | 'normal'
  /** Banner message */
  message?: string
  /** @defaultValue '3rem' */
  height?: string
  /** Persist dismissal in localStorage under this key */
  id?: string
}

export function Banner({ id, variant = 'normal', message, height = '3rem', ...props }: BannerProps) {
  const [open, setOpen] = useState(true)
  const storageKey = id ? `banner-${id}` : undefined

  useEffect(() => {
    if (storageKey) setOpen(localStorage.getItem(storageKey) !== 'true')
  }, [storageKey])

  const onClick = useCallback(() => {
    setOpen(false)
    if (storageKey) localStorage.setItem(storageKey, 'true')
  }, [storageKey])

  if (!open) return null

  return (
    <div
      id={id}
      {...props}
      style={{ height }}
      className={cn(
        'sticky top-0 z-40 flex flex-row items-center justify-center bg-secondary px-4 text-center text-sm font-medium',
        variant === 'rainbow' && 'bg-background',
        props.className,
      )}
    >
      {variant === 'rainbow' ? <RainbowLayer /> : null}
      {message || props.children}
      <button
        type="button"
        aria-label="Banner'ı kapat"
        onClick={onClick}
        className={cn(buttonVariants({ variant: 'ghost', size: 'icon' }), 'absolute end-2 top-1/2 -translate-y-1/2 text-muted-foreground')}
      >
        <X className="h-4 w-4" />
      </button>
    </div>
  )
}

function RainbowLayer() {
  return (
    <>
      <div className="absolute inset-0 z-[-1] rainbow-banner-gradient-1" />
      <div className="absolute inset-0 z-[-1] rainbow-banner-gradient-2" />
    </>
  )
}
