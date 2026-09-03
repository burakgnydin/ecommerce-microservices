import * as React from 'react'
import { motion } from 'framer-motion'
import { CheckCircle2, Circle, XCircle } from 'lucide-react'
import { cn } from '@/lib/utils'

export type OrderTrackingStepStatus = 'completed' | 'current' | 'cancelled' | 'upcoming'

export interface OrderTrackingStep {
  name: string
  timestamp?: string
  status: OrderTrackingStepStatus
}

export interface OrderTrackingProps extends React.HTMLAttributes<HTMLDivElement> {
  steps: OrderTrackingStep[]
}

function StepIcon({ status }: { status: OrderTrackingStepStatus }) {
  if (status === 'completed') return <CheckCircle2 className="h-6 w-6 shrink-0 text-primary" />
  if (status === 'cancelled') return <XCircle className="h-6 w-6 shrink-0 text-destructive" />
  if (status === 'current') {
    return (
      <span className="relative flex h-6 w-6 shrink-0 items-center justify-center">
        <motion.span
          className="absolute inline-flex h-full w-full rounded-full bg-primary/40"
          animate={{ scale: [1, 1.6], opacity: [0.6, 0] }}
          transition={{ duration: 1.4, repeat: Infinity, ease: 'easeOut' }}
        />
        <Circle className="relative h-6 w-6 text-primary" />
      </span>
    )
  }
  return <Circle className="h-6 w-6 shrink-0 text-muted-foreground" />
}

function lineColor(status: OrderTrackingStepStatus) {
  if (status === 'completed') return 'bg-primary'
  if (status === 'cancelled') return 'bg-destructive'
  return 'bg-muted-foreground/30'
}

const OrderTracking = React.forwardRef<HTMLDivElement, OrderTrackingProps>(
  ({ steps = [], className, ...props }, ref) => {
    return (
      <div ref={ref} className={cn('w-full max-w-md', className)} {...props}>
        {steps.length > 0 ? (
          <div>
            {steps.map((step, index) => (
              <motion.div
                key={step.name}
                className="flex"
                initial={{ opacity: 0, x: -8 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ duration: 0.25, delay: index * 0.1, ease: 'easeOut' }}
              >
                <div className="flex flex-col items-center">
                  <StepIcon status={step.status} />
                  {index < steps.length - 1 && (
                    <div className={cn('w-[1.5px] grow', lineColor(steps[index + 1].status))} />
                  )}
                </div>
                <div className="ml-3 pb-6">
                  <p
                    className={cn(
                      'text-sm font-medium',
                      step.status === 'cancelled' ? 'text-destructive' : 'text-foreground',
                    )}
                  >
                    {step.name}
                  </p>
                  {step.timestamp && <p className="text-sm text-muted-foreground">{step.timestamp}</p>}
                </div>
              </motion.div>
            ))}
          </div>
        ) : (
          <p className="text-sm text-foreground/80">Bu siparişe ait takip bilgisi yok.</p>
        )}
      </div>
    )
  },
)
OrderTracking.displayName = 'OrderTracking'

export { OrderTracking }
