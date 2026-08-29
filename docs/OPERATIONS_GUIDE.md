# دليل التشغيل والعمليات — مشروع المسافر AlMosafer
## Local XAMPP & Production Operations Guide

---

## 💻 1. متطلبات بيئة التشغيل المحلية (Local Prerequisites)
- **إطار العمل:** .NET 10 SDK (`net10.0`).
- **خادم قواعد البيانات:** XAMPP Control Panel مع تفعيل خدمة **MySQL / MariaDB** على المنفذ `3306`.
- **قاعدة البيانات:** `mosafir_db`.

---

## 🚀 2. خطوات تشغيل التطبيق (Run Instructions)
1. تأكد من تشغيل خادم MySQL عبر لوحة تحكم XAMPP.
2. افتح نافذة الأوامر في مجلد المشروع `c:\xampp\htdocs\almosafir`.
3. نفذ أمر التشغيل:
   ```bash
   dotnet run --project src\AlMosafer.Web --urls http://localhost:5163
   ```
4. افتح المتصفح على الرابط: `http://localhost:5163`.

---

## 🛠️ 3. أوامر الفحص والتحقق والنشر (Build, Test & Release Publish)
- **بناء الحل البرمجي:**
  ```bash
  dotnet build AlMosafer.slnx
  ```
- **تشغيل حزمة الاختبارات البرمجية:**
  ```bash
  dotnet test tests\AlMosafer.Tests\AlMosafer.Tests.csproj
  ```
- **نشر إصدار الإنتاج النهائي (Release Publish):**
  ```bash
  dotnet publish src\AlMosafer.Web\AlMosafer.Web.csproj -c Release
  ```

---

## 📞 4. التواصل الفني والتشغيلي
- **مسؤول الدعم:** **عمار عادل المصوعي**
- **الهاتف:** `712275038` | **الواتساب:** `https://wa.me/967712275038`
