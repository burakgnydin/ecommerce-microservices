# E-Commerce Microservices — Proje Kuralları

## Genel Mimari
- Mikroservis mimarisi: api-gateway, auth-service, product-service, order-service, payment-service, notification-service
- Her servisin kendi PostgreSQL veritabanı var, servisler birbirinin veritabanına doğrudan erişmez
- Clean Architecture: Api → Application → Domain (bağımsız) ← Infrastructure
- İlk sürümde servisler arası iletişim REST; RabbitMQ/Kafka/Redis/Kubernetes şimdilik eklenmiyor

## Git Kuralları
- Tüm branch isimleri İngilizce: feature/*, bugfix/* formatında
- Commit mesajları İngilizce ve açıklayıcı (neyin değil, neden değiştiğini de yansıtsın)
- Commit mesajlarında veya kodda AI/Claude'a atıf yapılmaz
- Her subtask kendi branch'inde geliştirilir, PR ile main'e döner

## Tasarım Prensibi
- Pattern/soyutlama, gerçek bir ihtiyaç doğmadan eklenmez (YAGNI) - örn. tek implementasyon varken Factory kurulmaz
- Yeni bir pattern/kütüphane eklenmeden önce gerekçesi açıkça belirtilir

## Güvenlik Taban Çizgisi
- appsettings.json'a asla secret/connection string yazılmaz (user-secrets veya .env kullanılır)
- CORS her zaman bilinen origin'lere kilitli, AllowAnyOrigin kullanılmaz
- Girdi doğrulama sunucu tarafında da yapılır (FluentValidation)

## Proje Yönetimi
- İş takibi Jira'da (KAN projesi, Epic: KAN-20)
- Her yeni servis için önce mimari öneri hazırlanır, onay sonrası geliştirmeye geçilir
