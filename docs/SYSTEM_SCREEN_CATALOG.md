# كتالوج الشاشات والواجهات الفعلية للنظام — AlMosafer نظام المسافر
## Master System Screen Catalog & Controller Inventory

---

## 📌 1. كتالوج الشاشات الفعلي (Screen Inventory)

| اسم الشاشة | المسار (URL) | Controller / Action | الدور المسموح (Role) | الغرض الهندسي الوظيفي |
|---|---|---|---|---|
| **الرئيسية** | `/` | `HomeController.Index` | الكل (Visitor, Traveler, Driver, Admin) | واجهة ترحيبية وإبراز مميزات النظام ومربعات البحث |
| **تسجيل مسافر** | `/Account/Register` | `AccountController.Register` | الزائر (Guest) | إنشاء حساب مسافر جديد في قاعدة البيانات |
| **تسجيل سائق** | `/Account/RegisterDriver` | `AccountController.RegisterDriver` | الزائر (Guest) | إنشاء حساب سائق وتوثيق بيانات المركبة |
| **تسجيل الدخول** | `/Account/Login` | `AccountController.Login` | الزائر (Guest) | التوثيق وإنشاء جلسة كوكيز آمنة |
| **البحث عن رحلات** | `/Trips/Search` | `TripsController.Search` | الكل | استعلام مرن عن الرحلات حسب المدن والتواريخ |
| **تفاصيل الرحلة** | `/Trips/Details/{id}` | `TripsController.Details` | الكل | عرض تفاصيل الرحلة والمقاعد المتاحة وسعر المقعد |
| **تأكيد الحجز** | `/Bookings/Create` | `BookingsController.Create` | Traveler, Admin | اختيار عدد المقاعد وتأكيد الحجز الفوري |
| **إيصال الحجز** | `/Bookings/Receipt/{id}` | `BookingsController.Receipt` | Traveler, Driver, Admin | طباعة وعرض الإيصال الرقمي للحجز المحجوز |
| **لوحة المسافر** | `/Traveler/Dashboard` | `TravelerController.Dashboard` | Traveler, Admin | متابعة حجوزات المسافر الحالية والسابقة |
| **لوحة السائق** | `/Driver/Dashboard` | `DriverController.Dashboard` | Driver, Admin | متابعة رحلات السائق والركاب ورصيد التقييم |
| **إنشاء رحلة** | `/Driver/CreateTrip` | `DriverController.CreateTrip` | Driver, Admin | نشر رحلة سفر جديدة بين مدينتين |
| **لوحة الإدارة** | `/Admin/Dashboard` | `AdminController.Dashboard` | Admin حصرياً | المراقبة المركزية والإحصائيات الحية |
| **إدارة المستخدمين** | `/Admin/Users` | `AdminController.Users` | Admin حصرياً | التحكم بالحسابات والتفعيل والتجميد |
| **إدارة المدفوعات** | `/Admin/Payments` | `AdminController.Payments` | Admin حصرياً | مراجعة السجلات المالية والمطابقات |
| **التقارير التحليلية** | `/Admin/Reports` | `AdminController.Reports` | Admin حصرياً | عرض الرسومات البيانية التفاعلية لـ Chart.js |
| **فحص صحة النظام** | `/health` | Health Check Endpoint | الكل | استجابة فورية بحالة الخادم وقاعدة البيانات |
