import { Package } from 'lucide-react'
import { getProductGradient } from '../lib/productImage'

interface ProductImagePlaceholderProps {
  productId: string
  className?: string
  iconClassName?: string
}

export function ProductImagePlaceholder({
  productId,
  className = '',
  iconClassName = 'h-10 w-10',
}: ProductImagePlaceholderProps) {
  return (
    <div
      className={`flex items-center justify-center bg-gradient-to-br ${getProductGradient(productId)} ${className}`}
    >
      <Package className={`text-white/90 ${iconClassName}`} strokeWidth={1.5} />
    </div>
  )
}
