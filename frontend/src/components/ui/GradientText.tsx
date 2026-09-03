import { type ReactNode } from 'react'
import { motion, type MotionProps } from 'framer-motion'
import { cn } from '@/lib/utils'

interface GradientTextProps {
  children: ReactNode
  className?: string
  as?: React.ElementType
}

export function GradientText({ children, className, as: Component = 'span' }: GradientTextProps) {
  const MotionComponent = motion.create(Component) as React.ComponentType<
    { className?: string; children?: ReactNode } & MotionProps
  >

  return (
    <MotionComponent
      className={cn(
        'relative inline-flex bg-[length:250%_100%,auto] bg-clip-text text-transparent',
        '[background-image:var(--bg-light)]',
        className,
      )}
      style={
        {
          '--bg-light':
            'linear-gradient(to right, #93c5fd, #3b82f6, #1d4ed8, #60a5fa, #93c5fd)',
        } as React.CSSProperties
      }
      initial={{ backgroundPosition: '0% 50%' }}
      animate={{ backgroundPosition: '100% 50%' }}
      transition={{ duration: 3, repeat: Infinity, repeatType: 'reverse', ease: 'linear' }}
    >
      {children}
    </MotionComponent>
  )
}

export default GradientText
