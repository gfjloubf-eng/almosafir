# AREA_AI_CURRENT_SYSTEM_AUDIT.md

## تدقيق النظام الحالي — Area AI (اقرأ-فقط، بلا تعديل)
- **التاريخ:** 2026-08-31 | **المستودع:** `gfjloubf-eng/almosafir`
- **المنهجية:** سحب كامل التاريخ والوسوم والفروع البعيدة، ثم فحص شجرة كل التزام عبر Git object database (قراءة فقط — لم أبدّل الفروع ولم أعدّل ملفاً ولم أشغّل أي تثبيت).

---

# 🎯 القرار الجوهري الأول: أين يعيش نظام C# فعلاً؟

خرافدة سابقة (كانت بناءً على `main` فقط) صُحّحت بالفحص العميق لجميع المراجع:

| المرجع (Ref) | الالتزام | المحتوى |
|---|---|---|
| `main` + `origin/HEAD` + وسامي `v1.1-chat-foundation` | `e1e607e` | **PHP فقط** |
| `develop` + `production` + وسما `v0.8`/`v1.0-stable` | `b4bd4fc` | PHP MVP القديم |
| وسما `v1.2.1` / `v1.3-production-ready` / `v1.3.0-rc` | `7e10caf` → `be3742d` | PHP + Chat API |
| **`v1.2-chat-api` (فرع)** ⭐ | **`74bab19`** | **PHP + نظام C# الكامل** (`src/`, `AlMosafer.slnx`, `tests/`, `docs/`, `Dockerfile`, `.github/`) |

