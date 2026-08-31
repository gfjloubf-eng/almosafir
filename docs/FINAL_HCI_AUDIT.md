# التدقيق النهائي لتفاعل الإنسان مع الحاسوب — مشروع المسافر AlMosafer
## Master Enterprise HCI & Usability Audit Matrix

---

## 📌 1. مصفوفة تدقيق المبادئ الـ 10 لـ Nielsen Norman

| المبدأ (Heuristic) | التقييم والتطبيق البرمجي الفعلي | الشاشة / الواجهة | الدليل والتحقق | الحالة |
|---|---|---|---|:---:|
| **1. Visibility of System Status** | إبراز شارات الحالة الملونة (مؤكد/معلق/ملغى) ورسائل التنبيه | كافة الواجهات و `/health` | Bootstrap Badges & Alert Toasts | **مفحوص** |
| **2. Match Between System & Real World** | مصطلحات نقل بري يمني مألوفة (انطلاق، وصول، مقاعد) | واجهة البحث والرحلات | `TripsController.cs` & Views | **مفحوص** |
| **3. User Control & Freedom** | إمكانية التراجع، تصفح المسارات عبر Breadcrumbs | كل الصفحات الرئيسية | `_Breadcrumbs.cshtml` Navigation | **مفحوص** |
| **4. Consistency & Standards** | اتساق الهيكل والخط والأنماط البصرية باللغة العربية | كافة الواجهات | `site.css` & RTL Bootstrap 5 | **مفحوص** |
| **5. Error Prevention** | التحقق من صحة المدخلات والتأكد من المقاعد بالسيرفر | نموذج الحجز والتسجيل | Client & Server Validation | **مفحوص** |
| **6. Recognition Rather Than Recall** | إبراز خيارات البحث والمدن الشائعة في واجهة سهلة | واجهة البحث والواجهة الرئيسية | Search Form Autocomplete | **مفحوص** |
| **7. Flexibility & Efficiency** | مرونة الوصول والتنقل وتوفير اختصارات سريعة | لوحات التحكم الثلاث | Dashboards Layouts | **مفحوص** |
| **8. Aesthetic & Minimalist Design** | واجهة حد أقصى من البساطة والنقاء دون إعلانات حشو | كافة الواجهات | Clean UI Card Architecture | **مفحوص** |
| **9. Help Users Recover from Errors** | رسائل أخطاء صديقة للمستخدم ومفهومة دون كشف الأسرار | صفحات الأخطاء والرسائل | StatusCodePagesWithReExecute | **مفحوص** |
| **10. Help & Documentation** | إبراز اسم وسائل اتصال مسؤول الدعم فوريًا بالفوتر | الفوتر وكافة الواجهات | **عمار عادل المصوعي — 712275038** | **مفحوص** |

---

## 📱 2. تجاوب الشاشات ودعم ذوي الهمم (Accessibility & Responsiveness)
- **RTL & Fonts:** محاذاة يمين-إلى-يسار كاملة مع خط Tajawal الاحترافي.
- **Responsiveness:** تجاوب مرن ومفحوص على الشاشات (`375px`, `768px`, `1024px`, `1440px`, `1920px`).
- **WCAG 2.2 AA:** دعم الكامل للوحة المفاتيح (Focus Outline)، ووسوم ARIA القياسية.
