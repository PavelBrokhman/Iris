# Status

Last updated: 2026-10-24.

What is built and checked, and where the work is. Written when a branch lands. The tasks
themselves are in Jira; the decisions with their reasons in [decisions/](decisions/README.md).

## Built and checked

Nothing of the agent yet - documents only.

## Done

- 2026-10-16: the folder, as the umbrella over the video projects and the home of the agent
  (decided by Pavel: Iris replaces the planned "Video Studio"; VLC Playlists joined the same day).
  Documents: [architecture/spec.md](architecture/spec.md) (the tasks, from real cases),
  [knowledge.md](knowledge.md).
- VS Code workspace: `D:\Projects\VSC Workspaces\Iris.code-workspace` - Iris, Mosaic Remover,
  VideoToSRT, VLC Playlists, AI Gateway, Claude Hub; the terminal opens in Iris.
- 2026-10-24: the way of working settled with Pavel - [decisions/](decisions/README.md). Branches
  `DEV`, `QA`, `PROD` from `master`; the remote https://github.com/PavelBrokhman/Iris (`IRIS-5`).
  The backlog of the umbrella written into Jira. `docs/` in Milo's layout (`IRIS-4`).

## Where the work is

Jira, https://pavlogix.atlassian.net - a space per project ([decisions/jira.md](decisions/jira.md)):

| Space | Project | Epics |
|---|---|---|
| `IRIS` | Iris | Way of working, Agent |
| `MOSAIC` | Mosaic Remover | Player, Checks, Ecosystem, Backend and UI, Known problems, Diagnostic, Results and files, Improve picture, Window |
| `VIDEOTOSRT` | VideoToSRT | Run modes, Transcription quality, Translation, OCR, Deployment and services, Host UI, Design ideas |
| `GATEWAY` | AI Gateway | Media for Iris, For VideoToSRT |
| `HUB` | Claude Hub | Jira |

The "Next" list this file held until 2026-10-24 is in
[archive/next-steps-2026-10-16.md](archive/next-steps-2026-10-16.md), with the story each item became.
