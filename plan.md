# MediatR E-Ticaret API Projesi Plan

## Proje Hedefi
MediatR kullanımını öğrenmek için DDD, CQRS Pattern ve EAV (Entity-Attribute-Value) Pattern'larını uygulayan bir e-ticaret API'si geliştirmek.

## Teknolojiler
- **.NET Aspire** (AppHost mevcut)
- **MongoDB** (Primary database - NoSQL)
- **SQL Server/PostgreSQL** (Reporting ve analytics için)
- **Docker** (Veritabanı container'ları)
- **Redis** (Cache)
- **MediatR** (CQRS implementation)

## 1. Proje Yapısı ve .NET Aspire Setup

### AppHost Konfigürasyonu
- MongoDB container ekleme
- SQL database container ekleme (reporting için)
- Redis container ekleme
- ServiceDefaults yapılandırması

### Solution Structure
```
MeadiaTR/
├── src/
│   ├── MeadiaTR.AppHost/           (Mevcut)
│   ├── MeadiaTR.Domain/            (Domain entities ve business logic)
│   ├── MeadiaTR.Application/       (CQRS handlers, MediatR)
│   ├── MeadiaTR.Infrastructure/    (MongoDB, SQL, Redis implementations)
│   ├── MeadiaTR.API/              (Web API controllers)
│   └── MeadiaTR.ServiceDefaults/   (Shared configurations)
├── tests/
│   ├── MeadiaTR.Domain.Tests/
│   ├── MeadiaTR.Application.Tests/
│   └── MeadiaTR.API.Tests/
└── docs/
```

## 2. Domain Layer (DDD)

### Entities
- **User**: Kullanıcı bilgileri
- **Product**: Ürün temel bilgileri
- **Category**: Kategori hiyerarşisi
- **Advertisement**: İlan bilgileri
- **Order**: Sipariş işlemleri
- **Cart**: Alışveriş sepeti

### Value Objects
- **Email**: Email validation
- **Money**: Para birimi ve miktar
- **Address**: Adres bilgileri
- **PhoneNumber**: Telefon numarası

### Domain Events
- **UserRegistered**: Kullanıcı kayıt olduğunda
- **ProductCreated**: Ürün oluşturulduğunda
- **AdvertisementPublished**: İlan yayınlandığında
- **OrderPlaced**: Sipariş verildiğunda

### Domain Services
- **PricingService**: Fiyatlama logic'i
- **CategoryService**: Kategori işlemleri
- **UserService**: Kullanıcı business rules

## 3. Application Layer (CQRS + MediatR)

### Commands (Write Operations)
- **RegisterUserCommand**: Kullanıcı kaydı
- **LoginUserCommand**: Kullanıcı girişi
- **CreateProductCommand**: Ürün oluşturma
- **CreateAdvertisementCommand**: İlan oluşturma
- **PlaceOrderCommand**: Sipariş verme
- **AddToCartCommand**: Sepete ekleme

### Queries (Read Operations)
- **GetUserQuery**: Kullanıcı bilgisi getirme
- **GetProductsQuery**: Ürün listesi
- **GetCategoriesQuery**: Kategori listesi
- **GetAdvertisementsQuery**: İlan listesi
- **GetOrdersQuery**: Sipariş geçmişi
- **SearchProductsQuery**: Ürün arama

### MediatR Behaviors
- **ValidationBehavior**: FluentValidation
- **LoggingBehavior**: Request/Response logging
- **PerformanceBehavior**: Performance monitoring
- **CachingBehavior**: Redis cache

### DTOs
- Request/Response modelleri
- Mapping profiles (AutoMapper)

## 4. Infrastructure Layer

### MongoDB Implementation
- **Primary Database**: Ürünler, kullanıcılar, ilanlar
- **Collections**: Users, Products, Advertisements, Orders, Categories
- **Repository Pattern**: Generic ve specific repositories
- **EAV Pattern**: Product attributes için flexible schema

### SQL Database (Reporting)
- **Analytics Database**: Raporlama ve analiz
- **Views**: Satış raporları, kullanıcı istatistikleri
- **Stored Procedures**: Complex reporting queries

### Cache Layer (Redis)
- **Product Cache**: Sık erişilen ürünler
- **Category Cache**: Kategori hiyerarşisi
- **User Sessions**: JWT token cache

### Authentication & Authorization
- **JWT Implementation**: Token-based auth
- **Role-based Authorization**: Admin, User, Seller
- **Permission System**: Fine-grained permissions

## 5. API Layer

### Controllers
- **UsersController**: Kullanıcı işlemleri
- **ProductsController**: Ürün yönetimi
- **AdvertisementsController**: İlan işlemleri
- **OrdersController**: Sipariş yönetimi
- **CategoriesController**: Kategori işlemleri
- **CartController**: Sepet işlemleri

### Middleware
- **ExceptionHandlingMiddleware**: Global exception handling
- **AuthenticationMiddleware**: JWT validation
- **LoggingMiddleware**: Request logging
- **RateLimitingMiddleware**: API rate limiting

### API Features
- **Swagger Documentation**: API dokümantasyonu
- **Health Checks**: Database, Redis, external services
- **Versioning**: API versioning strategy
- **CORS**: Cross-origin resource sharing

## 6. Key Features

### Kullanıcı Yönetimi
- Kullanıcı kaydı ve doğrulama
- JWT token-based authentication
- Profil yönetimi
- Rol ve yetki sistemi

### Ürün ve İlan Yönetimi
- Ürün CRUD işlemleri
- EAV pattern ile flexible attributes
- Kategori hiyerarşisi
- İlan yayınlama ve onay süreci
- Resim upload ve management

### E-ticaret İşlemleri
- Alışveriş sepeti
- Sipariş işlemleri
- Ödeme entegrasyonu (mock)
- Kargo takibi

### Arama ve Filtreleme
- ElasticSearch entegrasyonu (opsiyonel)
- MongoDB text search
- Category-based filtering
- Price range filtering

## 7. Testing Strategy

### Unit Tests
- Domain entities ve value objects
- Application handlers
- Repository implementations

### Integration Tests
- API endpoint tests
- Database integration tests
- Cache integration tests

### Architecture Tests
- Dependency rules
- Layer separation
- Naming conventions

## 8. Development Phases

### Phase 1: Foundation
- .NET Aspire setup tamamlama
- Domain model oluşturma
- MongoDB ve Redis setup

### Phase 2: Core Features
- User registration/login
- Product management
- Basic CRUD operations

### Phase 3: E-commerce Features
- Shopping cart
- Order management
- Advertisement system

### Phase 4: Advanced Features
- Search functionality
- Reporting (SQL database)
- Performance optimization

## 9. EAV Pattern Implementation

### MongoDB'de EAV
```json
{
  "_id": "product_id",
  "name": "iPhone 15",
  "categoryId": "smartphones",
  "attributes": [
    {"key": "color", "value": "Blue", "type": "string"},
    {"key": "storage", "value": "128", "type": "number", "unit": "GB"},
    {"key": "price", "value": "999.99", "type": "currency", "currency": "USD"}
  ]
}
```

### Attribute Schema Validation
- Category-based attribute definitions
- Type validation (string, number, boolean, date)
- Required vs optional attributes

## 10. Deployment ve DevOps

### Docker Containers
- MongoDB cluster
- Redis cluster
- SQL Server/PostgreSQL
- Application containers

### Monitoring
- Health checks
- Performance metrics
- Error tracking
- Log aggregation

Bu plan MediatR'ın CQRS pattern ile birlikte nasıl kullanıldığını göstermek ve modern .NET teknolojilerini öğrenmek için kapsamlı bir yaklaşım sunmaktadır.

---

## 📍 PROJE DURUMU (Son Güncelleme: 7 Eylül 2024)

### ✅ Tamamlanan İşler:
1. **Solution Yapısı** - OptimatePlatform yapısına uygun klasör organizasyonu oluşturuldu
2. **AppHost Konfigürasyonu** - MongoDB, Redis ve SQL Server containers eklendi
3. **SharedKernel** - BaseEntity, IDomainEvent, BaseValueObject oluşturuldu
4. **Domain Layer** - Enums (UserRole, OrderStatus, AdvertisementStatus) ve Value Objects (Email, Money) oluşturuldu
5. **Anemic Domain Model** - Entity'ler sadece data holder olacak şekilde refactor edildi (OptimatePlatform tarzı)
6. **BusinessLogic Classes** - Application layer'da ProductBusinessLogic, CategoryBusinessLogic, AdvertisementBusinessLogic oluşturuldu
7. **Domain Events** - UserRegistered, ProductCreated, AdvertisementPublished events oluşturuldu
8. **MediatR Commands/Queries** - CreateProductCommand, CreateCategoryCommand, CreateAdvertisementCommand, ApproveAdvertisementCommand ve handler'ları oluşturuldu
9. **Repository Interface Base** - IRepository<T> generic interface tanımlandı

### ✅ Tamamlanan Sonraki Adımlar:
1. **Repository Interfaces** - ✅ IProductRepository, ICategoryRepository, IAdvertisementRepository, IUserRepository, IOrderRepository tanımlandı
2. **Infrastructure Layer** - ✅ MongoDB repository implementasyonları oluşturuldu (ProductRepository, CategoryRepository, AdvertisementRepository, UserRepository, OrderRepository)
3. **Domain Event Handlers** - ✅ MediatR event handler'ları implement edildi (UserRegisteredEventHandler, ProductCreatedEventHandler, AdvertisementPublishedEventHandler)
4. **Application Layer DI** - ✅ MediatR package'ı eklendi ve DI konfigürasyonu tamamlandı
5. **Build Errors** - ✅ ProductAttribute, Money.FromDecimal/Zero metodları, BaseEntity.Raise metodu eklendi
6. **Aspire DI Integration** - ✅ ApiService'e Application ve Infrastructure referansları eklendi, Program.cs'de DI konfigürasyonu yapıldı, AppHost.cs düzenlendi

### ✅ Tamamlanan API Layer:
1. **API Controllers** - ✅ ProductsController, CategoriesController, AdvertisementsController, UsersController, OrdersController oluşturuldu ve MediatR entegrasyonu yapıldı

### ✅ Son Tamamlanan:
1. **DTO Organization** - ✅ Tüm Features altında DTOs klasörleri oluşturuldu ve mevcut DTO'lar organize edildi
2. **Order Domain Events** - ✅ OrderPlacedEvent oluşturuldu
3. **Order BusinessLogic** - ✅ OrderBusinessLogic oluşturuldu
4. **Order Commands** - ✅ PlaceOrderCommand oluşturuldu

### 🔄 Devam Eden İşler:
1. **PlaceOrderCommandHandler** - PlaceOrderCommand'ın handler'ı oluşturulacak
2. **Order Commands** - UpdateOrderStatusCommand oluşturulacak
3. **Order Queries** - GetOrderQuery, GetUserOrdersQuery, GetPendingOrdersQuery oluşturulacak
4. **Order EventHandlers** - OrderPlacedEventHandler oluşturulacak

### 🔄 Sonraki Adımlar:
1. **Query implementasyonları** - Eksik Query handler'larını oluştur
2. **Authentication & Authorization** - JWT implementation
3. **API Testing** - Postman collection veya integration tests

### 📂 Mevcut Proje Yapısı:
```
MeadiaTR/
├── MediaTR.AppHost/          ✅ (MongoDB, Redis, SQL containers)
├── MediaTR.SharedKernel/     ✅ (Base classes)
├── MediaTR.Domain/           ✅ (Entities: User, Product, Category, Advertisement, Order, OrderItem | Enums, Value Objects tamamlandı)
├── MediaTR.Application/      ✅ (MediatR Commands/Queries, Event Handlers, Business Logic, DI Configuration)
├── MediaTR.Infrastructure/   ✅ (MongoDB Repositories, DI Configuration)
├── MediaTR.ApiService/       ✅ (Controllers: Products, Categories, Advertisements, Users, Orders)
└── MediaTR.ServiceDefaults/  ✅ (Aspire Extensions, OpenTelemetry, Health Checks)
```

### 🎯 Bir Sonraki Session'da Odaklanılacak Alan:
- **Anemic Domain Model** yaklaşımı tamamlandı (OptimatePlatform tarzında)
- MediatR Commands/Queries ile BusinessLogic integration
- Repository pattern implementation
- Infrastructure layer (MongoDB, Redis) implementasyonu
- Domain event handler'ları ile business logic orchestration

- Not: Orders'ın Commands , EventHandlers ve Queries'i eksik

### 🔄 Son Güncelleme (24 Eylül 2024):
1. **PlaceOrderCommandHandler** - ✅ Tamamlandı
   - OrderItemBusinessLogic entegrasyonu yapıldı
   - Money.Create() metoduyla düzeltildi
   - Order.AddOrderItem() metodu eklendi
   - Reflection yerine basit çözüm kullanıldı

2. **Devam Eden:**
   - UpdateOrderStatusCommand oluşturulacak
   - UpdateOrderStatusCommandHandler oluşturulacak
   - Order Queries (GetOrderQuery, GetUserOrdersQuery, GetPendingOrdersQuery) oluşturulacak
   - OrderPlacedEventHandler oluşturulacak

3. **Teknik Notlar:**
   - ✅ Tüm Entity ID'ler string'den Guid'e değiştirildi (Entity'ler, Repository'ler, Command/Query'ler, DTO'lar, API Controller'lar)
   - ✅ Order entity'sine AddOrderItem() ve ClearOrderItems() metodları eklendi
   - ✅ OrderItemBusinessLogic ile OrderBusinessLogic ayrımı netleştirildi
   - ✅ MongoRepository ve tüm implementasyonlar Guid kullanacak şekilde güncellendi
   
   User entity için UserBusinessLogic yazmaya gerek yok. Şu nedenlerle:
   1. Mevcut BusinessLogic sınıfları domain-specific business rules içeriyor:
    - ProductBusinessLogic: Ürün fiyatlama, stok yönetimi, slug oluşturma
    - CategoryBusinessLogic: Kategori hiyerarşisi, slug oluşturma
    - AdvertisementBusinessLogic: İlan onay süreci, durum değişiklikleri
    - OrderBusinessLogic: Sipariş yaşam döngüsü, iş kuralları
   2. User için tipik business logic operations:
    - User registration → Bu zaten Command/Handler'da yapılır
    - Authentication → Ayrı bir servis (Identity/Auth service)
    - Password reset → Ayrı bir servis
    - Profile updates → Basit CRUD operations
   3. YAGNI (You Aren't Gonna Need It) prensibi:
    - Şu anda User için karmaşık business logic yok
    - Gerektiğinde ekleyebiliriz
   4. User işlemleri genellikle şu şekilde yapılır:
    - Registration: CreateUserCommand + CreateUserCommandHandler
    - Login: Authentication service
    - Profile update: UpdateUserCommand + UpdateUserCommandHandler

### 🔄 Son Güncelleme (26 Eylül 2024):
1. **Order Module Tamamlandı** - ✅
   - PlaceOrderCommand + PlaceOrderCommandHandler
   - UpdateOrderStatusCommand + UpdateOrderStatusCommandHandler
   - GetOrderQuery, GetUserOrdersQuery, GetPendingOrdersQuery + Handler'ları
   - OrderPlacedEventHandler
   - OrderItemDto, GetOrderResult DTO'ları
   - OrderBusinessLogic ve OrderItemBusinessLogic

2. **Result Pattern Implementation Tamamlandı** - ✅
   - ✅ SharedKernel/ResultAndError/Error.cs oluşturuldu
   - ✅ SharedKernel/ResultAndError/Result.cs oluşturuldu
   - ✅ Application/Abstractions/Messaging/ interface'leri oluşturuldu:
     * IQuery<TResponse>
     * ICommand, ICommand<TResponse>
     * IQueryHandler<TQuery, TResponse>
     * ICommandHandler<TCommand>, ICommandHandler<TCommand, TResponse>
   - ✅ Application/Orders/Errors/OrderErrors.cs oluşturuldu
   - ✅ Order Commands/Queries Result pattern'e çevrildi:
     * PlaceOrderCommand → ICommand<Guid>
     * UpdateOrderStatusCommand → ICommand
     * GetOrderQuery → IQuery<GetOrderResult>
   - ✅ Handler'lar Result<T> döndürecek şekilde güncellendi
   - ✅ Exception handling yerine Result pattern kullanılıyor

3. **Sonraki Adımlar:**
   - Diğer feature'lardaki Commands/Queries'leri Result pattern'e çevir (Product, Category, Advertisement, User)
   - Query Handler'larını da Result pattern'e çevir
   - API Controller'ları Result pattern'i handle edecek şekilde güncelle
   - Global exception handling middleware ekle

### 🔄 Son Güncelleme (27 Eylül 2024):
1. **Result Pattern Implicit Operator Tamamlandı** - ✅
   - SharedKernel/ResultAndError/Result.cs'e Error → Result<TValue> implicit operator eklendi:
     ```csharp
     public static implicit operator Result<TValue>(TValue? value) => Create(value);
     public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
     ```
   - Hem TValue → Result<TValue> hem de Error → Result<TValue> implicit operator çalışıyor
   - GetOrderQueryHandler'da implicit operator kullanımı örneklendi

2. **Features Klasörü Result Pattern Dönüşümü** - 🔄 DEVAM EDİYOR
   - ✅ **Orders**: Tüm Commands/Queries Result pattern'e çevrildi
     * GetOrderQueryHandler'da error handling ve implicit operator kullanımı
     * GetUserOrdersQueryHandler'da user validation ve error handling eklendi
   - ✅ **Categories**: CreateCategoryCommand/Handler Result pattern'e çevrildi
   - ✅ **Products**: CreateProductCommand/Handler ve GetProductQuery/Handler Result pattern'e çevrildi
   - ✅ **Advertisements**: Zaten Result pattern'e çevrilmişti
   - 🔄 **KALAN**: GetPendingOrdersQuery/Handler, diğer eksik Query'ler

3. **Şu An Durduğumuz Yer:**
   - D:\GitHub\PROJECTS\MeadiaTR\MediaTR.Application\Features\ klasöründe Result pattern implementasyonu
   - Tüm Command/Query/Handler dosyalarını yeni ICommand, IQuery, ICommandHandler, IQueryHandler interface'lerini kullanacak şekilde çeviriyoruz
   - Error handling ve implicit operator kullanımını örnekliyoruz

4. **Sonraki Adımlar:**
   - Kalan Features klasöründeki dosyaları Result pattern'e çevir
   - API Controller'ları Result pattern'i handle edecek şekilde güncelle
   - Domain-specific error sınıfları oluştur (ProductErrors, CategoryErrors, AdvertisementErrors)
   - Global exception handling middleware ekle
