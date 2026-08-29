# دليل أنبوب التكامل والنشر المستمر — مشروع المسافر AlMosafer
## Continuous Integration & Continuous Deployment (CI/CD) Guide

---

## 📌 1. خطة تدفق أنبوب التكامل والنشر (CI/CD Pipeline Flow)

```text
[ Developer Push ] ──► [ GitHub Repository ]
                              │
                              ▼
                   [ GitHub Actions Workflow ]
                   ├── 1. dotnet restore
                   ├── 2. dotnet build (Release)
                   ├── 3. dotnet test (59 Automated Tests)
                   └── 4. dotnet publish (Release Output)
                              │
                              ▼ (If All Tests Pass)
                   [ Render Auto-Deploy ]
```

---

## 🛡️ 2. ضوابط الأمان داخل Pipeline
- عدم تخزين أي أسرار أو كلمات مرور داخل ملف `.github/workflows/ci-cd.yml`.
- إيقاف النشر التلقائي فوراً في حال فشل أي اختبار آلي (0 Failed Test Threshold).
