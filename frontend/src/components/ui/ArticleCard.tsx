import React from 'react'
import { Tags } from 'lucide-react'
import { cn } from '@/lib/utils'

interface ArticleCardProps extends React.HTMLAttributes<HTMLDivElement> {
  /** Category title shown on the card. */
  title: string
  /** Number of products in this category. */
  count: number
  /** Background image URL, or null to fall back to a placeholder. */
  imageUrl: string | null
}

export const ArticleCard = React.forwardRef<HTMLDivElement, ArticleCardProps>(
  ({ title, count, imageUrl, className, ...props }, ref) => {
    return (
      <div
        ref={ref}
        className={cn(
          'group relative h-64 cursor-pointer overflow-hidden rounded-xl bg-cover bg-center shadow-sm transition-all duration-300 ease-in-out hover:-translate-y-1 hover:scale-[1.02] hover:shadow-xl sm:h-72',
          !imageUrl && 'bg-muted',
          className,
        )}
        style={imageUrl ? { backgroundImage: `url(${imageUrl})` } : undefined}
        {...props}
      >
        {!imageUrl && (
          <div className="absolute inset-0 flex items-center justify-center">
            <Tags className="h-10 w-10 text-muted-foreground" strokeWidth={1.5} />
          </div>
        )}

        <span className="absolute left-4 top-4 rounded-full bg-card/90 px-3 py-1 text-xs font-semibold text-card-foreground shadow-sm backdrop-blur-sm">
          {count} ürün
        </span>

        <div className="absolute inset-x-4 bottom-4 rounded-lg bg-card/90 px-4 py-3 shadow-sm backdrop-blur-sm">
          <h2 className="text-lg font-semibold text-card-foreground">{title}</h2>
        </div>
      </div>
    )
  },
)

ArticleCard.displayName = 'ArticleCard'
