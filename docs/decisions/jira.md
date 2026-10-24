# The backlog in Jira

Decided 2026-10-24 (Pavel).

**A space per project** (`IRIS`, `MOSAIC`, `VIDEOTOSRT`, `GATEWAY`, `HUB`; VLC Playlists when it gets
tasks), all made the same way - team-managed Kanban through the API, for consistency - on
https://pavlogix.atlassian.net. **Epic = a theme** inside a space, story = one piece, subtasks = its
steps, comments = the plan before and the check after. Free Jira has three levels (Epic -
Story/Task - Subtask); the space gives the fourth, so Pavel's "umbrella - user story - feature -
task" fits.

**The steps of a piece are subtasks inside its story**, as the work went in Mosaic Remover - not
all every time, but this shape: tests written first and seen failing, code, tests pass, a check
page for Pavel, Pavel goes through it, decide it is done.

**Statuses:** To Do; In Progress while Claude works (and whenever its feature branch is open); In
Review while it waits for Pavel; Done when it is decided. A piece dropped is closed as Done with a
comment why.

**A dependency:** a subtask in the asking story ("Request to <project>: <key>"), a story with its
own subtasks in the other space, a link `blocks` between the two.

**A project used by several workspaces** (AI Gateway: Iris, Milo) lives in one space; a workspace
is a saved filter or dashboard over spaces, not a container; who asked is a link between issues or
a label (`for-iris`). Credentials stay one for all, the space key goes to each project.

## How it came to this, the same day

Kept because each step was a decision that was then changed.

- One Jira project per workspace (Pavel: "a project is the level of the workspace"), components =
  repositories, epics = directions crossing them. Then epic = a project and themes as labels. Then
  a space per project and epic = a theme - "this will be more right", and a project used by several
  workspaces lives in one place.
- Kanban, not Scrum or project management: one developer with Claude, piece after piece, no
  sprints to plan.
- The site: a first one, `golddragon2404.atlassian.net`, was made by mistake; Pavel wanted
  `pavlogix.atlassian.net`, which held Jira Service Management only and was "unavailable" for a
  while; once it was back with Jira Software added, the work moved there. The same API token
  works on both - it belongs to the account, not the site.
- The first `IRIS` space was made in the browser (`IRISAGENT`, fewer work types); it was made anew
  through the API like the others and the old one deleted (to the trash, 60 days).

## Where the credentials are

`D:\Projects\claude-hub\.env.iris` (gitignored - the hub's `.gitignore` keeps every `.env.*` out
since 2026-10-24): `JIRA_PROJECT_KEY` (a URL of a space - the client takes the site from it),
`JIRA_EMAIL`, `JIRA_API_KEY`. Never into the chat. Claude Hub's `shared/jira/README.md` names the
variables wrongly (`JIRA_URL`, `JIRA_API_TOKEN`) and says a project per repository - `HUB-2`.
A `scripts/jira.py` for Iris is `IRIS-7`.
