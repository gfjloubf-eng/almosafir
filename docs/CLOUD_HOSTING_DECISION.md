# تقرير اختيار خيارات الاستضافة السحابية المجانية — مشروع المسافر AlMosafer
## Cloud Hosting Provider Evaluation & Decision Document

---

## 📌 1. تقييم خيارات الاستضافة السحابية المجانية (Evaluation of Free-Tier Providers)

### أ. منصة Render (Render Web Services) — **الخيار الموصى به والمختار**
- **الدعم التكنولوجي:** دعم مباشر لتطبيقات Docker و ASP.NET Core و .NET 10.
- **التكلفة:** خطة مجانية دائمًا (Free Web Service) تتيح استضافة التطبيقات مجانًا مع شهادة SSL تلقائية وحساب نطاق فرعي مجاني (`*.onrender.com`).
- **التكامل:** دعم الربط المباشر مع GitHub للنشر التلقائي عبر CI/CD ويدعم المتغيرات البيئية (`Environment Variables`).
- **القيود:** وضع الخمول (Spin-down) بعد 15 دقيقة من خمول الزيارات (يستغرق 30 ثانية للإعادة عند أول طلب).

### ب. منصة Azure App Service (Free F1 Plan)
- **الدعم التكنولوجي:** دعم رسمي ممتاز من Microsoft لـ .NET 10.
- **القيود:** حد ذاكرة RAM (1 GB) وحد زمني 60 دقيقة CPU يومياً، ويتطلب بطاقة ائتمان للتفعيل.

### ج. منصة PlanetScale / Aiven / Railway لـ MySQL Database
- **الدعم التكنولوجي:** توفير قواعد بيانات MySQL/MariaDB سحابية مجانية مع اتصال مشفر SSL.
- **التوافقية:** توافق تام مع EF Core و Pomelo MySql Provider المستخدم في المشروع.

---

## 🏛️ 2. القرار المعماري النهائي (Final Architectural Decision)
- **استضافة تطبيق الويب (Web App Host):** Render Web Services (باستخدام Docker Image المنشأ أو Native .NET Build).
- **استضافة قاعدة البيانات السحابية (Cloud DB Host):** Aiven / Railway Free MySQL Instance (سعة 5GB مجانية مع SSL).
- **البديل المحلي المعتمد (Local Baseline):** بقاء تشغيل XAMPP MariaDB محلياً على المنفذ `3306` دون تغيير.
