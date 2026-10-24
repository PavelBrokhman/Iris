# Tests

Decided 2026-10-24 (Pavel: "beri oba").

Both rules: Mosaic Remover's - test first, seen failing against a stub; and Claude Hub's - in the
same commit as the code, and scripts that only measure are tested too.

**Why both:** a test that never failed does not show it can catch anything (Mosaic Remover); a
measuring script's numbers are quoted as fact (Claude Hub). They do not contradict: Mosaic's is
stricter and contains the hub's.

**A default, not a law** (Pavel: "inogda jeti modeli mogut ne podhodit"). Where it does not fit - a
probe in `sandbox/`, picture quality Pavel judges, a window - the Jira story says before the piece
starts what checks it instead.

Domain-driven (how the code is shaped), event-driven (how its parts talk) and test-driven (how it
is checked) are different things and live together in one project: Iris's recipes speak the words
of video, long jobs report by events, both tested first.
