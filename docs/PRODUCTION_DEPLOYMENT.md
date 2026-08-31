# دليل النشر والتشغيل السحابي خطوات إجراية — مشروع المسافر AlMosafer
## Step-by-Step Production Deployment Guide

---

## 🚀 1. خطة النشر على السحاب (Cloud Deployment Workflow)

```text
[ Developer Machine ] ── Push Code ──► [ GitHub Repository ]
                                              │
                                              ▼
                                    [ GitHub Actions CI/CD ]
                                      (Restore, Build, Test)
                                              │
                                              ▼
                                    [ Render Web Service ]
                                   (Docker / .NET 10 Build)
                                              │
                                              ▼
                                 [ Cloud MySQL Database ]
```

---

## 🛠️ 2. خطوات النشر التفصيلية (Step-by-Step Instructions)

### الخطوة 1: تجهيز المستودع و GitHub Actions
1. رفع كود المشروع إلى مستودع GitHub الخاص.
2. يتكفل ملف `.github/workflows/ci-cd.yml` بإجراء الفحص التلقائي للبناء والاختبارات الـ 59 عند كل Push.

### الخطوة 2: إعداد قاعدة البيانات السحابية (Cloud Database)
1. إنشاء حزمة MySQL مجانية على خادم سحابي (مثل Aiven أو Railway).
2. استيراد مخطط وبيانات `scratch/mosafir_db_backup.sql` إلى قاعدة البيانات السحابية.

### الخطوة 3: إعداد خدمة الويب على Render
1. إنشاء **Web Service** جديدة على Render وربطها بمستودع GitHub.
2. إضافة المتغيرات البيئية (`Environment Variables`):
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `ConnectionStrings__DefaultConnection` = `Server=cloud_host;Port=3306;Database=mosafir_db;User=cloud_user;Password=cloud_pass;SSLMode=Preferred;`
   - `AdminSettings__Email` = `admin@almosafir.com`
   - `AdminSettings__Password` = `Admin@123456`
3. النقر على **Deploy Web Service**.

---

## 🌐 3. فحص الرابط الميداني بعد النشر (Post-Deployment Verification)
- رابط التطبيق السحابي: `https://almosafir-app.onrender.com`
- رابط فحص الصحة والسلامة السحابي: `https://almosafir-app.onrender.com/health`
