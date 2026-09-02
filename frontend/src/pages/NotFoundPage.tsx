import { Link } from 'react-router-dom'
import { Header } from '../components/Header'
import { Button } from '../components/ui/Button'

export default function NotFoundPage() {
  return (
    <div className="min-h-screen">
      <Header />
      <main className="mx-auto flex max-w-3xl flex-col items-center px-4 py-24 text-center">
        <span className="text-sm font-medium text-muted-foreground">404</span>
        <h1 className="mt-2 text-3xl font-bold text-foreground">Sayfa bulunamadı</h1>
        <p className="mt-3 max-w-sm text-muted-foreground">
          Aradığın sayfa taşınmış veya hiç var olmamış olabilir.
        </p>
        <Link to="/" className="mt-8">
          <Button>Ana sayfaya dön</Button>
        </Link>
      </main>
    </div>
  )
}
