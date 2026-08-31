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
