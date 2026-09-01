# Progress

**Stabilization Sprint**
- [x] 1. Fix registration database mismatch & migrate complete DB schema on port 3308.
- [x] 2. Fix search query builder and sorting.
- [ ] 3. Fix profile stars() crash.
- [ ] 4. Create missing chat.php module.
- [ ] 5. Move OpenAI integration to backend proxy.
- [ ] 6. Implement CSRF protection.
- [ ] 7. Implement session_regenerate_id(true).
- [ ] 8. Fix logout redirect issues.
- [ ] 9. Fix dashboard duplicate rows.
- [ ] 10. Fix booking race conditions.

**What works**
- Core PHP pages and complete DB setup for BlaBlaCar features.
- User registration with unique phone validation and unified styling.

**Known issues**
- Authentication and session security flaws (being fixed).
- Missing messaging modules (being fixed).
- Dynamic search filter crashes (being fixed).

## P40 «النبض الحي» (2026-08-31) — مُسلَّم ✅ (CI 33445047592 أخضر)
- SignalR: Hubs/AppHub.cs ([Authorize], مجموعات user-{id}) + RealtimeNotificationServiceDecorator (يلتف حول NotificationService بلا تعديله — بث بعد حفظ القاعدة، فشل البث لا يُسقط الحفظ) + تسجيل DI مصنع في Program.cs + MapHub("/hubs/app") + AddSignalR.
- عميل: wwwroot/lib/signalr/signalr.min.js (v8.0.7 رسمي من npm registry — jsdelivr/unpkg/cdnjs محجوبة؛ SHA-256 موثق في README.txt بجانبه) + js/realtime.js (reconnect تلقائي + toast + شارة) + قائمة Layout مشروطة بالتسجيل + NotificationsController.Count.
- manifest: أربعة shortcuts للتثبيت. تقرير: docs/PROMPT_40_REALTIME_REPORT.md.
- عملية: ملف workflow تسرّب مرة (drift قديم بمحفزات arena) — استُرجع وأُعيد الالتزام --amend؛ القاعدة قائمة: لا تلمس ci-cd.yml أبداً.
- main HEAD: دمج 9c59c816. fix5 (EnableRetryOnFailure) CI 33443864157 أخضر مؤكد.

## «النبض الحي» الموجة ٢ + ختم الحقوق (2026-08-31) — مُسلَّم ✅ (CI 33445982815 أخضر)
- RealtimeMessageServiceDecorator :IMessageService (يلتف MessageService؛ يقرأ طرفي المحادثة AsNoTracking؛ بث conv-{id}+user-{recipient}؛ فشل البث آمِن).
- AppHub.JoinConversation(int) بفحص عضوية DB (IDOR مرفوض صامتاً). realtime.js: حدث ReceiveMessage + CustomEvent almosafer:chat-refresh + almosafer:realtime-join (إعادة انضمام بعد reconnect) + window.__uid من Layout.
- Details.cshtml: data-conversation-id + refresh() مستخرجة؛ polling 15ث شبكة أمان فقط.
- Footer Layout: «© 2026–2027 منصة المسافر — حقوق الطبع محفوظة لدى عمار الموعي» (طلبه حرفياً؛ ملاحظة: قسم «معلومات المنظومة» وقسم SupportTeam بالاسم الكامل عمار عادل المصوعي/712275038 من يد المالك في 74bab19).
- خارطة: ترويسة قرار المالك «تنفيذ هذا الشهر» + البند 2 مُعلَّم ✅. main: merge 46d5d8d7. تقرير: docs/PROMPT_41_LIVE_CHAT_REPORT.md.

## إصلاح عرضي (2026-08-31) — رابط الإعدادات الدائم ✅ (CI 33446550690)
- Navbar: رابط «⚙️ الإعدادات» مباشر بجانب الإشعارات (خارج userMenu — كان المالك لا «يستطيع الضغط» عليه للوصول لبطاقة الأسماء).
- تشخيص «شبكة الخطوط غير متصلة» عند المالك: صفحات /Lines وأخواتها تقرأ RouteLines (هجرة Phase2) — على الأرجح لم تُطبَّق لديه (dotnet ef توقف صامتاً). أُرشد: تثبيت dotnet-ef ثم database update حتى سطر النجاح. لم أُضف تخفيفاً صامتاً (إخفاء عطب إعداد = غير أمين).
- ملاحظة صحة: greps أظهرت صحة runtime كاملة سابقاً على جداول Phase1 فقط — لذا الصفحات الأخرى عملت والخطوط لا.

