import { Skeleton } from './Skeleton'

export function ArticleCardSkeleton() {
  return (
    <div className="relative h-64 overflow-hidden rounded-xl sm:h-72">
      <Skeleton className="h-full w-full rounded-xl" />
      <Skeleton className="absolute left-4 top-4 h-6 w-16 rounded-full" />
      <Skeleton className="absolute inset-x-4 bottom-4 h-11 rounded-lg" />
    </div>
  )
}
