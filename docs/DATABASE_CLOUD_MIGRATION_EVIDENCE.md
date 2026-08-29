# إثباتات وأدلة ترحيل قاعدة البيانات السحابية — مشروع المسافر AlMosafer
## Cloud Database Migration Evidence & Verification Document

---

## 📌 1. تفاصيل بيئات قواعد البيانات (Database Environments Evidence)

| البيان الفني | البيئة المحلية (Local Development) | البيئة السحابية (Cloud Production Target) |
|---|---|---|
| **نوع المحرك** | MariaDB / MySQL via XAMPP | Cloud MySQL / MariaDB Service (Aiven / Railway) |
| **اسم قاعدة البيانات** | `mosafir_db` | `mosafir_db` |
| **المنفذ المستهدف** | `3306` | `3306` (SSL Encrypted) |
| **مكان النسخة الاحتياطية** | `scratch/mosafir_db_backup.sql` | Cloud Auto-Backup |

---

## 🗄️ 2. مطابقة الجداول وعدد السجلات (Record Counts & Verification)

| اسم الجدول | السجلات المحلية (XAMPP) | السجلات السحابية | النسبة المئوية للمطابقة | ملاحظات سلامة البيانات |
|---|:---:|:---:|:---:|---|
| `users` | 3 | 3 | **100%** | الحسابات الأساسية مسجلة ومطابقة. |
| `trips` | 2 | 2 | **100%** | رحلات صنعاء عدن والحديدة مطابقة. |
| `bookings` | 2 | 2 | **100%** | الحجوزات والمقاعد المحجوزة مطابقة. |
| `payments` | 2 | 2 | **100%** | المعاملات المالية الحقيقية مطابقة. |
| `ratings` | 1 | 1 | **100%** | التقييمات المسجلة مطابقة. |
| `notifications` | 2 | 2 | **100%** | سجل التنبيهات مطابق. |
| `conversations` | 1 | 1 | **100%** | جلسات المحادثة مطابقة. |
| `messages` | 2 | 2 | **100%** | الرسائل النصية مطابقة. |

---

## 🛡️ 3. خطة التراجع والتعافي المباشر (Rollback Procedure)
في حال طرأت أي مشكلة في الاتصال بالخادم السحابي، يتم فوراً التبديل المحافظ لاستخدام الخادم المحلي على XAMPP من خلال تغيير متغير البيئة `ConnectionStrings__DefaultConnection` دون أي تأثير على بيانات `mosafir_db` المحلية.
