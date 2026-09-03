import { type FormEvent, useState } from 'react'
import type { Category, Product, ProductCreateRequest } from '../../api/types'
import { Button } from '../ui/Button'
import { Card } from '../ui/Card'

const inputClasses =
  'h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground outline-none focus-visible:ring-2 focus-visible:ring-ring'

interface ProductFormProps {
  initialValue?: Product
  categories: Category[]
  isSubmitting: boolean
  onSubmit: (dto: ProductCreateRequest) => void
  onCancel: () => void
}

export function ProductForm({ initialValue, categories, isSubmitting, onSubmit, onCancel }: ProductFormProps) {
  const [name, setName] = useState(initialValue?.name ?? '')
  const [description, setDescription] = useState(initialValue?.description ?? '')
  const [price, setPrice] = useState(String(initialValue?.price ?? ''))
  const [stock, setStock] = useState(String(initialValue?.stock ?? ''))
  const [categoryId, setCategoryId] = useState(initialValue?.categoryId ?? categories[0]?.id ?? '')
  const [allowsPreOrder, setAllowsPreOrder] = useState(initialValue?.allowsPreOrder ?? false)
  const [imageUrl, setImageUrl] = useState(initialValue?.imageUrl ?? '')

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    onSubmit({
      name,
      description: description || null,
      price: Number(price),
      stock: Number(stock),
      categoryId,
      allowsPreOrder,
      imageUrl: imageUrl || null,
    })
  }

  return (
    <Card className="p-6">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="product-name" className="mb-1 block text-sm font-medium text-muted-foreground">
            Ad
          </label>
          <input
            id="product-name"
            type="text"
            required
            value={name}
            onChange={(e) => setName(e.target.value)}
            className={inputClasses}
          />
        </div>

        <div>
          <label htmlFor="product-description" className="mb-1 block text-sm font-medium text-muted-foreground">
            Açıklama
          </label>
          <textarea
            id="product-description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={3}
            className="w-full rounded-md border border-border bg-card px-3 py-2 text-sm text-foreground outline-none focus-visible:ring-2 focus-visible:ring-ring"
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label htmlFor="product-price" className="mb-1 block text-sm font-medium text-muted-foreground">
              Fiyat
            </label>
            <input
              id="product-price"
              type="number"
              min="0"
              step="0.01"
              required
              value={price}
              onChange={(e) => setPrice(e.target.value)}
              className={inputClasses}
            />
          </div>

          <div>
            <label htmlFor="product-stock" className="mb-1 block text-sm font-medium text-muted-foreground">
              Stok
            </label>
            <input
              id="product-stock"
              type="number"
              min="0"
              required
              value={stock}
              onChange={(e) => setStock(e.target.value)}
              className={inputClasses}
            />
          </div>
        </div>

        <div>
          <label htmlFor="product-category" className="mb-1 block text-sm font-medium text-muted-foreground">
            Kategori
          </label>
          <select
            id="product-category"
            required
            value={categoryId}
            onChange={(e) => setCategoryId(e.target.value)}
            className={inputClasses}
          >
            {categories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="product-image-url" className="mb-1 block text-sm font-medium text-muted-foreground">
            Görsel URL
          </label>
          <input
            id="product-image-url"
            type="url"
            value={imageUrl}
            onChange={(e) => setImageUrl(e.target.value)}
            className={inputClasses}
          />
        </div>

        <label className="flex items-center gap-2 text-sm text-foreground">
          <input
            type="checkbox"
            checked={allowsPreOrder}
            onChange={(e) => setAllowsPreOrder(e.target.checked)}
            className="h-4 w-4 rounded border-border"
          />
          Ön siparişe izin ver
        </label>

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={onCancel}>
            Vazgeç
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Kaydediliyor...' : 'Kaydet'}
          </Button>
        </div>
      </form>
    </Card>
  )
}
