import * as React from 'react'
import { AnimatePresence, motion } from 'framer-motion'
import { CheckCircle2 } from 'lucide-react'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/Button'

interface OrderConfirmationCardProps {
  orderId: string
  paymentMethod: string
  dateTime: string
  totalAmount: string
  onGoToAccount: () => void
  title?: string
  buttonText?: string
  icon?: React.ReactNode
  className?: string
}

export const OrderConfirmationCard: React.FC<OrderConfirmationCardProps> = ({
  orderId,
  paymentMethod,
  dateTime,
  totalAmount,
  onGoToAccount,
  title = 'Siparişin başarıyla alındı',
  buttonText = 'Hesabıma git',
  icon = <CheckCircle2 className="h-12 w-12 text-success" />,
  className,
}) => {
  const details = [
    { label: 'Sipariş No', value: orderId },
    { label: 'Ödeme Yöntemi', value: paymentMethod },
    { label: 'Tarih & Saat', value: dateTime },
    { label: 'Toplam', value: totalAmount, isBold: true },
  ]

  const containerVariants = {
    hidden: { opacity: 0, scale: 0.95 },
    visible: {
      opacity: 1,
      scale: 1,
      transition: { duration: 0.4, ease: 'easeInOut' as const, staggerChildren: 0.1 },
    },
  }

  const itemVariants = {
    hidden: { opacity: 0, y: 20 },
    visible: { opacity: 1, y: 0, transition: { type: 'spring' as const, stiffness: 100 } },
  }

  return (
    <AnimatePresence>
      <motion.div
        variants={containerVariants}
        initial="hidden"
        animate="visible"
        aria-live="polite"
        className={cn(
          'w-full max-w-sm rounded-xl border border-border bg-card p-6 text-card-foreground shadow-lg sm:p-8',
          className,
        )}
      >
        <div className="flex flex-col items-center space-y-6 text-center">
          <motion.div variants={itemVariants}>{icon}</motion.div>

          <motion.h2 variants={itemVariants} className="text-2xl font-semibold">
            {title}
          </motion.h2>

          <motion.div variants={itemVariants} className="w-full space-y-4 pt-4">
            {details.map((item, index) => (
              <div
                key={item.label}
                className={cn('flex items-center justify-between border-b border-border pb-4 text-sm text-muted-foreground', {
                  'border-none pb-0': index === details.length - 1,
                  'font-bold text-card-foreground': item.isBold,
                })}
              >
                <span>{item.label}</span>
                <span className={cn({ 'text-lg': item.isBold })}>{item.value}</span>
              </div>
            ))}
          </motion.div>

          <motion.div variants={itemVariants} className="w-full pt-4">
            <Button onClick={onGoToAccount} className="h-12 w-full text-base" size="lg">
              {buttonText}
            </Button>
          </motion.div>
        </div>
      </motion.div>
    </AnimatePresence>
  )
}
