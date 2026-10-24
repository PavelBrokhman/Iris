# Status

Last updated: 2026-10-24.

## Done

- 2026-10-16: the folder, as the umbrella over the video projects and the home of the agent
  (decided by Pavel: Iris replaces the planned "Video Studio"; VLC Playlists joined the same day).
  Documents only: [spec.md](spec.md) (the tasks, from real cases), [knowledge.md](knowledge.md).
- VS Code workspace: `D:\Projects\VSC Workspaces\Iris.code-workspace` - Iris, Mosaic Remover,
  VideoToSRT, VLC Playlists, AI Gateway, Claude Hub; the terminal opens in Iris.
- 2026-10-24: branches `DEV`, `QA`, `PROD` from `master` (Jira `IRIS-5`); the remote
  https://github.com/PavelBrokhman/Iris, all four pushed.

## How we work - decided 2026-10-24 (Pavel)

Which way, Mosaic Remover's or Claude Hub's, gone through point by point. Decided so far:

- **Branches.** The permanent branches are teams: `DEV`, `QA`, `PROD`. A child's prefix says what
  kind of work it is: `feature/`, `userstory/`, `docs/` under `DEV`; `test/`, `loadstress/` under
  `QA`; `release/`, `patch/` under `PROD`; the set is open. `master` stays as it is - a passing
  point, its name does not matter. Only the names changed: the rest of the hub's model (fast-forward,
  `start/` and `end/` tags, verifying in the child before landing) stays until Pavel decides otherwise.
- **Direction of the umbrella.** The target is the hub's shape: tools through AI Gateway, data
  through AI Ecosystem, the UI apart from the core. Reached piece by piece, never all at once:
  take one piece (of Mosaic Remover first), rebuild it, check it works, then the next. Meanwhile
  each project keeps working as it is - Mosaic Remover with its own ffmpeg.
  The Gateway already runs ffmpeg (`/probe`, `/extract-audio`, `/detect-silence`,
  `/extract-frames`, `/grab-frame` - made for VideoToSRT); for Iris `/probe` says too little and
  replaces an unknown fps with 25, and there is nothing for `check` / `cut` / `freeze`.
