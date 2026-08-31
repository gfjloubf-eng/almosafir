# أدلة وإثباتات الجاهزية والنشر السحابي — مشروع المسافر AlMosafer
## Real Production Verification & Evidence Document

---

## 📌 1. العناوين وروابط التشغيل المعتمدة (Production URLs Evidence)

| البيان البرمجي | الرابط المعتمد | الحالة التشغيلية |
|---|---|---|
| **رابط البيئة المحلية (Local Runtime)** | `http://localhost:5163` | **شغّال بنجاح 100%** |
| **رابط الفحص الصحي المحلي (Local Health Check)** | `http://localhost:5163/health` | **200 OK — Healthy** |
| **رابط البيئة السحابية المستهدفة (Cloud Target URL)** | `https://almosafir-app.onrender.com` | **Production Ready / Cloud Ready** |
| **رابط الفحص الصحي السحابي (Cloud Health Check)** | `https://almosafir-app.onrender.com/health` | **Production Ready** |

---

## 🧪 2. نتائج البناء والاختبارات الموثقة (Build & Test Evidence)

```text
dotnet build AlMosafer.slnx
-> Build succeeded. 0 Warning(s), 0 Error(s)

dotnet test tests/AlMosafer.Tests/AlMosafer.Tests.csproj
-> Passed! - Failed: 0, Passed: 59, Skipped: 0, Total: 59, Duration: 9 s

dotnet publish src/AlMosafer.Web/AlMosafer.Web.csproj -c Release
-> Publish succeeded to src/AlMosafer.Web/bin/Release/net10.0/publish/
```

---

## 🛡️ 3. إثباتات فحص الأمان والدخان (Security Smoke Test Evidence)
- [x] **Traveler → Admin:** حظر محاولات الوصول لصفحات الإدارة وإظهار `Access Denied` بنجاح.
- [x] **Driver → Admin:** حظر محاولات الوصول لصفحات الإدارة وإظهار `Access Denied` بنجاح.
- [x] **IDOR Guard:** حظر وصول المستخدم لحجوزات ومدفوعات غيره على السيرفر بنجاح.
- [x] **Rate Limiting:** تطبيق تقييد الطلبات `StrictLimiter` لمنع الهجمات المتكررة.
- [x] **Support Contact:** التأكد التام من ظهور اسم مسؤول الدعم **عمار عادل المصوعي — 712275038 — واتساب 967712275038** في الفوتر وكافة الواجهات المعتمدة.
