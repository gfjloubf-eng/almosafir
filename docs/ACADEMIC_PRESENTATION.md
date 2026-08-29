# مشروع المسافر — AlMosafer System
## التقرير الفني المكتمل والأكاديمي لمناقشة التخرج والعرض الدكتوراه/البكالوريوس

---

## 📌 1. ملخص المشروع (Executive Summary)
منصة **المسافر (AlMosafer)** هي نظام برمجي متكامل لإدارة ورحلات السفر بين المدن اليمنية، ربط السائقين والمسافرين، وحجز المقاعد، وحصر المدفوعات المسجلة، وإتاحة محادثات فورية وإشعارات برمجية وتقييمات تفاعلية، إضافة إلى لوحة تحكم إدارية شاملة ونظام تقارير وتحليلات بيانية ودعم اتخاذ القرار.

تم تطوير النظام وفق أعلى المعايير الهندسية الأكاديمية باستخدام **C# / ASP.NET Core 10** ومعمارية **Clean Architecture** الموزعة على طبقات منفصلة، مع الاعتماد الكامل على خادم قواعد البيانات **MySQL / MariaDB via XAMPP**.

---

## 🏛️ 2. المعمارية البرمجية (System Architecture)

النظام مبني على نمط **Clean Architecture** ذو أربع طبقات رئيسية:

```text
       [ AlMosafer.Web ]  (MVC Controllers, Razor Views, Bootstrap RTL, Chart.js)
              │
              ▼
    [ AlMosafer.Application ]  (Interfaces, DTOs, Business Rules)
              │
              ▼
      [ AlMosafer.Domain ]  (Entities, Enums, Value Objects)
              ▲
              │
  [ AlMosafer.Infrastructure ]  (EF Core DbContext, MySQL Persistence, Hashing, Services)
```

1. **`AlMosafer.Domain`**: يضم الكيانات المركزية (`User`, `Trip`, `Booking`, `Payment`, `Rating`, `Notification`, `Conversation`, `Message`) والحالات والأنواع (`UserRole`, `TripStatus`, `BookingStatus`, `PaymentStatus`).
2. **`AlMosafer.Application`**: يضم الواجهات والعقود البرمجية (`IAuthService`, `ITripService`, `IBookingService`, `IPaymentService`, `IRatingService`, `IAdminService`, `IReportingService`, `INotificationService`, `IConversationService`, `IMessageService`, `IDashboardService`) إضافة لكائنات نقل البيانات (`DTOs`).
3. **`AlMosafer.Infrastructure`**: يحتوي سياق EF Core (`AlMosaferDbContext`) والخدمات التنفيذية وتشفير الحماية كلمة المرور (`PBKDF2 / SHA-256`) والاتصال بقاعدة بيانات MySQL.
4. **`AlMosafer.Web`**: يضم متحكمات MVC والواجهات التفاعلية المصممة بلغة Razor وBootstrap RTL مع مكتبة Mappers التلقائية والحماية من هجمات XSS/CSRF/IDOR.

---

## 🗄️ 3. تصميم قاعدة البيانات (Database Design)
يعمل النظام على خادم MySQL / MariaDB المدار عبر XAMPP على المنفذ `3306` بقاعدة بيانات `mosafir_db`:
- **`users`**: جدول الحسابات والأدوار وتفاصيل المركبات والتقييم.
- **`trips`**: جدول الرحلات وخطوط السير والأسعار والمقاعد المتاحة.
- **`bookings`**: جدول الحجوزات وعدد المقاعد وتاريخ الحجز.
- **`payments`**: جدول العمليات المالية المسجلة وأرقام المعاملات.
- **`ratings`**: جدول التقييمات وآراء المسافرين المربوط بالرحلات السابقة.
- **`notifications`**: سجل التنبيهات البرمجية للمستخدمين.
- **`conversations`**: جدول جلسات المحادثة بين المسافرين والسائقين.
- **`messages`**: جدول الرسائل النصية داخل المحادثات.

---

## 🔐 4. الأمن وحماية المنظومة (Security & Protection)
1. **تشفير كلمات المرور (Password Hashing)**: الاعتماد على خوارزمية **PBKDF2 مع SHA-256** ونمط Salt عشوائي لكل حساب للوقاية من Rainbow Tables.
2. **التحقق من الصلاحيات (Role-Based Authorization)**: حماية مسارات الإدارة عبر `[Authorize(Roles = "Admin")]` ومسارات السائقين `[Authorize(Roles = "Driver")]`.
3. **الوقاية من IDOR (Resource Ownership Guards)**: التحقق من ملكية الحجز/المحادثة/البروفايل على مستوى خادم السيرفر باستخلاص ID المستخدم من `ClaimsPrincipal`.
4. **الوقاية من CSRF**: حماية جميع النماذج باستخدام `[ValidateAntiForgeryToken]` ورموز الحماية التلقائية في Razor.
5. **الوقاية من XSS**: الترميز التلقائي التام في Razor Views دون استخدام `Html.Raw` غير الآمن.

---

## 🎨 5. تصميم التفاعل وواجهة المستخدم (HCI & UX Principles)
1. **الرؤية والشفافية (System Status Visibility)**: استخدام تنبيهات Toast وشارات ملونة تعكس حالة الحجز والرحلة.
2. **منع الأخطاء البشرية (Error Prevention)**: منع التقييم المكرر، منع تقييم السائق لنفسه، منع التقييم قبل مكتمل الرحلة، منع حجز مقاعد تتجاوز السعة المتاحة.
3. **دعم اللغة العربية والاتجاه (Arabic RTL)**: تصميم عصري مريح للعين يدعم الجوالات والتابلت والشاشات الكبيرة.

---

## 🧪 6. التغطية والاختبارات الآلية (Test Suite Baseline)
- **إجمالي الاختبارات الآلية المنفذة:** **52 اختبارًا آليًا ناجحًا (52 Passed, 0 Failed)**.
- **تغطية الاختبارات:** تشمل تسجيل الدخول، إنشاء الحسابات، صلاحيات الأدوار، حظر IDOR، حساب متوسط التقييمات، تجميعات تقارير MySQL، ومعالجة نطاقات التواريخ والبيانات الخالية.
