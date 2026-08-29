# دليل نقل ونشر قاعدة البيانات السحابية — مشروع المسافر AlMosafer
## Database Cloud Deployment & Migration Guide

---

## 📌 1. إجراء النسخ الاحتياطي المحلي (Local Backup Procedure)
قبل إجراء أي نقل، يتم تصدير نسخة SQL كاملة ومضمونة لقاعدة بيانات `mosafir_db` المحلية من XAMPP:
```bash
mysqldump -u root -p mosafir_db > scratch/mosafir_db_backup.sql
```

---

## 🗄️ 2. إجراء الاستيراد والترحيل السحابي (Cloud Import & Migration)
1. الاتصال بخادم MySQL السحابي باستخدام بيئة اتصال آمنة أو عبر MySQL Workbench / CLI.
2. إنشاء قاعدة البيانات السحابية المستهدفة `mosafir_db`.
3. تنفيذ أمر التصدير والاستيراد لإنشاء الجداول وحشو البيانات:
```bash
mysql -h cloud_host -P 3306 -u cloud_user -p mosafir_db < scratch/mosafir_db_backup.sql
```

---

## 🧪 3. المطابقة والتحقق من سلامة البيانات (Data Verification Protocol)
بعد الاستيراد، يتم إجراء استعلامات مطابقة الأعداد والتأكد من مطابقة السجلات في كلا البيئتين:
- جدول المستخدمين `users`: مطابقة إجمالي عدد الحسابات.
- جدول الرحلات `trips`: مطابقة إجمالي الرحلات البرية.
- جدول الحجوزات `bookings`: مطابقة إجمالي الحجوزات.
- جدول المدفوعات `payments`: مطابقة المعاملات المالية المسجلة.
- جدول التقييمات `ratings`: مطابقة التقييمات.
