import { useEffect, useState } from 'react'
import { CreditCard, ListOrdered, Package, Tags } from 'lucide-react'
import { cn } from '@/lib/utils'

const overviewSlides = [
  { id: 1, title: 'Ürünler', description: 'Ürün kataloğunu ekle, düzenle ve yönet.', icon: Package },
  { id: 2, title: 'Kategoriler', description: 'Ürünleri kategorilere ayırarak düzenle.', icon: Tags },
  { id: 3, title: 'Siparişler', description: 'Gelen siparişleri takip et ve yönet.', icon: ListOrdered },
  { id: 4, title: 'Ödemeler', description: 'Ödeme durumlarına genel bakış.', icon: CreditCard },
]

export function AdminOverviewSlider() {
  const [currentSlide, setCurrentSlide] = useState(0)

  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentSlide((prev) => (prev === overviewSlides.length - 1 ? 0 : prev + 1))
    }, 5000)
    return () => clearInterval(interval)
  }, [])

  return (
    <div className="py-6">
      <div className="relative h-56 overflow-hidden">
        <div className="absolute inset-0 flex items-center justify-center">
          {overviewSlides.map((slide, index) => {
            const position = index - currentSlide
            const isActive = position === 0
            const Icon = slide.icon

            return (
              <div
                key={slide.id}
                className={cn(
                  'absolute w-72 rounded-2xl border bg-card p-6 transition-all duration-500 ease-in-out',
                  isActive ? 'border-border shadow-lg' : 'border-border/60 shadow-sm',
                )}
                style={{
                  transform: `translateX(${position * 110}%) scale(${isActive ? 1 : 0.9})`,
                  zIndex: isActive ? 30 : 20 - Math.abs(position),
                  opacity: Math.abs(position) > 1 ? 0 : 1,
                }}
              >
                <div className="flex h-12 w-12 items-center justify-center rounded-full bg-primary/10 text-primary">
                  <Icon className="h-6 w-6" />
                </div>
                <h3 className="mt-4 text-lg font-semibold text-foreground">{slide.title}</h3>
                <p className="mt-1 text-sm text-muted-foreground">{slide.description}</p>
              </div>
            )
          })}
        </div>
      </div>

      <div className="flex justify-center gap-6">
        {overviewSlides.map((slide, index) => (
          <button
            key={slide.id}
            type="button"
            onClick={() => setCurrentSlide(index)}
            className={cn(
              'text-sm font-medium transition-colors',
              currentSlide === index ? 'text-foreground' : 'text-muted-foreground hover:text-foreground',
            )}
          >
            {slide.title}
          </button>
        ))}
      </div>
    </div>
  )
}
