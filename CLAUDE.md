# CLAUDE.md — Iris

## What this folder is

Two things in one place (decided by Pavel, 2026-10-16):

1. **The umbrella for everything about video.** Video sessions with Claude are started from here,
   so the memory of this folder is the shared memory for video work. The video projects stay where
   they are, each with its own git and its own `docs/status.md`; work on them by absolute path.
2. **Iris, the video agent** - Pavel's own specialist for problems with video files (is it whole,
   which version is better, why does it stutter, cut it, fix it, match the subtitles), so that he
   does not have to come to Claude every time. What it does: [docs/spec.md](docs/spec.md).

## The projects under the umbrella

| Project | Folder | Repository | Status file |
|---|---|---|---|
| Mosaic Remover | `D:\Projects\Video Mosaic Censor Remover\` | https://github.com/PavelBrokhman/Mosaic-Remover.git | `docs/status.md` |
| VideoToSRT | `D:\Projects\VideoToSRT\` | https://github.com/PavelBrokhman/VideoToSRT.git | `docs/` (see its README) |
| VLC Playlists | `D:\Projects\VLC Playlists\` | none - local git only | `docs/ROADMAP.md` |
| Iris | `D:\Projects\Iris\` (here) | none yet - local git only | [docs/status.md](docs/status.md) |

Also used by them: AI Gateway (`D:\Projects\AI-Gateway\`, model access) and Claude Hub
(`D:\Projects\claude-hub\` - Pavel's own notes and preferences).

Paths are last-known (2026-10-16); check before relying on them.

## How to work here

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
