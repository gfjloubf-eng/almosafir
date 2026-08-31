# Active Context

**Current focus**: C#/.NET 10 baseline is authoritative (v1.2-chat-api @ 74bab19, merged ff into work branch). PHP = legacy/historical, do not touch.

**In progress**:
- [x] Prompt 29 — master baseline audit (docs/AREA_AI_MASTER_BASELINE_REPORT.md)
- [x] Prompt 30 — baseline blocker fixes: rate limiting enabled on auth POSTs, admin seed now env-configured (no default creds), production ${VAR} placeholders removed + fail-fast guard, CI triggers cover v1.2-chat-api, .dockerignore excludes PHP/docs
- [ ] Prompt 31 — real build/test/publish proof (blocked here: no .NET SDK, MS hosts unreachable), then ff-only consolidation to main + v2.0.0-csharp-baseline tag

**Decisions**:
- Tests static count = 59 (58 real); runtime numbers NOT yet executed — never claim "61".
- Consolidation = ff-only; no force push; no tag moves without approval.

**Open questions**:
- EF 9.0.0/Pomelo 9.0.0 on net10.0 — verify on first real build.
