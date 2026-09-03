import type { ReactNode } from 'react'
import { motion } from 'framer-motion'
import { cn } from '@/lib/utils'

type Variant = 'default' | 'secondary' | 'destructive' | 'primary'

interface ShimmerTextProps {
  children: ReactNode
  className?: string
  variant?: Variant
  duration?: number
  delay?: number
}

const variantMap: Record<Variant, string> = {
  default: '',
  secondary: 'text-secondary-foreground',
  destructive: 'text-destructive',
  primary: 'text-primary',
}

export function ShimmerText({ children, className, variant = 'default', duration = 1.5, delay = 1.5 }: ShimmerTextProps) {
  return (
    <span className="group inline-block overflow-hidden align-bottom">
      <span className="inline-block">
        <motion.span
          className={cn('inline-block [--shimmer-contrast:rgba(255,255,255,0.6)]', variantMap[variant], className)}
          style={
            {
              WebkitTextFillColor: 'transparent',
              background:
                'currentColor linear-gradient(to right, currentColor 0%, var(--shimmer-contrast) 40%, var(--shimmer-contrast) 60%, currentColor 100%)',
              WebkitBackgroundClip: 'text',
              backgroundClip: 'text',
              backgroundRepeat: 'no-repeat',
              backgroundSize: '50% 200%',
            } as React.CSSProperties
          }
          initial={{ backgroundPositionX: '250%' }}
          animate={{ backgroundPositionX: ['-100%', '250%'] }}
          transition={{ duration, delay, repeat: Infinity, repeatDelay: 1.5, ease: 'linear' }}
        >
          <span>{children}</span>
        </motion.span>
      </span>
    </span>
  )
}

export default ShimmerText
