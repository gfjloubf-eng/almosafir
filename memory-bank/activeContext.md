# Active Context

**Current focus**: BASELINE SETTLED — main = verified C#/.NET 10 (merge 3c498409, tag v2.0.0-csharp-baseline). CI green on main (run 33427662373). Owner proof: 63/63 tests.

**In progress**:
- [x] Prompt 29/30/31 — audit, blocker fixes, consolidation, tag
- [ ] Post-consolidation: live DB check (mosafir_db), real Docker build, optional CI widened triggers (local-only commit ready), nullable warnings cleanup in AdminService.cs

**Decisions**:
- main is the single source of truth. PHP = legacy subtree, read-only.
- Local CI-trigger commit exists but is unpushed (App lacks workflows scope).

**Open questions**:
- Owner to run phpMyAdmin/live DB verification.

## 2026-08-31 P32
نفّذت حزمة الصفحات القياسية + دورة حياة الرحلة (Started/Start/Complete) + دردشة ذاتية + تعريب + صفر تحذير nullable + 6 اختبارات جديدة. التفاصيل: docs/PROMPT_32_PAGES_AND_LIFECYCLE_REPORT.md

## 2026-08-31 P33
حزمة «محبوب» على main (486c467b) وCI أخضر (33432550810): نخبة السائقين + راقب-خطك (PreferencesJson بلا هجرة) + واتساب + سمة ليلية. التفاصيل: docs/PROMPT_33_LOVABLE_PACKAGE_REPORT.md

## 2026-08-31 P34
مدخل التسهيل على main (2b0d4c0d) وCI أخضر (33434886766): نقد عند الركوب + IPaymentGateway + تأكيد سائق + عربية المعاملة + 77 اختباراً.

## 2026-08-31 P35
المواصلات الداخلية p0 على main (026c5300)؛ CI الأول سقط لقيد قديم (رفض تطابق المدينتين) — حُرّر بتوثيق + fix-forward؛ run 33435595635 أخضر؛ 80 اختباراً.

## 2026-08-31 P36
المرحلة 2 على main (8cfe9b75) وCI أخضر (33436739056): شبكة خطوط رسمية بـ3 جداول جديدة + إدارة إدمن + 83 اختباراً؛ نجت إعادة-بذر ثانية بلا فقدان؛ خطوة المالك: dotnet ef database update (docs/LINES_PHASE2_UPGRADE.md).

## 2026-08-31 P37
حزمة يوم السفر على main (af3b98d9) وCI أخضر (33437860469): كشف ركوب + صعود Boarded + إشعارات الرحلة الأربعة + QR + 19 موضع حساب مقاعد + 86 اختباراً + أول حزمة NuGet (QRCoder).

## 2026-08-31 P38
الهوية على main (7c49f9f4) وCI أخضر (33439348146) بعد fix-forward موثق: بريد MailKit فاشل-آمن + استعادة كلمة مرور (رمز أحادي بـPreferencesJson بلا هجرة) + 90 اختباراً + الخطة العشرية docs/ROADMAP_TO_10.md.

## 2026-08-31 P39
PWA على main (b4a1a728) وCI أخضر (33440122854): تثبيت كتطبيق + فريق الدعم بالإعدادات + وثيقة التوزيع والبث docs/PWA_AND_DISTRIBUTION.md.
