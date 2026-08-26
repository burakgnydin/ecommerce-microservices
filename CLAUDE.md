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

## Şablon Takibi Kuralı
Yeni bir servis, önceki bir servisin mimari şablonunu (Clean Architecture katmanları, secrets yönetimi, hata yönetimi, CI, Docker Compose vb.) temel alırken: mimari öneri/prompt'ta "tekrar anlatmana gerek yok, sadece farkları belirt" denmesi, sadece DOKÜMANTASYONA (açıklamaya) yöneliktir - UYGULAMAYA değil. Şablonun TÜM adımları (Docker Compose dahil) yine de fiilen uygulanmalı, sadece metinde tekrar anlatılmasına gerek yok. Bir adım "zaten bilinen bir şablon parçası" diye atlanmamalı.
