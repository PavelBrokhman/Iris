# Branches

Decided 2026-10-24 (Pavel).

The permanent branches are teams: `DEV`, `QA`, `PROD`. A child's prefix says what kind of work it
is: `feature/`, `userstory/`, `docs/` under `DEV`; `test/`, `loadstress/` under `QA`; `release/`,
`patch/` under `PROD`; the set is open. `master` stays as it is - a passing point, its name does not
matter.

Only the names changed: the rest of Claude Hub's model (fast-forward, `start/` and `end/` tags,
verifying in the child before landing) stays until Pavel decides otherwise.

**Why these names:** Pavel had got used to Mosaic Remover's `feature/...`, `qa/...`, `release/...`;
then `test/` rather than `qa/`, because the prefix names what is done in the branch and the
permanent branch is only the name of the team.

An open feature branch means the work has started: its stories in Jira are In Progress (Pavel).
