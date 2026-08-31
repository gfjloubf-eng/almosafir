# AREA_AI_MASTER_BASELINE_REPORT.md

## تقرير خط الأساس الرئيسي — PROMPT 29 (تدقيق + تحضير توحيد)
- **التاريخ:** 2026-08-31 | **المنفذ:** Area AI
- **النطاق:** AUDIT + CONSOLIDATION PREPARATION فقط — لا تطوير، لا دمج، لا دفع، لا تعديل بيانات.
- **المنهجية:** `git fetch --all --tags` + فحص كل مرجع عبر `git ls-tree`/`git show` (قراءة فقط من قاعدة كائنات Git). لم تُبدَّل الفروع ولم يُلمس أي ملف مصدر. بيئة التدقيق **بلا .NET SDK ولا MySQL** → كل حكم «تشغيلي» موسوم صراحةً NOT EXECUTED.

---

## 1. الملخص التنفيذي

المستودع يحوي تاريخاً خطياً واحداً من **6 التزامات**، قمته تحمل نظامين متعايشين في نفس الشجرة:

```
b4bd4fc (v0.8, v1.0 ← develop, production)     PHP MVP
 └ e1e607e (v1.1 ← main, origin/HEAD)          PHP + براند + أساس دردشة
   └ 9dee05f                                   PHP + Chat API
     └ 7e10caf (v1.2.1-chat-csrf-fix)          PHP
       └ be3742d (v1.3-production-ready, v1.3.0-rc ← PHP!)
         └ 74bab19  ← v1.2-chat-api            ★ C#/.NET 10 الكامل + بقايا PHP
```

