import * as React from 'react'
import { AnimatePresence, motion } from 'framer-motion'
import { X } from 'lucide-react'
import { cn } from '@/lib/utils'
import { Button } from './Button'

type AlertCardVariant = 'destructive' | 'success'

interface AlertCardProps {
  variant?: AlertCardVariant
  icon?: React.ReactNode
  title: string
  description: string
  buttonText?: string
  onButtonClick?: () => void
  isVisible: boolean
  onDismiss?: () => void
  className?: string
}

const variantClasses: Record<AlertCardVariant, string> = {
  destructive: 'bg-destructive text-destructive-foreground',
  success: 'bg-success text-success-foreground',
}

export function AlertCard({
  variant = 'destructive',
  icon,
  title,
  description,
  buttonText,
  onButtonClick,
  isVisible,
  onDismiss,
  className,
}: AlertCardProps) {
  return (
    <AnimatePresence>
      {isVisible && (
        <motion.div
          initial={{ opacity: 0, y: 24, scale: 0.95 }}
          animate={{ opacity: 1, y: 0, scale: 1 }}
          exit={{ opacity: 0, y: 12, scale: 0.98 }}
          transition={{ type: 'spring', stiffness: 400, damping: 30 }}
          role="alert"
          aria-live="assertive"
          className={cn(
            'relative w-full max-w-sm overflow-hidden rounded-2xl p-5 shadow-lg',
            variantClasses[variant],
            className,
          )}
        >
          {onDismiss && (
            <button
              type="button"
              onClick={onDismiss}
              aria-label="Kapat"
              className="absolute right-3 top-3 flex h-7 w-7 items-center justify-center rounded-full hover:bg-white/20"
            >
              <X className="h-4 w-4" />
            </button>
          )}

          {icon && <div className="mb-3 flex h-9 w-9 items-center justify-center rounded-full bg-white/15">{icon}</div>}

          <h3 className="pr-6 text-base font-semibold">{title}</h3>
          <p className="mt-1 text-sm opacity-90">{description}</p>

          {buttonText && onButtonClick && (
            <Button
              size="sm"
              onClick={onButtonClick}
              className="mt-4 w-full bg-white text-foreground hover:bg-white/90"
            >
              {buttonText}
            </Button>
          )}
        </motion.div>
      )}
    </AnimatePresence>
  )
}