> **النظام الحالي (C#/.NET 10) موجود حقاً** — لكنه يعيش **حصرياً** على الفرع `v1.2-chat-api` عند الالتزام `74bab19` ("release: prepare AlMosafer for cloud deployment")، وهو **سليل مباشر** لخط تطوير PHP (الجذر المشترك `b4bd4fc`). و**لم يُدمج إلى `main` أبداً**، ولا يحمل وسماً، واسم فرعه مضلل (v1.2 بينما `main` عند v1.1).

### خريطة النسب الكاملة (5 التزامات + رأس واحد)
```
b4bd4fc (v0.8, v1.0 ← develop, production)   PHP MVP
   └─ e1e607e (v1.1 ← main)                  PHP + Branding + Chat DB
      └─ 9dee05f                             PHP + Chat API
         └─ 7e10caf (v1.2.1)                 PHP + إصلاح CSRF للدردشة
            └─ be3742d (v1.3-production-ready, v1.3.0-rc)   PHP + واجهة دردشة
               └─ 74bab19 ← v1.2-chat-api    ★ نظام C#/.NET 10 الكامل + بقايا PHP
```

---

# الجزء A — الفصل الحاسم بين النظامين (المطلوب في القسم 13)

## CURRENT C# SYSTEM
- **files:** 232 ملفاً تحت `src/` (5 مشاريع) + `tests/AlMosafer.Tests/` + `AlMosafer.slnx` + `Dockerfile` + `.dockerignore` + `.github/workflows/ci-cd.yml` + `docs/` (51 وثيقة).
- **projects:** `AlMosafer.Domain` → `AlMosafer.Application` → `AlMosafer.Infrastructure` → `AlMosafer.Web` (+ Tests) — تبعيات مؤكدة من `.slnx` و`.csproj`.
- **technologies:** C# / `net10.0`، ASP.NET Core MVC + Razor، EF Core (`9.0.0`) + Pomelo MySQL (`9.0.0`)، xUnit + EF InMemory + coverlet، Bootstrap RTL.
- **CURRENT ENTRY POINT:** `src/AlMosafer.Web/Program.cs` (DI + Cookie Auth + RateLimiter + HealthChecks + ForwardedHeaders + HSTS).
- **CURRENT DATABASE:** `mosafir_db` عبر EF Core؛ نص الاتصال التطويري `localhost:3306` (root بلا كلمة)؛ ترحيل EF واحد: `20260828154624_InitialCreate` (+ Factory للتصميم). جداول: users, trips, bookings, payments, ratings, notifications, conversations, messages.
- **CURRENT TEST PROJECT:** `tests/AlMosafer.Tests/AlMosafer.Tests.csproj` — **عدّ ساكن: 59 سمة [Fact]/[Theory]** (منها UnitTest1 الافتراضي ⇒ ~58 فعلياً). ❗ لم يُشغَّل في بيئة التدقيق (لا يوجد .NET SDK هنا) → **الحالة: مكتوب، غير مُنفَّذ للتحقق**.
- **CURRENT DEPLOYMENT MODEL:** Dockerfile متعدد المراحل (sdk:10.0 → aspnet:10.0، منفذ 8080) + CI (build/test/publish). **الحالة الفعلية الآن: NOT DEPLOYED** — رابط الإنتاج الموثّق `almosafir-app.onrender.com` يجيب `Not Found` على `/` و`/health` (فُحص مباشرة بتاريخ هذا التقرير).

## LEGACY PHP SYSTEM
- **files:** 12+ صفحة PHP في الجذر (`index`, `login`, `book`, `payments`, `admin`…) + `api/*.php` + `config/db.php` + `db_setup.sql` + `security.php` + `install.php`, `environment_check.php`, `public/.htaccess` + `memory-bank/` + تقارير TODO/Security.
- **purpose:** النسخة الأكاديمية الأولى (أسلاف Git مباشرة، v0.8→v1.3).
- **relationship to current system:** **LEGACY/HISTORICAL بامتياز** — سلف النسب للكود الحالي، ومخططه (`db_setup.sql`) هو أصل مخطط EF (لا بل حلّ C# محل TODO قديم: كيان `Conversation` يملك فعلاً `BookingId` + `LastMessageAt`).
- **ACTIVE DEPENDENCY: NO** — لا ملف C# واحد يشير إلى PHP. لكن ⚠️ ملفات PHP ما زالت **موجودة في شجرة الفرع نفسه** وتُنسخ داخل حاوية Docker (`COPY . ./`، و`.dockerignore` لا يستبعد `*.php`).
- **RECOMMENDATION:** عدم تعديلها؛ فصلها لاحقاً بتفويض صريح (نقل لفرع `legacy/php-archive` أو حذف من فرع C# بعد الاعتماد).

---

# الجزء B — الإجابات الـ36 المطلوبة

**1) ما النظام الحالي؟** منصة mobility عربية RTL لحجز رحلات بين المدن (سائق/مسافر/مشرف)، ASP.NET Core MVC على .NET 10، بنية نظيفة — موجودة على `v1.2-chat-api@74bab19`.
**2) التقنيات:** C#13/.NET10 + MVC + EF Core/Pomelo + MariaDB + Razor + xUnit + Docker + GH Actions. ✅ مؤكد من التكوينات الفعلية.
**3) ما PHP؟** السلف الأكاديمي — `main` كله ما زال PHP. غير نشط لنظام C#.
**4) المعمارية:** Clean Architecture حقيقية (لا مجرد ادعاء): Web→Application(عقود/DTOs)→Domain(كيانات/Enums خالصة)؛ Infrastructure تُنفّذ العقود. ✅ مؤكد بالمراجع والمساحات الاسمية.
**5) المشاريع الخمسة:** Domain (8 كيانات+5 Enums) / Application (14 واجهة+32 DTO+OperationResult) / Infrastructure (DbContext+8 Configurations+Migration+13 خدمة) / Web (11 متحكماً+31 عرضاً) / Tests (11 ملفاً). ✅
**6) الملفات الحرجة:** `Program.cs` (الخط الأنبوبي كاملاً)، `AccountController`، `AdminController`، `BookingService`، `TripService`، `PaymentService`، `RatingService`، `ReportingService`، `PasswordHasherService`، `ResourceOwnershipService`، `AlMosaferDbContext`، `UserConfiguration` (فهرس Email فريد).
**7) الخدمات الكبرى (13):** Auth, Trip, Booking, Payment, Rating, Reporting, Admin, Dashboard, Notification, Conversation, Message, PasswordHasher, ResourceOwnership (+DbHealth). كلها مسجّلة Scoped في DI. ✅
**8) الكيانات:** User(Email فريد/Role/TotalEarnings/Rating…)، Trip(Seats/Status/PricePerSeat/TripTime)، Booking(SeatsBooked/Status)، Payment، Rating، Notification، Conversation(BookingId?,LastMessageAt)، Message — +Enums خامسة. ✅
**9) المصادقة:** Cookie Auth (`AlMosafer.AuthToken`, HttpOnly, SameSite=Lax, 14 يوماً منزلقة) + Claims (Id/Name/Email/Role) + PBKDF2 عبر `PasswordHasher<User>` + Open-Redirect محروس (`Url.IsLocalUrl`) + Anti-Forgery على كل POST. ✅ كودياً.
**10) التخويل:** مصفوفة أدوار على مستوى الأصناف/الأفعال: AdminController=`Admin`، Driver=`Driver,Admin`، Traveler=`Traveler,Admin`، Trips: البحث/التفاصيل علني وCreate/MyTrips للسائق، Bookings/Conversations/Notifications/Payments=`[Authorize]`، Ratings.Create=`Traveler`. +IDOR عبر ResourceOwnershipService (تجاوز Admin مقصود). ✅ كودياً.
**11) المسافر:** لوحة (`GetTravelerDashboardAsync`) → بحث عمومي → تفاصيل → حجز → إيصال → تقييم → مراسلة/إشعارات. ✅
**12) السائق:** نشر/تعديل/إلغاء رحلة (TripService) + رحلاتي + لوحة أرباح + محادثات الركاب. ✅
**13) المشرف:** Dashboard + إدارة Users/Trips/Bookings/Payments/Ratings/Notifications/Conversations + Reports + SystemHealth — كلها خلف `Roles="Admin"`. ✅ كودياً.
**14) الحجز:** معاملة `Serializable` + فحص: رحلة مفتوحة، وقت مستقبلي، منع حجز السائق لنفسه، منع التكرار النشط، حساب المقاعد من مجموع الحجوزات المؤكدة، ثم إنشاء الحجز → دفع → أرباح السائق → محادثة → إشعاران → Commit/Rollback. **نمط سليم هندسياً.** ✅ كودياً (تشغيلياً: غير مُتحقق — لم تُشغَّل الاختبارات).
**15) حماية المقاعد:** الحساب من `Sum(Confirmed)` داخل معاملة متسلسلة ⇒ لا overbooking تصميمياً (التحقق التشغيلي يتطلب اختبار حمل حقيقياً على InnoDB — InMemory لا يحاكي الأقفال).
**16) الدفع:** سجلات دفع فقط (سعر من الخادم = Seats×PricePerSeat — موثوقية حسابية ✅)؛ بلا بوابة حقيقية (مقصود أكاديمياً)؛ الإلغاء يعلّم الدفع `Failed` (لا استرجاع).
**17) التقييم:** CreateRate مربوط بأهلية الحجز (`CanTravelerRateBookingAsync`) + ملخص سائق مجمّع. ✅
**18) المراسلة:** محادثة لكل حجز (`EnsureBookingConversationExistsAsync`)، وصول مفلتر بالملكية، `LastMessageAt`. ✅
**19) التقارير:** `GetAdminReportSummaryAsync(ReportFilterDto)` + DTOs إحصائية (Users/Trips/Bookings/Payments/Ratings/Routes/DriverPerformance/TimeSeries) + **Chart.js مؤكد في `Admin/Reports.cshtml`**. ✅
**20) قاعدة البيانات:** EF Code-First بترحيل `InitialCreate` واحد؛ تكوينات Fluent منفصلة لكل كيان؛ نص الإنتاج بمتغيرات `${…}` — ⚠️ **ASP.NET لا يوسّعها تلقائياً** (يلزم `ConnectionStrings__*` بيئياً وإلا يُقرأ النص حرفياً).
**21) الأمن الفعلي:** CSRF ✅ / XSS (Razor encoding افتراضي) ✅ / IDOR ✅ / Roles ✅ / Hashing ✅ / HSTS+HTTPS+ForwardedHeaders ✅ / Health endpoint ✅ / **RateLimiting: مسجّل لكن غير مفعّل — لا `[EnableRateLimiting]` في أي مكان** 🔴 / Admin seed بكلمة افتراضية `Admin@123456` عند غياب التكوين ⚠️.
**22) HCI الفعلي:** `dir="rtl" lang="ar"` ✅، Bootstrap RTL ✅، رسائل عربية إنسانية في الخدمات ✅، Breadcrumbs جزئي ✅، AccessDenied عربي إنساني — **لكنه عام، وليس موجَّهاً حسب الدور كما تدّعي الوثائق** ⚠️.
**23) الوصول:** skip-nav ✅، `:focus-visible` ✅، ARIA على القائمة ✅، الخط الفعلي **Cairo — وليس Tajawal كما يوثّق** ⚠️.
**24) الاختبارات:** 59 سمة (≈58 حقيقية) تغطي Auth/Admin/Rating/Reporting/Security/Dashboard/Messaging/Health/Domain/Booking. **لم تُنفَّذ في بيئة التدقيق.**
**25) المكسور حالياً:** (أ) `main` لا يعكس النظام الحالي (حوكمة)، (ب) Rate Limiter غير مطبَّق، (ج) رابط الإنتاج ميت (`Not Found`)، (د) CI لا يشمل فرع C# (يشغّل main/master فقط).
**26) المكتمل جزئياً:** OperationResult (موجود **لكنّ الخدمات لا تستخدمه** — تعيد tuples)؛ AccessDenied (عام لا دوري)؛ توحيد الخط (Cairo≠Tajawal)؛ تناسق الإصدارات (EF 9 على net10).
**27) الموثَّق فقط (غير مُتحقق تشغيلياً):** «61+ اختباراً» (الساكن 59)، «Cloud Ready»، النشر على Render، تغطية الأداء، دقة التقارير.
**28) الجاهز إنتاجياً فعلاً:** البنية والكود والحاوية — كملفات. تشغيلياً: يحتاج build/test/run ناجحة + قاعدة بيانات + تفعيل الحدّ — **لم يُثبت شيء منها بيئياً هنا**.
**29) المنشور سحابياً فعلاً:** **لا شيء** — الرابط يرد `Not Found` بتاريخ التدقيق. الحالة الصادقة: CLOUD-CONFIGURED (ملفات) وليس DEPLOYED.
**30) حالة Git:** 5 التزامات خطية + رأس `74bab19` على `v1.2-chat-api` فقط؛ `develop`/`production` متقادمتان (MVP قديم)؛ وسما v1.3 يشيران لكود PHP — **تضليل إصداري**.
**31) لا يُعدَّل باستخفاف:** `Program.cs`، `AccountController` (درس الدور)، `BookingService` (التزامن/الماليات)، `UserConfiguration` (الفهرس الفريد)، `InitialCreate` (لا يُعدَّل — يُضاف ترحيل)، `db_setup.sql` (مرجع تاريخي)، وقاعدة البيانات الحقيقية (ممنوع DROP/TRUNCATE).
**32) ملفات التطوير المستقبلي المرجّحة:** Controllers/Views ذات الصلة بالميزة + خدمة Infrastructure واحدة + DTO + اختبار + ترحيل EF بسيط عند الحاجة — دون لمس النوى في (31).
**33) الدَّين التقني:** `Class1.cs` ×3 (قوالب)، `UnitTest1.cs`، نتائج tuple بدل OperationResult، RateLimiter معطّل، PHP مخلوط في شجرة C# وتُنسخ للحاوية، EF9/NET10، تضارب منافذ تاريخي (3306 توثيق C# / 3308 كود PHP)، غياب خصائص تنقّل حرجة؟ (تم التحقق — موجودة)، انعدام Rate-limiting للدخول تحديداً.
**34) أعلى 10 مخاطر:** ① حوكمة Git (النظام الحقيقي غير مدمج/موسوم و`main` خادعة) ② CI لا يفحص فرع C# ③ Rate limiting غير فعّال ④ نص إنتاج `${}` غير موسَّع ⑤ كلمة Admin افتراضية ⑥ الحالة المنشورة وهمية (الرابط ميت) ⑦ اختبارات غير منفَّذة (رقم 61 غير مؤكد) ⑧ تضارب التوثيق/الكود (دور-التوجيه، Tajawal، 403 الدوري) ⑨ EF9 على net10 بلا تحقق ⑩ خلط PHP في التوزيع.
**35) أعلى 10 فرص:** ① دمج `v1.2-chat-api` → `main` (PR رسمي) ووسم صحيح v2.0.0-csharp، ② توسيع CI لكل الفروع + تشغيله فعلاً، ③ تفعيل `[EnableRateLimiting("StrictLimiter")]` على حسابات الدخول/الحجز، ④ استبدال `${}` بآلية env override موثقة، ⑤ إخضاع Admin seed لمتغير إلزامي بلا افتراضي خطير، ⑥ تشغيل build/test/publish ببيئة .NET حقيقية وتثبيت الرقم الفعلي، ⑦ اعتماد OperationResult عبر الخدمات (أو حذفه — قرار معماري واحد)، ⑧ AccessDenied دوري الوعي (الدرس الموثق موجود)، ⑨ فصل PHP إلى أرشيف وتحديث `.dockerignore` ⑩ توحيد الخط والمنافذ وتحديث سجل الإصدارات الصادق.
**36) أأمن طور تطوير تالٍ:** **"طور التثبيت والمصداقية" قبل أي ميزة جديدة:** دمج الفرع إلى main ← تشغيل CI وتثبيت رقم الاختبارات ← تفعيل الحدّ والإنتاج ← smoke فعلي — ومن ثم ميزات الرؤية (My Journey, Trust Score…) كسلائسل صغيرة لكلٍّ منها اختبارها.

