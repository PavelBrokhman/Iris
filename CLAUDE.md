# CLAUDE.md — Iris

## What this folder is

Two things in one place (decided by Pavel, 2026-10-16):

1. **The umbrella for everything about video.** Video sessions with Claude are started from here,
   so the memory of this folder is the shared memory for video work. The video projects stay where
   they are, each with its own git and its own `docs/status.md`; work on them by absolute path.
2. **Iris, the video agent** - Pavel's own specialist for problems with video files (is it whole,
   which version is better, why does it stutter, cut it, fix it, match the subtitles), so that he
   does not have to come to Claude every time. What it does: [docs/architecture/spec.md](docs/architecture/spec.md).

## The projects under the umbrella

| Project | Folder | Repository | Status file |
|---|---|---|---|
| Mosaic Remover | `D:\Projects\Video Mosaic Censor Remover\` | https://github.com/PavelBrokhman/Mosaic-Remover.git | `docs/status.md` |
| VideoToSRT | `D:\Projects\VideoToSRT\` | https://github.com/PavelBrokhman/VideoToSRT.git | `docs/` (see its README) |
| VLC Playlists | `D:\Projects\VLC Playlists\` | none - local git only | `docs/ROADMAP.md` |
| Iris | `D:\Projects\Iris\` (here) | https://github.com/PavelBrokhman/Iris | [docs/STATUS.md](docs/STATUS.md) |

Also used by them: AI Gateway (`D:\Projects\AI-Gateway\`, model access) and Claude Hub
(`D:\Projects\claude-hub\` - Pavel's own notes and preferences).

Paths are last-known (2026-10-16); check before relying on them.

## How to work here

- **At the start of a session read [docs/STATUS.md](docs/STATUS.md)** - what is built and where the
  work is. No bootstrap questions: the Gateway is always in, the scratch routing always on
  ([decisions/session-start.md](docs/decisions/session-start.md)).
- **Before working on a project, read its own `CLAUDE.md`** (Mosaic Remover has one since
  2026-10-24, with its working rules and their reasons). A session started here does not load it by
  itself - only the `CLAUDE.md` of this folder is read at the start.
- **A decision about one project goes into that project** (its `status.md` / docs, its git).
  Only what concerns more than one of them, or video in general, goes here.
- **Commit in the repository the change belongs to.** Each project has its own branch habits: look
  at `git branch` there before cutting a branch. The name of the main branch does not matter to
  Pavel - it is only a passing point.
- What is known about video in general - codecs, players, key frames, damaged files, subtitles -
  is in [docs/knowledge.md](docs/knowledge.md), with the project and date it came from.

## The rules decided on 2026-10-24

Each in one line here; the reason and the history are in its file under
[docs/decisions/](docs/decisions/README.md). Iris follows them; the other projects keep their own
until their piece is rebuilt.

- **Branches** ([branches.md](docs/decisions/branches.md)): `DEV`, `QA`, `PROD` are teams; a child
  is named by the kind of work - `feature/`, `userstory/`, `docs/` from `DEV`; `test/`,
  `loadstress/` from `QA`; `release/`, `patch/` from `PROD`. Fast-forward only, `start/` and `end/`
  tags, verify in the child before landing. Commit and push only when Pavel says so.
- **Direction** ([umbrella-direction.md](docs/decisions/umbrella-direction.md)): towards Claude Hub's
  shape - tools through AI Gateway, data through AI Ecosystem, the UI apart from the core - one piece
  at a time, each checked before the next; meanwhile every project keeps working as it is.
- **Jira** ([jira.md](docs/decisions/jira.md)): https://pavlogix.atlassian.net, a space per project,
  an epic per theme, a story per piece, its steps as subtasks. In Progress while Claude works or its
  branch is open, In Review while it waits for Pavel, Done when decided. A dependency is a "Request
  to X" subtask, a story in X's space, and a `blocks` link. Credentials in
  `D:\Projects\claude-hub\.env.iris`, never in the chat.
- **Where things are written** ([docs-layout.md](docs/decisions/docs-layout.md)): tasks in Jira;
  `docs/STATUS.md` when a branch lands; a decision per file in `docs/decisions/`; `architecture/`,
  `design/`, `guides/`, `archive/` as needed - a folder appears with its first file.
- **Scratch and samples** ([scratch-and-samples.md](docs/decisions/scratch-and-samples.md)):
  scratch, PoCs and tools through Claude Hub's `tmp/<project>/`, `sandbox/`, `shared/`, not the
  Windows temp folder; video samples never in a git - `D:\Video Samples\`, a folder per case.
- **Tests** ([tests.md](docs/decisions/tests.md)): written first and seen failing, committed with
  the code, measuring scripts too. Where that does not fit, the story says beforehand what checks
  the piece instead.
- **Code** ([rules-of-code.md](docs/decisions/rules-of-code.md)): every dependency behind an
  interface from day one, instance classes; a number with a meaning is a named constant, a setting
  only when someone will change it; no widening of scope; comments only for a non-obvious why; keep
  CRLF; never rewrite a source file through PowerShell without `-Encoding`.
