# المخطط المعماري النصي النهائي وتفاصيل الطبقات — AlMosafer
## Master Final Architecture Diagram & Layer Guide

---

## 📌 1. المخطط المعماري النصي (Text Architecture Diagram)

```text
                               ┌─────────────────────────┐
                               │  المستخدم / متصفح الويب │
                               └────────────┬────────────┘
                                            │ (HTTP / HTTPS)
                                            ▼
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│ [ AlMosafer.Web Layer ]                                                                 │
│ - Controllers: Account, Home, Trips, Bookings, Traveler, Driver, Admin                  │
│ - Views: Razor Views (RTL Bootstrap 5, Tajawal Font, Chart.js)                          │
│ - Middleware: RateLimiting (StrictLimiter), HealthChecks (/health), ForwardedHeaders     │
└───────────────────────────────────────────┬─────────────────────────────────────────────┘
                                            │ (Calls Application Services & DTOs)
                                            ▼
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│ [ AlMosafer.Application Layer ]                                                         │
│ - Interfaces: IAccountService, ITripService, IBookingService, IAdminService, etc.       │
│ - Common Models: OperationResult<T>                                                     │
│ - DTOs & ViewModels                                                                     │
└───────────────────────────────────────────┬─────────────────────────────────────────────┘
                                            │ (References Domain Entities)
                                            ▼
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│ [ AlMosafer.Domain Layer ]                                                              │
│ - Entities: User, Trip, Booking, Payment, Rating, Notification, Conversation, Message   │
│ - Enums: UserRole, TripStatus, BookingStatus, PaymentStatus                             │
│ - Pure Domain Rules & Contracts                                                         │
└─────────────────────────────────────────────────────────────────────────────────────────┘
                                            ▲
                                            │ (Implements Interfaces & DB Operations)
┌───────────────────────────────────────────┴─────────────────────────────────────────────┐
│ [ AlMosafer.Infrastructure Layer ]                                                      │
│ - DbContext: AlMosaferDbContext (EF Core 9.0)                                           │
│ - Services Implementations: AccountService, TripService, BookingService, AdminService   │
│ - Security: PasswordHasherService (PBKDF2 / SHA-256 + Salt)                             │
└───────────────────────────────────────────┬─────────────────────────────────────────────┘
                                            │ (Pomelo MySQL Provider)
                                            ▼
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│ [ MariaDB / MySQL Database ] (mosafir_db on Port 3306)                                  │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 🏛️ 2. اتجاه اعتمادات المشاريع (Dependency Directions)
- `AlMosafer.Web` ──► `AlMosafer.Application` ──► `AlMosafer.Domain`
- `AlMosafer.Infrastructure` ──► `AlMosafer.Application` ──► `AlMosafer.Domain`
- لا توجد أي اعتمادات دائرية (No Circular Dependencies) إطلاقاً.
