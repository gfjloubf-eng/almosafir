# دليل حوكمة وإدارة الأسرار والمتغيرات البيئية — مشروع المسافر AlMosafer
## Enterprise Secrets Management & Security Architecture Guide

---

## 📌 1. سياسة التعامل مع الأسرار (Secrets Security Policy)
يُحظر حظرًا تامًا تضمين أو كتابة كلمات مرور قواعد البيانات، أو أسرار التوثيق، أو مفاتيح API داخل كود المصدر C# أو في ملفات الإعدادات المسجلة على مستودعات Git.

---

## 🔑 2. تقسيم البيئات وإدارة المتغيرات البيئية (Environment Configuration Matrix)

| اسم المتغير البيئي | البيئة المحلية (Local XAMPP) | بيئة النشر السحابي (Production Cloud) |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| `ConnectionStrings__DefaultConnection` | `appsettings.Development.json` (Local) | Render/Cloud Environment Variables |
| `AdminSettings__Email` | `admin@almosafir.com` | Configured via Cloud Dashboard Secrets |
| `AdminSettings__Password` | Admin@123456 (Local Seed) | Configured via Cloud Dashboard Secrets |

---

## 🛡️ 3. قوالب الإعدادات الخالية من الأسرار
تم إنشاء الملف القالبي **`appsettings.Example.json`** الخالي تمامًا من أي أسرار ليكون مرجعًا مفتوحًا للمطورين دون خطر تسريب الأسرار في GitHub.
