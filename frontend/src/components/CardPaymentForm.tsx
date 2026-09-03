import { type FormEvent, useState } from 'react'
import { ApiError } from '../api/client'
import { charge } from '../api/payments'
import type { Payment } from '../api/types'
import { Button } from './ui/Button'

interface CardPaymentFormProps {
  orderId: string
  onSuccess: (payment: Payment) => void
}

const inputClasses =
  'h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground outline-none focus-visible:ring-2 focus-visible:ring-ring'

export function CardPaymentForm({ orderId, onSuccess }: CardPaymentFormProps) {
  const [cardNumber, setCardNumber] = useState('')
  const [expiryMonth, setExpiryMonth] = useState('')
  const [expiryYear, setExpiryYear] = useState('')
  const [cvv, setCvv] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError(null)
    setIsSubmitting(true)
    try {
      const payment = await charge({
        orderId,
        cardNumber: cardNumber.replace(/\s/g, ''),
        expiryMonth: Number(expiryMonth),
        expiryYear: Number(expiryYear),
        cvv,
      })
      onSuccess(payment)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Ödeme alınamadı.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label htmlFor="cardNumber" className="mb-1 block text-sm font-medium text-muted-foreground">
          Kart Numarası
        </label>
        <input
          id="cardNumber"
          type="text"
          inputMode="numeric"
          autoComplete="cc-number"
          required
          maxLength={19}
          placeholder="4242 4242 4242 4242"
          value={cardNumber}
          onChange={(e) => setCardNumber(e.target.value)}
          className={inputClasses}
        />
      </div>

      <div className="flex gap-3">
        <div className="flex-1">
          <label htmlFor="expiryMonth" className="mb-1 block text-sm font-medium text-muted-foreground">
            Ay
          </label>
          <input
            id="expiryMonth"
            type="text"
            inputMode="numeric"
            autoComplete="cc-exp-month"
            required
            maxLength={2}
            placeholder="12"
            value={expiryMonth}
            onChange={(e) => setExpiryMonth(e.target.value)}
            className={inputClasses}
          />
        </div>
        <div className="flex-1">
          <label htmlFor="expiryYear" className="mb-1 block text-sm font-medium text-muted-foreground">
            Yıl
          </label>
          <input
            id="expiryYear"
            type="text"
            inputMode="numeric"
            autoComplete="cc-exp-year"
            required
            maxLength={4}
            placeholder="2028"
            value={expiryYear}
            onChange={(e) => setExpiryYear(e.target.value)}
            className={inputClasses}
          />
        </div>
        <div className="w-24">
          <label htmlFor="cvv" className="mb-1 block text-sm font-medium text-muted-foreground">
            CVV
          </label>
          <input
            id="cvv"
            type="text"
            inputMode="numeric"
            autoComplete="cc-csc"
            required
            maxLength={4}
            placeholder="123"
            value={cvv}
            onChange={(e) => setCvv(e.target.value)}
            className={inputClasses}
          />
        </div>
      </div>

      {error && <p className="text-sm text-destructive">{error}</p>}

      <Button type="submit" disabled={isSubmitting} className="h-11 w-full">
        {isSubmitting ? 'Ödeme işleniyor...' : 'Ödemeyi Tamamla'}
      </Button>
    </form>
  )
}