- **The backlog is in Jira.** One Jira project per workspace (not per repository - Claude Hub's
  `shared/jira/README.md` says otherwise and is to be corrected): `IRIS` for this one. Changed the same day: **a space per project**
  (`IRIS`, `MOSAIC`, `GATEWAY`, `HUB` - all made the same way, team-managed Kanban through the API, for consistency; VideoToSRT and VLC Playlists when they get tasks),
  **epic = a theme** inside it, story = one piece, subtasks = its steps, comments = the plan before
  and the check after. Free Jira has three levels (Epic - Story/Task - Subtask); the space gives the
  fourth, so Pavel's "umbrella - user story - feature - task" fits. A project used by several
  workspaces (AI Gateway: Iris, Milo) lives in one space; a workspace is a saved filter / dashboard
  over spaces, not a container; who asked is a link between issues (`blocks`) or a label (`for-iris`).
  Credentials then stay one for all (`claude-hub/.env`), the space key goes to each project's `.env`.
  **The steps of a piece are subtasks inside its story** (Pavel, 2026-10-24), as the work went in
  Mosaic Remover - not all every time, but this shape: tests written first and seen failing, code,
  tests pass, a check page for Pavel, Pavel goes through it, decide it is done. **Statuses:** In
  Progress while Claude works; In Review while it waits for Pavel; Done when it is decided.
  **A dependency:** a subtask in the asking story ("Request to <project>: <key>"), a story with its
  own subtasks in the other space, a link `blocks` between the two (done: `GATEWAY-3` blocks
  `MOSAIC-10`, `GATEWAY-2` blocks `IRIS-10` and `IRIS-12`; `MOSAIC-19` relates to `IRIS-11`).
  **An open feature branch means the work has started** (Pavel): its stories are In Progress.
  `VIDEOTOSRT` space made the same day from its branches, branch-notes, design records and memory:
  epics Run modes, Transcription quality, Translation, OCR, Deployment and services, Host UI, Design
  ideas; In Progress - the run-modes stories (branch `DEV-run-modes`) and the lab's review file and
  layering (`DEV-Analyze`); three requests to `GATEWAY` (epic For VideoToSRT) with `blocks` links.
  Mosaic Remover's features merged but not gone through by Pavel are In Review (`MOSAIC-21` …
  `-73`, eight of them); the four whose check pages he went through are not in Jira. Statuses: the default four - To Do (no
  branch), In Progress (branch open), In Review (ready, waiting for Pavel's check by hand), Done
  (checked, landed in `DEV`, documents updated); a piece dropped is closed as Done with a comment why.
- **Scratch, PoCs and tools** go by the hub's routing (`claude-hub/tmp/<project>/`, `sandbox/`,
  `shared/`), not to the Windows temp folder. **Video samples** - never into a git: one folder
  `D:\Video Samples\`, a folder per case with its own `README`. Lada's samples
  (`D:\Projects\lada\samples\`) move there; then the references in Mosaic Remover's `status.md`
  and in the memory are fixed.
- **Where things are written - Milo's layout.** Tasks, the plan of a piece, its check - Jira.
  `docs/STATUS.md` - what is built, checked, next (written when a branch lands); `docs/decisions/`
  - a file per decision, with its reason and date; `docs/design/` - alternatives, accumulated;
  `docs/architecture/` - how it is built; `docs/guides/` - how to install and to check;
  `docs/archive/` - what is closed; `knowledge.md` stays. Rules of working with Claude - memory and
  `CLAUDE.md`. Not moved yet: `spec.md` → `architecture/`, the decisions of this file → `decisions/`.
- **Tests - both rules.** Test first, seen failing against a stub; in the same commit as the code;
  scripts that only measure are tested too. A default, not a law (Pavel: "inogda jeti modeli mogut
  ne podhodit") - where it does not fit (a probe in `sandbox/`, picture quality Pavel judges, a
  window), the Jira story says before the piece starts what checks it instead. Domain-driven (how
  the code is shaped), event-driven (how its parts talk) and test-driven (how it is checked) are
  different things and live together in one project: Iris's recipes speak the words of video,
  long jobs report by events, both tested first.
- **How a session starts - no bootstrap.** Iris's `CLAUDE.md` loads by itself and holds the rules
  decided here, pointing to `docs/`; Claude reads `docs/STATUS.md` at the start, and a project's own
  `CLAUDE.md` before working on it. The hub's three questions are not needed: the Gateway is always
  in, the `sandbox/` / `shared/` routing always on, Full / Lite is only about tokens.
- **Rules of code.** Every dependency (ffmpeg, the Gateway, the disk, Lada, time) has an interface
  from the first day - a project too small for a test is rare, and a test is already a second
  implementation (Pavel); instance classes, not static. Pure recipe logic needs none - it is tested
  on data directly. A number with a meaning is always a named constant; a setting only when someone
  will change it. Also, from both sides: do not widen the scope (offer in one line); a comment only
  for a non-obvious why, none for removed code; validate at the boundaries, trust internal code;
  keep CRLF (`crlf.py`); never rewrite a source file through PowerShell without `-Encoding`; push
  back on "a chto esli" with YAGNI.

## Next

0. **Jira - connected 2026-10-24, the rest waits** until the way of working is settled (Pavel:
   other tasks will join it). Done: space `IRISAGENT` ("Iris", team-managed, types Epic / Story /
   Task / Subtask); credentials in `D:\Projects\claude-hub\.env.iris` (the hub's `.gitignore` got
   `.env.*` and `!.env.example` - not committed yet); read checked; the four sample issues deleted.
   Same day moved to **`pavlogix.atlassian.net`** (back up, Jira Software added; Kanban space
   `IRISAGENT` "IRIS"; the same token works - it belongs to the account, not the site);
   `.env.iris` points there. The golddragon2404 site is not used any more. The sample issues
   deleted; the backlog written, then spread over the spaces (created through the API): `IRIS` (made anew, the first `IRISAGENT` deleted - to the trash, 60 days) -
   epics Way of working (`IRIS-1`), Agent (`IRIS-2`); `MOSAIC` - Player, Checks, Ecosystem, Backend and UI,
   Known problems (Mosaic's status.md open work); `GATEWAY` - Media for Iris; `HUB` - Jira. From
   now on the tasks live there.
   Waiting: `scripts/jira.py` on Milo's pattern reading `.env.iris` (`IRIS-7`), the hub's README (`HUB-2`).
   The notes from before: Site:
   `https://golddragon2404.atlassian.net` - the working one. Pavel would rather have
   `pavlogix.atlassian.net` (Jira Service Management only, a test; "site is currently unavailable"
   on 2026-10-24) - being decided. Pavel's steps:
   create a Kanban project with key `IRIS`; create an API token
   (https://id.atlassian.com/manage-profile/security/api-tokens); write into
   `D:\Projects\claude-hub\.env` (gitignored) - never into the chat:
   `JIRA_PROJECT_KEY=https://pavlogix.atlassian.net/jira/software/projects/IRIS`,
   `JIRA_EMAIL=...`, `JIRA_API_KEY=...`. Then Claude: check the connection read-only, propose the
   components and epics, a `scripts/jira.py` on Milo's pattern. The hub's `shared/jira/README.md`
   names the variables wrongly (`JIRA_URL`, `JIRA_API_TOKEN`; the code reads `JIRA_PROJECT_KEY` as a
   URL and `JIRA_API_KEY`) - to fix in the hub.
1. **Merge the Claude memory** of Mosaic Remover and VideoToSRT (and VLC Playlists) into the memory
   of this folder. A draft is shown to Pavel first: what goes in, what is dropped (state that is
   only a date's snapshot), what stays in a project.
   *2026-10-24:* copied as it was, not merged (Pavel: copy as is, decide later which way is better,
   then merge) - `C:\Users\brokh\.claude\projects\D--Projects-Iris\memory\`, one folder per project,
   one `MEMORY.md` over them; the originals untouched. Mosaic Remover, run without the hub's
   bootstrap, got a `CLAUDE.md` with its working rules and their reasons the same day. Known
   differences to settle: branch names (`feature/` there, `DEV-` in the hub); Mosaic's memory note
   "RAISE AT SESSION START" of 2026-10-13 is partly out of date.
2. **The main tasks first** (Pavel, 2026-10-16 - see "The main tasks" in [spec.md](spec.md)):
   questions about a folder - which files have mosaic, which are already done, add to demosaic,
   which are of low resolution, is improving worth it. Ideas - reliability is not the point yet.
   What Iris cannot do it says so and passes on to the developers as a request (see spec.md).
3. **The Gateway first** (Iris owns the recipe, the Gateway the hands - spec.md): extend its
   `/probe` with codec, profile, B-frames, key frame interval, variable frame rate, bit rate,
   sound - in `D:\Projects\AI-Gateway`, on its own branch.
4. **First recipes**: move the recipe part of `MosaicRemover.Check` (`check`, `repair cut|freeze`) here; fix its weak
   spot first - when the end of the damage is not found, `cut` must stop with an error instead of
   cutting to the end of the file. Decide how the Mosaic Remover core is shared.
5. Then `diagnose` and `compare`.

From 2026-10-24 the open work is in Jira (space `IRIS`, and `MOSAIC`, `VIDEOTOSRT`, `GATEWAY`,
`HUB`); this list stays as it was written until the layout of `IRIS-4` replaces it.
