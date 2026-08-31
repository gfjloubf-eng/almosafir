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
