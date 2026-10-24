# How a session starts

Decided 2026-10-24 (Pavel).

No bootstrap. Iris's `CLAUDE.md` loads by itself and holds the rules decided here, pointing to
`docs/`; Claude reads `docs/STATUS.md` at the start, and a project's own `CLAUDE.md` before working
on it.

**Why not Claude Hub's three questions:** two of them are settled for good - the Gateway is always
in, the `sandbox/` / `shared/` routing always on - and Full / Lite is only about tokens, not about
the work.
