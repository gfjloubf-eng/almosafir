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
