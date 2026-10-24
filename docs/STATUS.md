# Status

Last updated: 2026-10-24.

What is built and checked, and where the work is. Written when a branch lands. The tasks
themselves are in Jira; the decisions with their reasons in [decisions/](decisions/README.md).

## Built and checked

- `iris check` and `iris repair cut|freeze` (`IRIS-11`, 2026-10-24) - the first recipes, moved from
  Mosaic Remover's `Diagnostic` ([decisions/check-recipes.md](decisions/check-recipes.md)): `Iris.Core`
  (recipes, ffmpeg behind `IVideoTools`), `Iris.Cli` (`iris.exe`), 30 tests. Checked live on the six
  samples of `D:\Projects\lada\samples\`: the places found match their README; cut gives a file a
  second check finds whole, freeze (NVENC) too. Two problems of Mosaic Remover's check fixed on the
  way (`MOSAIC-19`): a silent hole in the frames after a few complaints is part of the damaged place
  (`damaged-h264-nal-short.mp4`: 01:00 - 04:07, was 01:00 - 01:01); complaints of ffmpeg's null output
  are not damage (`damaged-timestamps.mp4`, `vfr-frame-rate-90000.mp4`: none, were 3 and 2 places).
  Build: `dotnet build Iris.sln`; tests: `dotnet test Iris.sln`.
- `iris diagnose` and `iris compare` (`IRIS-12`, 2026-10-24). diagnose: the file's facts - codec,
  B-frames, key frame interval, the reported against the real frame rate - each with what it means
  for Explorer, VLC and Lada, "not checked" where nothing was seen. compare: length, picture, real
  frames/s (a minute from the middle with repeats dropped), bit rate, which is better on the numbers;
  `--stills` writes frames of both from the same moments for Pavel. 55 tests. Checked live on the
  samples and on `Doch'.mp4` against its `.demosaic` (HEVC, 2 B-frames, a key frame every 2 s - as
  knowledge.md says of Lada's results); four problems found that way, each a test first.

- `scripts/jira.py` (`IRIS-7`, 2026-10-24) - Jira from the command line, on Milo's pattern, over
  Claude Hub's shared client: IRIS by default, `--project` for the other spaces, a subtask always in
  its parent's space, `link` for dependencies. Tests in `tests/test_jira.py`; checked live by reading
  issues and moving `IRIS-7`'s own subtasks. Runs in Iris's own `venv` (`requirements.txt`).

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
