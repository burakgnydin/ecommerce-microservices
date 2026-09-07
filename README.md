# E-Ticaret Mikroservis Uygulaması

.NET tabanlı, altı bağımsız mikroservisten oluşan uçtan uca bir e-ticaret sistemi. Ürün yönetimi, kimlik doğrulama, sepet/sipariş yönetimi, ödeme işleme, bildirim ve tüm sistemi tek bir giriş noktasında birleştiren bir API Gateway içerir. React tabanlı bir storefront ile birlikte gelir.

## İçindekiler

- [Mimari](#mimari)
- [Servisler](#servisler)
- [Teknoloji Yığını](#teknoloji-yığını)
- [Güvenlik Modeli](#güvenlik-modeli)
- [Kurulum ve Çalıştırma](#kurulum-ve-çalıştırma)
- [Test](#test)
- [CI/CD](#cicd)
- [Deployment](#deployment)
- [Proje Yapısı](#proje-yapısı)

## Mimari

Sistem, her biri kendi veritabanına sahip, birbirinden bağımsız olarak geliştirilip test edilebilen mikroservislerden oluşur. Her servis **Clean Architecture** prensiplerine göre katmanlandırılmıştır:

```
Api → Application → Domain ← Infrastructure
```

- **Domain** katmanı hiçbir dış bağımlılığa sahip değildir; entity'ler kendi kendini valide eder (private setter + iş kuralları).
- **Application** katmanı iş mantığını, DTO'ları ve servis arayüzlerini barındırır.
- **Infrastructure** katmanı veritabanı erişimi, dış servis istemcileri ve teknik detayları içerir.
- **Api** katmanı yalnızca HTTP giriş noktalarını (controller/endpoint) barındırır.

Servisler arası iletişim, kullanıcının kendi JWT'sini ileri ileten (**bearer-forward**) ya da servise özel bir JWT ile kimliklenen (**service-to-service auth**) REST çağrıları üzerinden gerçekleşir.

## Servisler

| Servis | Sorumluluk |
|---|---|
| **auth-service** | Kullanıcı kaydı, giriş, JWT üretimi (RS256), refresh token rotasyonu, rol yönetimi |
| **product-service** | Ürün ve kategori yönetimi, arama (PostgreSQL `pg_trgm`) |
| **order-service** | Sepet yönetimi ve sipariş yaşam döngüsü (Pending → Paid/Cancelled) |
| **payment-service** | Ödeme simülasyonu, sipariş durumu güncelleme, bildirim tetikleme |
| **notification-service** | Bildirim kaydı ve loglama |
| **api-gateway** | YARP tabanlı, tüm servislere tek giriş noktası (routing) |

Frontend (`frontend/`), yukarıdaki servislerle API Gateway üzerinden canlı olarak entegre çalışan bir React storefront'udur: ürün listeleme, sepet, ödeme akışı, kullanıcı hesap yönetimi ve bir admin paneli içerir.

## Teknoloji Yığını

**Backend**
- .NET 10 Web API
- PostgreSQL + Entity Framework Core (Npgsql)
- FluentValidation (SharpGrip)
- Scalar (API dokümantasyonu)
- xUnit + Testcontainers (unit + entegrasyon testleri)
- YARP (API Gateway routing)
- RS256 JWT (asimetrik kimlik doğrulama, Zero Trust — her servis token'ı kendi public key'iyle bağımsız doğrular)

**Frontend**
- React + Vite + TypeScript
- Tailwind CSS
- Framer Motion

**Altyapı**
- Docker Compose (yerel geliştirme ve tek komutla tam stack orkestrasyonu)
- GitHub Actions (CI — her serviste build + test)
- Render (deployment)

## Güvenlik Modeli

- **Zero Trust JWT doğrulama:** Her servis, gelen token'ı kendi elindeki RS256 public key'iyle bağımsız olarak doğrular; merkezi bir yetkilendirme sunucusuna her istekte başvurmaz.
- **Bearer-forward pattern:** Bir servis, kullanıcı adına başka bir servisi çağırırken kullanıcının kendi token'ını ileri iletir (örn. sipariş oluştururken product-service'e ürün doğrulatma).
- **Service-to-service auth:** Hassas durum geçişleri (örn. bir siparişin "Paid" olarak işaretlenmesi) yalnızca servise özel, ayrı bir role sahip JWT ile yapılabilir; kullanıcı token'ı bu işlemler için yetkili değildir.
- **IDOR koruması:** Kullanıcılar yalnızca kendi kaynaklarına erişebilir; sahip olunmayan kaynak istekleri "not found" ile aynı yanıtı döner (kaynağın var olup olmadığı sızdırılmaz).
- Şifreler BCrypt ile hash'lenir, refresh token'lar rotasyona tabidir ve hash'lenmiş olarak saklanır, login endpoint'i rate limiting ile korunur.

## Kurulum ve Çalıştırma

### Gereksinimler
- Docker & Docker Compose
- Node.js 20+ (frontend için)

### Adımlar

1. Depoyu klonlayın:
   ```bash
   git clone https://github.com/burakgnydin/ecommerce-microservices.git
   cd ecommerce-microservices
   ```

2. `.env.example` dosyasını `.env` olarak kopyalayıp gerekli değerleri doldurun (JWT anahtar çifti, veritabanı kimlik bilgileri, servis token'ları).

3. Tüm backend servislerini ve veritabanlarını tek komutla ayağa kaldırın:
   ```bash
   docker compose up --build
   ```
   Servisler hazır olduğunda API Gateway `http://localhost:5100` üzerinden erişilebilir olur.

4. Frontend'i başlatın:
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
   Uygulama varsayılan olarak `http://localhost:5173` adresinde çalışır.

### Demo Akışı

Kayıt ol → giriş yap → ürünleri listele → sepete ürün ekle → sipariş oluştur → ödeme yap → bildirim/sipariş durumunu görüntüle. Admin hesabıyla giriş yapıldığında `/admin` altında tüm sipariş ve ödemeleri gösteren bir yönetim paneli mevcuttur.

## Test

Her serviste hem birim hem entegrasyon testleri bulunur; entegrasyon testleri **Testcontainers** ile gerçek bir PostgreSQL container'ı üzerinde çalışır.

```bash
dotnet test
```

Frontend için:
```bash
npm run build   # TypeScript tip kontrolü dahil
npm run lint
```

## CI/CD

Her serviste, her push'ta otomatik derleme ve test çalıştıran bir GitHub Actions iş akışı bulunur.


## Proje Yapısı

```
ecommerce-microservices/
├── auth-service/
├── product-service/
├── order-service/
├── payment-service/
├── notification-service/
├── api-gateway/
├── frontend/
├── docker-compose.yml
└── CLAUDE.md
```

Her servis kendi `Api/`, `Application/`, `Domain/`, `Infrastructure/` klasörlerine ve kendi `postman/` koleksiyonuna sahiptir.

---
