import { Header } from '../components/Header'
import { AdminOverviewSlider } from '../components/ui/AdminOverviewSlider'
import { Banner } from '../components/ui/Banner'
import { Card } from '../components/ui/Card'

const upcomingSections = [
  { title: 'Ürün ve Kategori Yönetimi', description: 'Ürün ve kategori ekleme, düzenleme, silme işlemleri.' },
  { title: 'Sipariş ve Ödeme Genel Bakışı', description: 'Sipariş durumları ve ödeme takibi.' },
]

export default function AdminPage() {
  return (
    <div className="min-h-screen">
      <Banner id="admin-overview" variant="rainbow" message="🎉 Yönetim paneli geliştirme aşamasında" height="2.5rem" />
      <Header />
      <main className="mx-auto max-w-5xl px-4 py-10">
        <h1 className="text-3xl font-bold tracking-tight text-foreground">Yönetim Paneli</h1>
        <p className="mt-2 text-muted-foreground">Mağazanı buradan yönet.</p>

        <AdminOverviewSlider />

        <div className="mt-8 grid gap-4 sm:grid-cols-2">
          {upcomingSections.map((section) => (
            <Card key={section.title} className="p-6">
              <div className="flex items-center justify-between">
                <h2 className="text-lg font-semibold text-foreground">{section.title}</h2>
                <span className="rounded-full bg-secondary px-3 py-1 text-xs font-medium text-secondary-foreground">Yakında</span>
              </div>
              <p className="mt-2 text-sm text-muted-foreground">{section.description}</p>
            </Card>
          ))}
        </div>
      </main>
    </div>
  )
}
