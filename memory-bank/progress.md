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
