# خط التأسيس والتحقق الأولي قبل النشر — مشروع المسافر AlMosafer
## Pre-Deployment Baseline & Audit Document

---

## 📌 1. بيئة النظام والمعطيات الفنية (System Environment Baseline)
- **إطار العمل المستهدف:** C# 13 / .NET 10 Target Framework `net10.0`.
- **معمارية المشروع:** Clean Architecture (`Domain`, `Application`, `Infrastructure`, `Web`, `Tests`).
- **خادم قواعد البيانات المحلي:** MySQL / MariaDB via XAMPP على المنفذ `3306`.
- **قاعدة البيانات الحالية:** `mosafir_db`.

---

## 🧪 2. نتائج الفحص الآلي الأولي (Automated Verification Baseline)
- **نتيجة البناء:** **Build succeeded. 0 Error(s), 0 Warning(s)**.
- **نتيجة الاختبارات البرمجية:** **59 Passed, 0 Failed, 0 Skipped (100% Pass Rate)**.
- **نتيجة النشر المحقق:** **Publish Succeeded** إلى `src/AlMosafer.Web/bin/Release/net10.0/publish/`.

---

## 🔒 3. حوكمة البيانات والحماية
- **سلامة قواعد البيانات:** عدم إجراء أي عملية حذف هدمي (`DROP`, `TRUNCATE`, `DELETE`).
- **فصل البيئات:** تكوين `appsettings.Production.json` و `appsettings.Example.json` لمنع تسريب أسرار وقواعد البيانات داخل مستودع Git.
- **معلومات فريق الدعم المعتمدة:** **عمار عادل المصوعي — 712275038 — واتساب 967712275038**.
