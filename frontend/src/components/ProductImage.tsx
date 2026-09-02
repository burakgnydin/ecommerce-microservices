import { useState } from 'react'
import type { Product } from '../api/types'
import { ProductImagePlaceholder } from './ProductImagePlaceholder'

interface ProductImageProps {
  product: Pick<Product, 'id' | 'name' | 'imageUrl'>
  className?: string
  iconClassName?: string
}

export function ProductImage({ product, className = '', iconClassName }: ProductImageProps) {
  const [failed, setFailed] = useState(false)

  if (!product.imageUrl || failed) {
    return <ProductImagePlaceholder productId={product.id} className={className} iconClassName={iconClassName} />
  }

  return (
    <img
      src={product.imageUrl}
      alt={product.name}
      loading="lazy"
      className={`object-cover ${className}`}
      onError={() => setFailed(true)}
    />
  )
}
