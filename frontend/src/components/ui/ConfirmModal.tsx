import type { ComponentType } from 'react'
import * as AlertDialogPrimitive from '@radix-ui/react-alert-dialog'
import { AnimatePresence, motion } from 'framer-motion'
import { AlertTriangle } from 'lucide-react'
import { Card } from './Card'
import { Button } from './Button'

interface ConfirmModalProps {
  isOpen: boolean
  title: string
  message: string
  confirmText?: string
  cancelText?: string
  isDestructive?: boolean
  icon?: ComponentType<{ className?: string }>
  onConfirm: () => void
  onCancel: () => void
}

export function ConfirmModal({
  isOpen,
  title,
  message,
  confirmText = 'Onayla',
  cancelText = 'Vazgeç',
  isDestructive = false,
  icon,
  onConfirm,
  onCancel,
}: ConfirmModalProps) {
  const Icon = icon ?? (isDestructive ? AlertTriangle : undefined)

  return (
    <AlertDialogPrimitive.Root open={isOpen} onOpenChange={(open) => !open && onCancel()}>
      <AnimatePresence>
        {isOpen && (
          <AlertDialogPrimitive.Portal forceMount>
            <AlertDialogPrimitive.Overlay asChild forceMount>
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                transition={{ duration: 0.15 }}
                className="fixed inset-0 z-50 bg-black/50"
              />
            </AlertDialogPrimitive.Overlay>
            <AlertDialogPrimitive.Content asChild forceMount>
              <motion.div
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                exit={{ opacity: 0, scale: 0.95 }}
                transition={{ duration: 0.15 }}
                className="fixed inset-0 z-50 flex items-center justify-center px-4"
              >
                <Card className="w-full max-w-sm p-6">
                  {Icon && (
                    <div
                      className={`mb-4 flex h-10 w-10 items-center justify-center rounded-full ${
                        isDestructive ? 'bg-destructive/10 text-destructive' : 'bg-primary/10 text-primary'
                      }`}
                    >
                      <Icon className="h-5 w-5" />
                    </div>
                  )}
                  <AlertDialogPrimitive.Title className="text-lg font-semibold text-foreground">
                    {title}
                  </AlertDialogPrimitive.Title>
                  <AlertDialogPrimitive.Description className="mt-2 text-sm text-muted-foreground">
                    {message}
                  </AlertDialogPrimitive.Description>
                  <div className="mt-6 flex justify-end gap-3">
                    <AlertDialogPrimitive.Cancel asChild>
                      <Button variant="outline" onClick={onCancel}>
                        {cancelText}
                      </Button>
                    </AlertDialogPrimitive.Cancel>
                    <AlertDialogPrimitive.Action asChild>
                      <Button variant={isDestructive ? 'destructive' : 'default'} onClick={onConfirm}>
                        {confirmText}
                      </Button>
                    </AlertDialogPrimitive.Action>
                  </div>
                </Card>
              </motion.div>
            </AlertDialogPrimitive.Content>
          </AlertDialogPrimitive.Portal>
        )}
      </AnimatePresence>
    </AlertDialogPrimitive.Root>
  )
}
