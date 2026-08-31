# خطة التعافي من الكوارث واستعادة الخدمات — مشروع المسافر AlMosafer
## Enterprise Disaster Recovery & Business Continuity Plan

---

## 📌 1. سيناريوهات الأخطار وكيفية التعامل معها (Failure Scenarios)

### السيناريو 1: انقطاع قاعدة البيانات السحابية (Database Outage)
- **الإجراء:** التبديل الآلي أو اليدوي لـ Connection String للربط مع النسخة الاحتياطية المماثلة، وإظهار صفحة خطأ آمنة للمستخدم تنوه بإجراء صيانة مؤقتة دون كشف تفاصيل Exception.

### السيناريو 2: تلف أو خمل خدمة الويب السحابية (App Service Failure)
- **الإجراء:** إعادة تشغيل الحاوية أو الخدمة السحابية فوراً (Redeploy) عبر لوحة تحكم Render أو GitHub Actions Pipeline.

---

## 🗄️ 2. جدول النسخ الاحتياطي والاستعادة (Backup & Recovery Schedule)
- **النسخ الاحتياطي التلقائي:** تصدير نسخة SQL كاملة يومياً وحفظها في مجلد آمن خارجي.
- **أمر الاستعادة المباشر:**
```bash
mysql -h localhost -P 3306 -u root -p mosafir_db < scratch/mosafir_db_backup.sql
```
