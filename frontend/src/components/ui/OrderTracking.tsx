import * as React from 'react'
import { CheckCircle2, Circle } from 'lucide-react'
import { cn } from '@/lib/utils'

export interface OrderTrackingStep {
  name: string
  timestamp: string
  isCompleted: boolean
}

export interface OrderTrackingProps extends React.HTMLAttributes<HTMLDivElement> {
  steps: OrderTrackingStep[]
}

const OrderTracking = React.forwardRef<HTMLDivElement, OrderTrackingProps>(
  ({ steps = [], className, ...props }, ref) => {
    return (
      <div ref={ref} className={cn('w-full max-w-md', className)} {...props}>
        {steps.length > 0 ? (
          <div>
            {steps.map((step, index) => (
              <div key={step.name} className="flex">
                <div className="flex flex-col items-center">
                  {step.isCompleted ? (
                    <CheckCircle2 className="h-6 w-6 shrink-0 text-primary/70" />
                  ) : (
                    <Circle className="h-6 w-6 shrink-0 text-muted-foreground" />
                  )}
                  {index < steps.length - 1 && (
                    <div
                      className={cn('w-[1.5px] grow', {
                        'bg-primary/70': steps[index + 1].isCompleted,
                        'bg-muted-foreground': !steps[index + 1].isCompleted,
                      })}
                    />
                  )}
                </div>
                <div className="ml-3 pb-6">
                  <p className="text-sm font-medium text-foreground">{step.name}</p>
                  <p className="text-sm text-muted-foreground">{step.timestamp}</p>
                </div>
              </div>
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
