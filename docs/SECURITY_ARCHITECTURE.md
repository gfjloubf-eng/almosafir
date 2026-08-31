# مرجع الهندسة الأمنية وحماية البيانات — مشروع المسافر AlMosafer
## Enterprise Security Architecture Specification

---

## 📌 1. نموذج الأمان المتكامل (Security Architecture Overview)
تم تصميم نظام المسافر وفق استراتيجية **الدفاع في العمق (Defense in Depth)** لحماية بيانات المستخدمين والمعاملات المالية وحركات الحجز من أي هجمات سيبرانية أو تزوير.

---

## 🛡️ 2. الضوابط الأمنية المنفذة (Implemented Security Controls)

### A. التوثيق وتشفير كلمات المرور (Authentication & Password Security)
- **الخوارزمية:** **PBKDF2 مع SHA-256** والملح العشوائي (**Salt**) الفريد لكل حساب عبر `PasswordHasherService`.
- **منع التعديد (Account Enumeration Prevention):** إرجاع رسائل خطأ موحدة وعامة عند فشل تسجيل الدخول دون تحديد ما إذا كان البريد موجوداً أم لا.

### B. إدارة الصلاحيات (Authorization & Role Integrity)
- **حراس الأدوار (Role Guards):**
  - `Admin`: محمي بحارس الصلاحيات `[Authorize(Roles = "Admin")]`.
  - `Driver`: `[Authorize(Roles = "Driver,Admin")]`.
  - `Traveler`: `[Authorize(Roles = "Traveler,Admin")]`.
- **حظر تغيير الدور الذاتي:** منع المستخدم من تعديل دوره أو منح نفسه صلاحيات الإدارة عند التسجيل أو تعديل الملف الشخصي.

### C. حماية ثغرات الوصول المباشر (IDOR Guard)
- عدم الثقة نهائيًا بالـ IDs الممررة في رابط الصفحة أو المدخلات الخفية.
- التحقق السيرفري الصارم من ملكية الحجز، والمدفوعات، والإيصالات، والمحادثات، باستخلاص هوية المستخدم الموثوقة من `ClaimsPrincipal` `NameIdentifier`.

### D. حماية الهجمات النصية والتزوير (CSRF, XSS & Rate Limiting)
- **CSRF:** حماية كافة عمليات الإرسال POST برمز `[ValidateAntiForgeryToken]`.
- **XSS:** الترميز النصي التلقائي التام في محرك Razor والتأكد من خلو المشروع من `Html.Raw` على بيانات المدخلات.
- **Rate Limiting:** إضافة سياسة تقييد معدل الطلبات `StrictLimiter` لحماية نقاط الدخول الحساسة من هجمات القوة الغاشمة.
