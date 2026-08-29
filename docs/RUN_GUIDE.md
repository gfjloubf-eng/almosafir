# دليل التشغيل والاختبار التفصيلي — مشروع المسافر AlMosafer

---

## 🛠️ 1. متطلبات بيئة التشغيل
1. **برنامج XAMPP Control Panel**:
   - تشغيل خدمة **Apache** (اختياري).
   - تشغيل خدمة **MySQL** على المنفذ الافتراضي `3306`.
2. **إطار عمل .NET SDK**:
   - إصدار `.NET 10.0` مثبت على الجهاز.

---

## 🚀 2. خطوات التشغيل من السطر البرمجي (Terminal / PowerShell)

### الخطوة الأولى: الانتقال لمجلد المشروع
```bash
cd C:\xampp\htdocs\almosafir
```

### الخطوة الثانية: بناء الحل البرمجي (Build)
```bash
dotnet build AlMosafer.slnx
```
*النتيجة المتوقعة:* `Build succeeded. 0 Errors, 0 Warnings`.

### الخطوة الثالثة: تشغيل حزمة الاختبارات الآلية (Run Tests)
```bash
dotnet test tests/AlMosafer.Tests/AlMosafer.Tests.csproj
```
*النتيجة المتوقعة:* `Passed! - Failed: 0, Passed: 52, Total: 52`.

### الخطوة الرابعة: تشغيل التطبيق (Run Web Application)
```bash
dotnet run --project src/AlMosafer.Web --urls http://localhost:5163
```

---

## 🌐 3. الوصول للتطبيق وحسابات التجربة الأكاديمية

افتح المتصفح وانتقل للرابط:
```text
http://localhost:5163
```

### بيانات الحساب الإداري الافتراضي (Admin Account):
- **البريد الإلكتروني:** `admin@almosafir.com`
- **كلمة المرور:** `Admin@123456`

---

## 🛠️ 4. حل المشاكل البرمجية والتأكد من البيئة (Troubleshooting)

### مشكلة عدم القدرة على الاتصال بقاعدة البيانات (MySQL Error 1042 / Connection Refused):
- **السبب:** خدمة MySQL غير شغالة في XAMPP أو المنفذ `3306` مشغول.
- **الحل:** افتح XAMPP Control Panel واضغط **Start** بجانب MySQL.

### مشكلة المنفذ مشغول (Port 5163 in use):
- **الحل:** قم بتنفيذ الأمر مع منفذ آخر مثل `http://localhost:5000`:
```bash
dotnet run --project src/AlMosafer.Web --urls http://localhost:5000
```