---

# الجزء C — تصنيف الملفات (القسم 18)

| التصنيف | الملفات |
|---|---|
| **CORE / لا يُعدَّل باستخفاف** | `Program.cs`, `AccountController`, `AdminController`, `BookingService`, `ResourceOwnershipService`, `PasswordHasherService`, `AlMosaferDbContext`, `*Configuration.cs`, `20260828154624_InitialCreate*`, `appsettings*.json`, `AlMosafer.slnx` |
| **FEATURE** | بقية الخدمات + `Trips/Bookings/Payments/Ratings/Conversations/Notifications/Traveler/Driver` Controllers |
| **INFRASTRUCTURE** | كل `Infrastructure/` + `Dockerfile` + `.dockerignore` |
| **UI** | كل `Views/**` + `wwwroot/**` |
| **TEST** | `tests/AlMosafer.Tests/*.cs` |
| **CONFIGURATION** | `appsettings*`, `launchSettings.json`, `ci-cd.yml` |
| **DOCUMENTATION** | `docs/**` (51) + تقارير الجذر |
| **LEGACY** | كل `*.php` بالجذر و`api/*.php`, `css/`, `db_setup.sql`, `install.php`, `environment_check.php`, `public/.htaccess`, `almosafir-react`, `almosafir_flutter/`, `memory-bank/`, `TODO*.md`, تقارير Security PHP |
| **UNKNOWN (لا يُعدَّل حتى يُفهم)** | `Class1.cs` (بقايا قالب ×3 — أغلب الظن حذفها آمن لاحقاً بعد اعتماد)، ملف `# Cloude Code ToolBox….md` |

