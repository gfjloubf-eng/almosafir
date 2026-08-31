# خط التدقيق المرجعي للنظام — مشروع المسافر AlMosafer
## Global Platform Transformation Discovery & Baseline Document

---

## 📌 1. حالة النظام قبل التحول (System Status Baseline)
- **الحل البرمجي (Solution):** `AlMosafer.slnx` يضم 5 مشاريع مرتبة وفق معمارية Clean Architecture:
  - `AlMosafer.Domain` (الكيانات الأساسية والقواعد الجوهرية)
  - `AlMosafer.Application` (حالات الاستخدام والواجهات البرمجية والـ DTOs)
  - `AlMosafer.Infrastructure` (الوصول بقواعد البيانات وتشفير كلمات المرور والخدمات)
  - `AlMosafer.Web` (طبقة العرض والتحكم عبر ASP.NET Core MVC و Razor Views)
  - `AlMosafer.Tests` (حزمة الاختبارات البرمجية الآلية)
- **بيئة العمل والتشغيل:**
  - **الإطار البرمجي:** C# 13 / .NET 10 Target Framework `net10.0`.
  - **خادم قواعد البيانات:** MySQL / MariaDB via XAMPP على المنفذ المحلي `3306`.
  - **قاعدة البيانات الحقيقية:** `mosafir_db`.
  - **رابط التطبيق المحلي:** `http://localhost:5163`.
- **نتائج الفحص والتحقق الأولية:**
  - البناء البرمجي: **0 Error(s), 0 Warning(s)**.
  - الاختبارات الآلية: **59 Passed, 0 Failed, 0 Skipped**.

---

## 🗄️ 2. جدول جرد الكيانات وقواعد البيانات (Entities & Database Schema Baseline)
1. `User`: جدول حسابات المسافرين والسائقين ومدرك الأدوار والمعلومات التلامسية.
2. `Trip`: جدول الرحلات المسجلة بين المدن وسعة المقاعد والسعر لكل مقعد.
3. `Booking`: جدول الحجوزات، المقاعد المحجوزة، وتاريخ وحالة الحجز.
4. `Payment`: جدول المعاملات المالية والمدفوعات الداخلية المسجلة لحساب الحجز.
5. `Rating`: جدول التقييمات والمراجعات المربوطة بالرحلات والحجوزات الفعلية.
6. `Notification`: جدول الإشعارات والتنبيهات الموجهة للمستخدمين.
7. `Conversation`: جدول جلسات المحادثة الثنائية بين المسافر والسائق.
8. `Message`: جدول الرسائل النصية داخل كل محادثة.

---

## 🛡️ 3. جرد آليات الأمان والصلاحيات (Security Controls Baseline)
- **تشفير كلمات المرور:** **PBKDF2 مع SHA-256** والملح العشوائي (**Salt**) لمنع هجمات المعاجم و Rainbow Tables.
- **حراس الصلاحيات (Role Authorization):**
  - `Admin`: `[Authorize(Roles = "Admin")]`.
  - `Driver`: `[Authorize(Roles = "Driver,Admin")]`.
  - `Traveler`: `[Authorize(Roles = "Traveler,Admin")]`.
- **حماية ثغرات IDOR:** التحقق من ملكية الحجز والمدفوعات والمحادثات على مستوى الخادم باستخدام `ClaimsPrincipal` `NameIdentifier`.
- **حماية CSRF & XSS:** `[ValidateAntiForgeryToken]` والترميز التلقائي التام في Razor Views.

---

## 👥 4. معلومات الدعم الفني المعتمدة (Support Team Baseline)
- **مسؤول الدعم:** **عمار عادل المصوعي**
- **رقم الهاتف المباشر:** **712275038** (`tel:712275038`)
- **رابط الواتساب المباشر:** `https://wa.me/967712275038`