## التهجير التلقائي عند الإقلاع (2026-08-31) — بصلاحية المالك ✅ (CI 33449708813)
- Program.cs: بعد Build()/ForwardedHeaders — `db.Database.Migrate()` داخل try/catch تحذيري (مطابق لنمط بذر الأدمن). يزيل اعتماد dotnet-ef اليدوي نهائياً. السبب الموثق: المالك عالق على أداة ef صامتاً مرتين + كان على فرع v1.2-chat-api قديم (2eb2a76) — انتقل لـmain بـFast-forward من e1e607e (375 ملف/103 ألف سطر) بعد تشخيص git branch --show-current.
- database/Phase2_RouteLines.sql وRUN-ALMOSAFER.bat أصبحا بديلين احتياطيين فقط (متعايشان idempotent مع Migrate).
- تغيير سياسة معمارية مُوثّق: «التطبيق لا يهاجر تلقائياً» انتهت بقرار المالك الصريح.

## baseline الشفاء الذاتي (2026-08-31) — الجذر النهائي ✅ (CI 33500883443 أخضر، merge 986fc16)
- الجذر الموثق من سجل النسخة الجديدة: قاعدة mosafir_db وُلدت أيام PHP (db_setup.sql) ⇒ __EFMigrationsHistory فارغ ⇒ Migrate كان يصطدم بـ«users already exists» قبل Phase2. الإصلاح: Program.cs يفحص users؛ إن وُجد بلا سجل ⇒ CREATE history IF NOT EXISTS + INSERT IGNORE InitialCreate ثم Migrate يكمل Phase2 فقط. مع فحص route_lines صريح وسطر حل جاهز.
- حادثة بيئية موثقة: استنساخ ضحل جديد (depth=1) فقد سلسلة الفروع محلياً ⇒ pushes رُفضت؛ الحل: fetch صريح للفرع + reset --hard + rebase يفشل في الضحل ⇒ أعدت تطبيق التعديل يدوياً ودفعت. القاعدة المضافة: في هذا الـsandbox تحقق من rev-list --count قبل أي git عملية حساسة.

## P42 «التجديد البصري» (2026-09-01) — CI 33503232178
- طبقة CSS 2026 فوق التوكنات القائمة + صورة بطل hero-journey.png أصيلة (مولّدة، بلا نص) + إعادة بناء Home/Index (نفس form/asp-actions) + .rvl بـIO محترم للحركة + تذييل ليلي.
- صفر عدادات مختلقة؛ صفر C#؛ Razor تتحقق CI. وتقرير docs/PROMPT_42_VISUAL_RENEWAL_REPORT.md.
- دمجيات تاليتان: 4f4ff27 — أخضر. سؤال مفتوح على المالك: تعميم الهوية على الصفحات الداخلية أم بند الخارطة التالي؟

## P43 «جولة التلميع» الموجة ١ (2026-09-01) — CI 33517671291
- plan: docs/P43_UI_FIX_PLAN.md (12 عيباً موثقاً بأدلة → 3 موجات). مشحون: توحيد btn-primary/secondary، زر انضمام، favicon من الشعار، datalist مدن (HomeController +ILineService + ViewBag.Cities؛ جداول الخطوط فارغة نظرياً عند المالك إلى أن يضيفها الأدمن)، navProgress، .alm-toast، قص حافلة الجوال 62%.
- درس حوكمة sandbox: منصة الحفظ تدفع arena بنفسها بين إجازاتي ⇒ أي push رفض FF → fetch+reset --hard FETCH_HEAD ثم إعادة تطبيق التعديلات (rebase يستحيل في استنساخ ضحل). سجّلته في systemPatterns.

## P43 الموجة ٢ (2026-09-01) — CI 33526333067 ✅
- Lucide 1.39.0 UMD + flatpickr 4.6.13 + l10n/ar مورَّدة من registry.npmjs.org (بصمات sha256 في README.txt بجانب كل مكتبة).
- 23 عنواناً h1-h3 بلا إيموجي → data-lucide (createIcons في site.js) + navbar بأيقونات + إخفاء «بحث عن رحلة» عند الرئيسية.
- flatpickr: input[type=date] → altInput عربي j F Y، إرسال Y-m-d ثابت → صفر مسّ للنماذج/الربط.
- data-paginate(N) عميلي: 8 tbody + 2 حاويتي بطاقات. درس: استنساخ ضحل — تعافيت بـreset --hard 9066bd8 (main محلياً) مع نسخ احتياطي /tmp.
- حادثة مُصلَحة: استبدال الإيموجي أصاب ViewData["Title"] (سلسلة C#) في 3 ملفات → «; expected» (CI 33525923275)؛ الإصلاح 9c83d54 بنزع الوسم من العنوان وإبقائه في h3. قاعدة جديدة: بحث الاستبدال الجماعي يجب أن يستبعد سلاسل Razor المقتبسة لا خصائص HTML فقط.