---

# الجزء D — مصفوفة "ادعاء ↔ واقع" (القسم 11)

| الادعاء | الحكم |
|---|---|
| Clean Architecture / 5 مشاريع | ✅ IMPLEMENTED (مؤكد ملفياً) |
| الأدوار + IDOR + CSRF + Hashing + Redirect-guard | ✅ IMPLEMENTED (مؤكد كودياً) |
| Rate limiting | 🔴 DOCUMENTED لكن **NOT ACTIVE** (مسجّل وغير مطبَّق) |
| 61+ اختباراً | ⚠️ الساكن 59؛ التنفيذ UNVERIFIED هنا |
| CI/CD | ⚠️ ملف ✅ لكن لا يشمل فرع C# ⇒ عملياً غير مُفعّل للنظام الحالي |
| Docker | ⚠️ ملف صحيح؛ لم يُبنَ هنا |
| Cloud Deployed (Render) | 🔴 NOT DEPLOYED (الرابط `Not Found` الآن) |
| Tajawal / AccessDenied دوري | ⚠️ كود يخالف التوثيق (Cairo / عام) |
| إصلاح "دور-التوجيه" بالدور الصريح | ⚠️ الكود يستخدم `User.IsInRole` مجدداً (يعمل لأن SignInAsync يبدّل principal؛ لكنه يخالف الدرس الموثّق ويستحق حارس اختبار) |
| reports/Chart.js | ✅ IMPLEMENTED |

---

# الخاتمة

النظام الحالي **حقيقي ومتماسك** هندسياً — لكنه "شبح تنظيمي": كود ممتاز على فرع جانبي، و`main` تبيع واجهة PHP لكل من يستنسخ المستودع. أولى خطوات التطوير ليست كوداً بل **حوكمة**: دمج، وسم، CI، وتثبيت أرقام صادقة.

**الحالة: ⏸️ بانتظار اعتماد التقرير — لم يُنفَّذ أي تطوير أو تعديل.**
