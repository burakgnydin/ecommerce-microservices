import type { ReactNode } from 'react'
import { motion } from 'framer-motion'
import { cn } from '@/lib/utils'

export function Highlight({ children, className }: { children: ReactNode; className?: string }) {
  return (
    <motion.span
      initial={{ backgroundSize: '0% 100%' }}
      animate={{ backgroundSize: '100% 100%' }}
      transition={{ duration: 2, ease: 'linear', delay: 0.5 }}
      style={{ backgroundRepeat: 'no-repeat', backgroundPosition: 'left center', display: 'inline' }}
      className={cn('relative inline-block rounded-lg bg-gradient-to-r from-blue-200 to-blue-300 px-1 pb-1', className)}
    >
      {children}
    </motion.span>
  )
}
