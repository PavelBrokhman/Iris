# Branches

Decided 2026-10-24 (Pavel); moved to Claude Hub on 2026-11-25.

The rule is common to every project and lives in Claude Hub's `docs/conventions.md` (*Git* and
*Promotion model*): `DEV`, `QA`, `PROD` are teams; a child's prefix says the kind of work
(`feature/`, `userstory/`, `docs/` from `DEV`; `test/`, `loadstress/` from `QA`; `release/`,
`patch/` from `PROD`); the parent comes into the child by a plain merge, the child lands into the
parent fast-forward only; `start/` and `end/` tags; an open feature branch means its stories are In
Progress. Nothing here differs from it.