**النظام المرجعي الحقيقي (C#) موجود ومتماسك هندسياً** على `v1.2-chat-api@74bab19`، لكنه غير مدمج إلى `main`، ولا موسوم، وCI لا يفحصه، واختباراته لم تُنفَّذ في هذا التدقيق، ورابط إنتاجه الموثّق معطوب حالياً. PHP سلف تاريخي — **ليس** تبعية نشطة لنظام C#.

## 2. الفرع المرجعي لـ C#
- **`v1.2-chat-api`** (الاسم مضلل — المحتوى هو قفزة C# الكبرى وليس v1.2).
- هو الفرع **الوحيد** في كامل المستودع الحاوي لأي ملف C# (0 ملفات .cs في بقية الالتزامات الخمسة — مُتحقق بعدّ شجرة كل التزام).

## 3. الالتزام المرجعي لـ C#
- **`74bab19` — "release: prepare AlMosafer for cloud deployment"**
- يحوي: `AlMosafer.slnx`، `src/AlMosafer.{Domain,Application,Infrastructure,Web}`، `tests/AlMosafer.Tests`، `docs/` (51 وثيقة)، `Dockerfile`، `.dockerignore`، `.github/workflows/ci-cd.yml`، ترحيل EF `20260828154624_InitialCreate`.

## 4. تصنيف PHP القديم
| الملف | التصنيف | ملاحظة |
|---|---|---|
| صفحات الجذر `*.php` (index…admin) | LEGACY/HISTORICAL | سلف وظيفي؛ لا مرجع له من C# |
| `api/*.php` | LEGACY | بديله خدمات C# |
| `config/db.php`, `security.php`, `helpers/ui.php` | LEGACY | منفذ 3308 وليس 3306 |
| `db_setup.sql` | HISTORICAL مرجع مخطط | يُنشئ 6 جداول فقط (يفترض users/trips موجودتين!) — conversations فيه تحمل `booking_id`+`last_message_at` وقد انتقل ذلك فعلاً لترحيل EF |
| `install.php`, `environment_check.php`, `public/.htaccess` | LEGACY (أدوات استضافة PHP/Apache) | ظهرت في طور الإنتاج PHP |
| `memory-bank/`, `TODO*.md`, تقارير Security PHP | DOCUMENTATION تاريخية | تصف حقبة PHP |
| `almosafir-react`, `almosafir_flutter/` | DOCUMENTATION/نوايا | لا كود فعلي |
| **ACTIVE DEPENDENCY؟** | **لا** | صفر إشارة من أي ملف C# إلى PHP |

**تلوث الأدوات بـ PHP:** `Dockerfile` ينفذ `COPY . ./` في مرحلة البناء (ملفات PHP تدخل سياق البناء فقط؛ الصورة النهائية تُنسخ من `/app/publish` حصرياً ⇒ **لا تلوث وقت-تشغيل**)؛ `.dockerignore` لا يستبعد `*.php` (تحسين لاحق غير عاجل)؛ CI لا يتفاعل مع PHP إطلاقاً.

## 5. بنية المستودع (عند 74bab19)
خمسة مشاريع + اختبارات + 51 وثيقة + حاوية + CI + بقايا PHP في الجذر. كيانات Domain: User, Trip, Booking, Payment, Rating, Notification, Conversation, Message + 5 Enums. Application: ‏32 DTO + 14 واجهة + OperationResult. Infrastructure: DbContext + Factory + 8 تكوينات + ترحيل واحد + 13 خدمة. Web: ‏11 متحكماً + 31 عرضاً.

## 6. تحقق البنية النظيفة
- اتجاه التبعية: Web→Application→Domain و Infrastructure→Application→Domain — **مؤكد من `.csproj` ProjectReferences**. ✓
- **نقاء Domain كامل:** الـ usings الوحيدة في الدومين هي `AlMosafer.Domain.Enums` — لا EF/HTTP/UI. ✓
- **لا DbContext في أي متحكم** (مُتحقق بالبحث الشامل)، EF في Web مقتصر على `Program.cs`/`.csproj` (Composition Root — مقبول). ✓
- كل الخدمات الـ14 مسجلة `Scoped` — مدة صحيحة لـ EF Core. ✓ لا تبعيات دائرية.

## 7. تحقق قاعدة البيانات
- ترحيل EF `InitialCreate` يُنشئ 8 جداول بأعمدة snake_case وفهارس وفريد `users.Email` وFKs (Cascade). الاتساق **كود↔ترحيل مؤكد ملفياً** (Configurations ↔ Designer).
- **الاتساق مع قاعدة XAMPP الحية: UNKNOWN** — القاعدة الحية على جهاز المالك ولا تطالها بيئة التدقيق؛ لم يُنفَّذ أي استعلام/ترحيل (ويجب ظهار: فحص→نسخة احتياطية→مقارنة→ترحيل آمن، لا شيء تدميري).
- ملاحظة مفاهيمية: مخطط EF ومخطط PHP يتشاركان النموذج (conversations.booking_id في كليهما) — تهجير البيانات موثَّق في `docs/DATA_MIGRATION_REPORT.md`.

## 8. تحقق المصادقة
Cookie `AlMosafer.AuthToken` (HttpOnly، SameSite=Lax، 14 يوماً منزلقة) + Claims (Id/Name/Email/Role) + `PasswordHasher<User>` (PBKDF2/SHA256+ملح عشوائي — يطابق الادعاء) + Anti-Forgery على كل POST + حارس Open-Redirect (`Url.IsLocalUrl`) + DataAnnotations عربية صارمة (بريد/هاتف/حد أدنى 6/مقارنة كلمتين). ✓ IMPLEMENTED.
**درس إعادة التوجيه:** الكود الحالي يستخدم `User.IsInRole` داخل `RedirectToDashboard()` بعد `SignInAsync` — يعمل (SignInAsync تبدّل principal في الطلب نفسه) لكنه **يخالف الصيغة الموثقة («الدور الصريح من نتيجة المصادقة»)**. لم يُغيَّر؛ موثَّق كمخاطرة انحدار تتطلب اختبار حارس قبل أي لمس مستقبلي.

## 9. تحقق حساب المدير
- `SeedDefaultAdminAsync(email,password)` يقرأ من `AdminSettings`، ينظّف/يطبّع البريد، **يتحقق من عدم وجود مدير/بريد مكرر**، يبني مستخدم `Role=Admin` ويخزّن **التجزئة فقط** عبر خدمة التجزئة الآمنة. ✓ الآلية سليمة ولا تكرار حسابات.
- ⚠️ الافتراضيان المدمجان (`admin@almosafir.com` / كلمة مرور افتراضية `Admin@123456`) يجب ألّا يصلا للإنتاج — بيانات المدير الحقيقية تُمرَّر بيئياً ولن تُطبع في أي ملف/تقرير (التزمت بذلك هنا).

## 10. مصفوفة التخويل (مؤكدة سمةً-سمةً)
| المتحكم | الحماية |
|---|---|
| AdminController | `[Authorize(Roles="Admin")]` على الصنف — كل الأفعال الـ11 (Users/UserDetails/Trips/Bookings/Payments/Ratings/Notifications/Conversations/SystemHealth/Reports/Dashboard) |
| DriverController | `Driver,Admin` | 
| TravelerController | `Traveler,Admin` |
| TripsController | Index/Details عمومي؛ Create/MyTrips = `Driver,Admin` |
| Bookings/Conversations/Notifications/Payments | `[Authorize]` + فحص ملكية داخل الخدمات |
| RatingsController.Create | `Traveler` |
| Home/Account | عمومي/`[Authorize]` حسب الفعل |

## 11. التدقيق الأمني (مصنّف بصراحة)
- **IMPLEMENTED:** CSRF/AntiForgery على POSTs • XSS (ترميز Razor الافتراضي) • Open-Redirect • PBKDF2 • IDOR (ResourceOwnershipService + حراس داخل الخدمات) • SQLi (EF مُعامِلاتي) • كوكي آمن • HSTS/HTTPS-Redirect/ForwardedHeaders • صفحة خطأ عامة للإنتاج • تحقق DataAnnotations • فريد البريد • لا أسرار مودَعة (فُحصت الشجرة).
- **PARTIALLY:** Rate limiting (**مسجّل `StrictLimiter` 30/دقيقة لكن بلا `[EnableRateLimiting]` في أي مكان ⇒ غير فعّال فعلاً**) • Security headers (لا CSP/X-Frame-Options) • حماية brute-force للدخول (تعتمد على المحدِّد المعطّل).
- **DOCUMENTED ONLY:** «جاهز سحابياً»، «نشر»، «مكتمل 100%» (عبارات PHP-era متبقية).
- **MISSING:** حصر فاشل-الدخول عددياً، 2FA (خارج النطاق الأكاديمي).

## 12. تدقيق سلامة الحجز
`BookingService.CreateBookingAsync`: معاملة **Serializable** (تُتخطى لـ InMemory فقط) + رحلة مفتوحة + رفض الرحلات الماضية + منع حجز السائق لنفسه + منع التكرار النشط + المقاعد = `Seats − Σ(Confirmed)` + مبلغ **خادمي** decimal (`Seats×PricePerSeat`) + دفع+أرباح+محادثة+إشعاران داخل المعاملة نفسها + Commit/Rollback. الإلغاء: صلاحية (مسافر/سائق/مدير) + `Cancelled` يحرر المقاعد آلياً (لأن التوافر محسوب من المؤكدة) + الدفع→`Failed` (لا استرجاع — مقصود أكاديمياً). **الثغرات المسجلة:** فحص التكرار تطبيقي فقط (لا قيد فريد جزئي على DB) وأداء الاعتماد على Serializable تحت حمل حقيقي لم يُقَس.

## 13. تدقيق الدردشة
Conversation(BookingId?,LastMessageAt) + Message + خدمتا Conversation/Message + متحكم + عرضان + **حارس ملكية داخل الخدمة** + إشعارات. الحكم: **COMPLETE (وظيفي MVC أساسي)** — بلا WebSockets/Real-time (HTTP polling بنمط MVC)، وبلا فهرسة ظاهرة لـ conversation_id في Messages (فرصة أداء).

## 14. تدقيق التقارير والإدارة
ReportingService: 8 حسابات (Users/Trips/Bookings/Payments/Ratings/Routes/Drivers/TimeSeries) بمرشح تاريخ **يُصحَّح آلياً عند العكس (swap)** + AsNoTracking + Chart.js مؤكد في العرض. ⚠️ ملاحظات أداء: تجميع جزئي في الذاكرة بعد `ToListAsync` (أدوار المستخدمين، إحصاء الرحلات بـ Include(Bookings) كاملاً) — صحيح وظيفياً لكنه يجرّ بيانات بدل GROUP BY؛ مناسب لحجم أكاديمي، ويحتاج إسقاطات SQL عند النمو. AdminService بفلاتر وبحث وTake محدود؛ DashboardService الأثقل (18 AsNoTracking) بلا ترقيم صفحات حقيقي (Take فقط، لا Skip) — فرصة.

## 15. تدقيق HCI/UX
RTL كامل `lang="ar" dir="rtl"` + Bootstrap.rtl + تنقّل مبني على الدور في _Layout + **Breadcrumbs مكوّن حقيقي واعٍ للمسار** + **حالات فارغة قابلة للتنفيذ** (مثال مؤكد في Trips/Index: رسالة + إجراء استرداد «عرض جميع الرحلات») + رسائل أعمال عربية إنسانية في كل الخدمات + AccessDenied عربي (403) بإجراءات عودة — لكنه **عام وليس دوري-السياق كما تدّعي بعض الوثائق**. ✅ (مع تحفظَي 403 والخط).

## 16. تدقيق الوصول (Accessibility)
skip-nav مرئي-عند-التركيز ✓ • `:focus-visible` شامل ✓ • ARIA على القائمة والـ breadcrumbs ✓ • عناوين/تباين معقولان ✓ • **الخط الفعلي Cairo — وليس Tajawal المدوَّن** (فرق توثيق/كود) • jquery-validation غير المتطفل مضمَّن للنماذج ✓.

## 17. تدقيق الأداء
AsNoTracking في 9 خدمات (عدّ مؤكد) ✓ • Take في قوائم رئيسية • لا Skip ⇒ **لا ترقيم حقيقي** • تجميع ذاكري في التقارير (أعلاه) • فريد Email ✓ (فهارس بحث FromCity/ToCity غير مرئية في التكوينات — تُدار بالمرشحات جزئياً) • لا N+1 ظاهر (Includes مقصودة). لم يُقَس شيء — تدقيق ساكن فقط.

## 18. حالة الاختبارات
- 11 ملفاً، **عدّ ساكن: 59 سمة [Fact]/[Theory]** (UnitTest1 قالب = ≈58 اختباراً حقيقياً) عبر: Auth(10)، Reporting(11)، Admin(8)، Security/Hardening(7 — منها اختبارات انعكاسية لسمات الأدوار + تجزئة مملّحة + OperationResult)، Rating(7)، Booking/Trip(5)، Messaging/Payment(4)، Dashboard(3)، Domain(2)، DbHealth(1).
- xUnit 2.9.3 + EF InMemory 9.0.0 + coverlet. **النتيجة: NOT EXECUTED — لا .NET SDK في بيئة التدقيق، ولن يُختلق رقم.** «61+» الموثّق لا يطابق العدد الساكن.

## 19. حالة البناء
**NOT EXECUTED** (نفس السبب). مؤشرات ساكنة إيجابية: مراجع سليمة، مساحات أسماء متسقة، لا ملفات مفقودة في `.slnx`. غير مؤكد: توافق EF/Pomelo **9.0.0 على TFM `net10.0`** حتى بناءٍ حقيقي.

## 20. حالة Docker
Dockerfile صحيح (SDK 10.0 build → restore بالـ slnx → publish Web → aspnet:10.0 نهائي، `ASPNETCORE_ENVIRONMENT=Production`, `PORT=8080`, EXPOSE 8080). الحكم: **LOCAL/GITHUB-SOURCE READY** — لم يُبنَ في هذا التدقيق.

## 21. حالة CI/CD
`.github/workflows/ci-cd.yml`: restore→build(Release)→test→publish. ✅ ملفياً. 🔴 **المحفِّز `push/pull_request` على `main|master` فقط ⇒ النظام الحالي (v1.2-chat-api) لا يُفحص مطلقاً.** لم يُلاحَظ أي run ناجح مثبت. الحكم: CI-DEFINED لكن NOT-ARMED للفرع المرجعي.

## 22. حالة النشر السحابي
بتاريخ هذا التدقيق: `https://almosafir-app.onrender.com/` و`/health` ⇒ **`Not Found`**. الحكم الرسمي: **CLOUD-CONFIGURED (وثائق/ملفات) — وليس CLOUD-DEPLOYED**. أي ادعاء نشر في الوثائق غير صالح حالياً.

## 23. تضاربات التوثيق (أمثلة مؤكدة)
1) وسما `v1.3-production-ready/v1.3.0-rc` يشيران لالتزام **PHP** لا C#. 2) «61+ اختباراً» ≠ 59 ساكناً. 3) «Rate limiting» موثّق ومعطّل. 4) «RedirectToDashboard بدور صريح» ≠ الكود (User.IsInRole). 5) «Tajawal» ≠ Cairo. 6) «AccessDenied دوري» ≠ عام. 7) «مكتمل 100%/جاهز إنتاج» من حقبة PHP تتناقض مع TODO المفتوحة. 8) OperationResult «الموحّد» غير مستخدم من الخدمات (tuples). 9) `main` نفسها في حد ذاتها «توثيق كاذب» للزائر الجديد.

## 24. مخاطر حرجة
- **CR-1** النظام الحقيقي خارج `main` وخارج CI (أي مساهم/مقيّم يستنسخ main يحصل PHP).
- **CR-2** حماية معدل الطلبات معطلة ⇒ تسجيل الدخول مكشوف لـ brute-force رغم الادعاء.
- **CR-3** بناء/اختبار C# لم يُثبتا في أي بيئة موثوقة بعد (EF9/NET10 غير متحقق).

## 25. مخاطر عالية
- **HR-1** ادعاء نشر سحابي غير قائم (رابط ميت) — فجوة مصداقية أكاديمية. **HR-2** كلمة مدير افتراضية fallback. **HR-3** نصوص `${VAR}` بـ appsettings.Production لا يوسّعها ASP.NET (يلزم env override موثّق). **HR-4** اسم الفرع/الوسوم مضللة (v1.2-chat-api يحمل قفزة C#؛ v1.3 على PHP).

## 26. مخاطر متوسطة
- **MR-1** فحص منع تكرار الحجز تطبيقي فقط (لا قيد DB جزئي). **MR-2** خلط PHP داخل شجرة/سياق بناء C#. **MR-3** تجميع تقارير ذاكري + لا ترقيم. **MR-4** توثيق يكذب الكود (يضلل الطالب في الدفاع الأكاديمي). **MR-5** لا اختبار حارس لدرس إعادة التوجيه.

## 27. مخاطر منخفضة
- **LR-1** بقايا قوالب `Class1.cs`×3 و`UnitTest1`. **LR-2** Cairo≠Tajawal. **LR-3** AccessDenied عام. **LR-4** فهارس إضافية (Messages.ConversationId، بحث المدن). **LR-5** اسم branch وحيد يحمل تاريخين.

## 28. استراتيجية التوحيد الموصى بها (خطة، لا تنفيذ)
التاريخ **خطي بالكامل** ⇒ التوحيد آمن تقنياً كـ **fast-forward** دون فقد أي شيء:
1. إثبات الخط الأساسي: تشغيل `dotnet build/test/publish` على `74bab19` ببيئة .NET 10 حقيقية (جهاز المالك/CI) وتثبيت الأرقام.
2. قرار PHP (غير تدميري): إبقاؤه مؤقتاً مع إعلانه LEGACY في README، أو تحريكه لفرع `legacy/php` — **بعد** اعتماد المالك.
3. `git checkout main && git merge --ff-only v1.2-chat-api` ⇒ main = C#. (+ إعادة تسمية الفرع لاحقاً `release/csharp-baseline`، وتصحيح وسوم الإصدار.)
4. توسيع محفِّزات CI لتشمل main فعلياً بعد الدمج + فحص actions.
5. حزمة ما بعد التوحيد (غير مانعة): تفعيل RateLimiter، appsettings env-override، Admin seed قسري التكوين، `.dockerignore` يستبعد `*.php`.
6. لا حذف للفروع/الوسوم القديمة؛ لا force-push؛ commit واحد توثيقي بخطة التوحيد.

## 29. ترتيب التطوير الموصى به (بعد التوحيد)
P0 حوكمة/إثبات (بناء+اختبار+دمج) → P1 أمن فعلي (Rate limiting، إنتاج config، Admin seed) → P2 دقة توثيق (محاذاة كل ادعاء بالكود) → P3 أداء التقارير/ترقيم → P4 ميزات الرؤية (MyJourney/Trust…) ميزةً-ميزة باختبار مصاحب.

## 30. القرار النهائي لخط الأساس
# 🟡 B — CONSOLIDATE AFTER BLOCKER FIXES
**المسوّغ:** الدمج نفسه آمن بنيوياً (FF، لا فقدان)، والكود المرجعي متماسك ومُدقَّق ساكناً عبر 36 محوراً. لكن مبدأ «لا أساس بلا إثبات» يرفض إعلان خط أساس لنظام **لم يُبنَ ولم تُختَبر حزمته قط** في بيئة موثوقة (CR-3)، وحماية مُعلَّنة وهي معطلة (CR-2)، وادعاء نشر كاذب واقعياً (HR-1). **المانع الأدنى قبل التوحيد:** ① تشغيل build/test/publish ناجحة وموثقة على `74bab19` ② قرار مالك المشروع بشأن أرشفة PHP ③ اعتماد خطة الدمج/الوسم المكتوبة أعلاه. بعدها يصبح: **A — SAFE TO CONSOLIDATE** فوراً.

---
*انتهى التقرير. لم تُنفَّذ أي تعديلات على الكود أو قاعدة البيانات أو Git. بانتظار الاعتماد.*
