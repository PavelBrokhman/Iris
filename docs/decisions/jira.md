# The backlog in Jira

Decided 2026-10-24 (Pavel). **The way of working with Jira lives in Claude Hub**,
`D:\Projects\claude-hub\shared\jira\README.md`: a space per project, epics as themes, subtasks as the parts of a feature
(not the steps of the process, 2026-10-26), statuses, dependencies, how an item is written, and how the decision came about. It was
written here first the same day and moved there (commit e18cd8f of the hub), because it holds for
every project, not only for Iris.

What is Iris's own:

- **The space `IRIS`** on https://pavlogix.atlassian.net, epics *Way of working* and *Agent*.
- **The credentials:** `D:\Projects\Iris\.env`, gitignored: `JIRA_PROJECT_KEY` (the URL of a
  space), `JIRA_EMAIL`, `JIRA_API_KEY`. Never into the chat. Each project keeps its own keys
  (Pavel, 2026-10-24); until then the file was `D:\Projects\claude-hub\.env.iris`.
- **The tool:** `scripts/jira.py` (`IRIS-7`) - `IRIS` by default, `--project` for the other spaces
  of the umbrella, `link A B` = A blocks B. Run it with `python -X utf8` when an item has Cyrillic.
