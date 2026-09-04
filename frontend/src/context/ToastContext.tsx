import { createContext, useCallback, useContext, useState, type ReactNode } from 'react'
import { AlertCard } from '../components/ui/AlertCard'

interface Toast {
  id: number
  variant: 'destructive' | 'success'
  title: string
  description: string
}

interface ToastContextValue {
  showError: (title: string, description: string) => void
  showSuccess: (title: string, description: string) => void
}

const ToastContext = createContext<ToastContextValue | null>(null)

const AUTO_DISMISS_MS = 5000

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([])

  const dismiss = useCallback((id: number) => {
    setToasts((prev) => prev.filter((t) => t.id !== id))
  }, [])

  const push = useCallback(
    (variant: Toast['variant'], title: string, description: string) => {
      const id = Date.now() + Math.random()
      setToasts((prev) => [...prev, { id, variant, title, description }])
      setTimeout(() => dismiss(id), AUTO_DISMISS_MS)
    },
    [dismiss],
  )

  const showError = useCallback((title: string, description: string) => push('destructive', title, description), [push])
  const showSuccess = useCallback((title: string, description: string) => push('success', title, description), [push])

  return (
    <ToastContext.Provider value={{ showError, showSuccess }}>
      {children}
      <div className="fixed bottom-4 right-4 z-[60] flex flex-col gap-3">
        {toasts.map((toast) => (
          <AlertCard
            key={toast.id}
            isVisible
            variant={toast.variant}
            title={toast.title}
            description={toast.description}
            onDismiss={() => dismiss(toast.id)}
          />
        ))}
      </div>
    </ToastContext.Provider>
  )
}

export function useToast() {
  const ctx = useContext(ToastContext)
  if (!ctx) throw new Error('useToast must be used within a ToastProvider')
  return ctx
}
