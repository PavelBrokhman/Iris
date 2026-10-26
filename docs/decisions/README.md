# Decisions

A file per decision, with its reason and its date. A decision changed later keeps its old text and
gets the new one beside it, dated - the reason something was tried and dropped is worth as much as
the decision that replaced it.

The decisions of 2026-10-24 came from going through Mosaic Remover's way of working and Claude
Hub's point by point with Pavel. They were written in `STATUS.md` that day and moved here,
not rewritten.

| Decision | In short |
|---|---|
| [Branches](branches.md) | `DEV`, `QA`, `PROD` are teams; a child's prefix says the kind of work |
| [The umbrella's direction](umbrella-direction.md) | towards the Gateway, the Ecosystem and a separate UI, piece by piece |
| [The backlog in Jira](jira.md) | a space per project, an epic per theme, subtasks = the parts of a feature, the statuses |
| [Scratch and video samples](scratch-and-samples.md) | the hub's `tmp/`, `sandbox/`, `shared/`; samples outside any git |
| [Where things are written](docs-layout.md) | Milo's layout of `docs/` |
| [Tests](tests.md) | test first, seen failing; in the same commit; a default, not a law |
| [How a session starts](session-start.md) | `CLAUDE.md` and `STATUS.md`, no bootstrap |
| [Rules of code](rules-of-code.md) | an interface for every dependency, named numbers, the rest from both sides |
