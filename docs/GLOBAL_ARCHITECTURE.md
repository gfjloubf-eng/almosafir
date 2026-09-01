# المرجع المعماري العام للمنصة — مشروع المسافر AlMosafer
## Global Mobility Platform Architectural Blueprint

---

## 📌 1. المعمارية الهيكلية (Architectural Overview)
يعتمد نظام **المسافر (AlMosafer)** على معمارية الطبقات النظيفة (**Clean Architecture**) المقسمة إلى 4 مشاريع منفصلة المسؤوليات لتأمين عزل المنطق التجاري عن التفاصيل التقنية:

```text
               [ AlMosafer.Web ]  (ASP.NET Core MVC, Razor Views, Bootstrap RTL, Chart.js)
                      │
                      ▼
            [ AlMosafer.Application ]  (Use Cases, Interfaces, DTOs, OperationResult)
                      │
                      ▼
              [ AlMosafer.Domain ]  (Entities, Enums, Business Invariants)
                      ▲
                      │
          [ AlMosafer.Infrastructure ]  (EF Core DbContext, MySQL MariaDB, PBKDF2 Hashing)
```

---

## 🎯 2. فاعلو النظام وحالات الاستخدام الأساسية (System Actors & Core Use-Cases)
1. **المسافر (Traveler):** البحث، تفاصيل الرحلات، الحجز الفوري، إصدار إيصال الحجز الرقمي، الدفع الداخلي، المحادثة، والتقييم.
2. **السائق (Driver):** نشر رحلات جديدة، تحديد المقاعد والسعر، متابعة الحجوزات، والتواصل مع الركاب.
3. **مدير النظام (Admin):** الإشراف الكامل على الحسابات، الرحلات، الحجوزات، المدفوعات، التقييمات، والتقارير التحليلية المباشرة.

---

## 🗄️ 3. محرك قاعدة البيانات والاستعلامات (EF Core & MariaDB Persistence)
- **الخادم:** MariaDB / MySQL via XAMPP على المنفذ `3306`.
- **قاعدة البيانات:** `mosafir_db`.
- **الميزات المنفذة:**
  - القراءة المحسّنة عبر `.AsNoTracking()`.
  - المعاملات المالية الذرية والمحمية ضد الأخطاء.
  - حساب مبالغ الحجز والأسعار حصريًا على مستوى خادم السيرفر بدقة `decimal(18,2)`.
