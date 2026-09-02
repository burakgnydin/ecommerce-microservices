import { Skeleton } from './ui/Skeleton'

export function ProductDetailSkeleton() {
  return (
    <div className="overflow-hidden rounded-lg border border-border bg-card">
      <Skeleton className="h-72 w-full rounded-none" />
      <div className="space-y-4 p-8">
        <Skeleton className="h-5 w-24 rounded-full" />
        <Skeleton className="h-8 w-2/3" />
        <Skeleton className="h-7 w-32" />
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-4 w-4/5" />
      </div>
    </div>
  )
}
