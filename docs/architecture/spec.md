# Iris — what it does

Rewritten 2026-10-24 (`IRIS-88`) from what Pavel decided that day. The spec of 2026-10-16 is in
[archive/spec-2026-10-16.md](../archive/spec-2026-10-16.md): it described Iris as a program with its own
"recipes" around ffmpeg, and code was built after it inside Iris (`IRIS-10/11/12`) that belongs to
the tools.

## Three parts

| Part | Answers for | Video work |
|---|---|---|
| **UI** | what the user sees | the windows of the tools - only presentation, replaceable at any time |
| **Backend** | the processing | the tools: Mosaic Remover's backend, VideoToSRT's, ffmpeg and models through AI Gateway |
| **Agent** | the communication - with the user or with other agents | **Iris** |

**The same split on two levels** (Pavel, 2026-11-14 - said to keep the table above from being read
as "Iris has no UI and no backend"):

1. **Iris itself** is a UI and a backend. Iris's backend does all of Iris's own work - understands
   the request, chooses the tool, remembers the user (AI Ecosystem), talks with other agents. Iris's
   UI is only the shell of the talk and may differ from one installation to another (a window, a
   web page, a chat); the backend is the same.
2. **Every tool** is built the same way: a UI and its backend, the UI may differ. A tool installs
   in variants - **browser or desktop** (a UI with its backend) or **daemon** (the backend alone) -
   and Iris may advise one. What Iris offers depends on whether there is a UI and **whether Iris
   reaches the tool's backend** (installed in the same space):
   - **a UI, and Iris reaches the backend** (the one running under the browser or desktop UI):
     Iris **asks** - "will you do it yourself, or shall I do it for you?" - and either tells the
     user what to open and do in the UI, or does the operations itself through the backend;
   - **a UI, and Iris does not reach the backend**: one way only - Iris tells the user what to open
     in the UI and what to do there;
   - **the backend alone, no UI** (daemon): one way only - Iris does the operations itself. Usually
     the user's choice made beforehand: the tool was installed as a daemon for Iris to work with.

Iris understands the request, chooses the tool and how to call it, calls it - or tells the user
which window to open and what to press - and explains what came back. The work itself is done by
the tools. A tool's backend does not know who calls it - its UI, Iris or another agent - and does
not care where its settings come from.

## Tools are not built in

Iris knows no tool in advance. The user starts Iris and says: "I installed Mosaic Remover, here is
its schema, remember how to use it". Iris writes the schema to AI Ecosystem and from then on works
with the tool through it - its commands, its settings, its events. So every tool's backend
describes itself in a machine-readable schema (Mosaic Remover: `MOSAIC-219`), and AI Ecosystem takes
a schema "on the fly" (`ECOSYSTEM-2`, `IRIS-82`).

## Preferences

A UI and a backend alone have settings, no preferences. Preferences come with the agent: said in a
talk ("new files always next to the video"), kept by Iris in AI Ecosystem, given to a tool's backend
as plain settings. What a program lets the user change itself is its local setting and wins there.
A UI that connects to Iris may ask or write preferences through Iris instead of going to the
backend directly.

## What the user asks

From the video problems Pavel brought to Claude (2026-10-11 … 2026-10-16) - real cases, not guesses.
Iris knows which tool answers each; where none does yet, it says so (below).

**About a folder** - "for now my main tasks for these requests are" (Pavel, 2026-10-16):

| # | The request | What it needs |
|---|---|---|
| 1 | Which files in this folder have mosaic? | frames spread over each video: *mosaic / none / not sure*, with the times |
| 2 | …and for which of them demosaic is already done? | a `<name>.demosaic.*` beside it or in the results folder; whole, the same length |
| 3 | Add this file / folder to demosaic | Mosaic Remover's backend takes the files (its lists over HTTP since `MOSAIC-16`) |
| 4 | Which files are of low resolution? | the heights of the folder's videos, against a threshold |
| 5 | Can this file be improved - does it make sense? | height, bit rate per pixel, anime or filmed, the measured speed of each way; a trial piece through *Improve picture* |

**About one file:**

| The question | Real case | How it was answered |
|---|---|---|
| Are these the same film? Which version is better? | `Nevestka4.mp4` against `vid_720p (7).mp4`; `HUBLK-003` 720p against 1080p | length, size, fps, bit rate; duplicated frames; frames from the same moments side by side |
| Is the file whole? Where is it damaged? Fix it | `Nevestka4.mp4`: no frame for ~3 minutes around 2:23:09 | Mosaic Remover's Diagnostic: check, cut, freeze (also `mosaic-remover check` / `repair`) |
| Why does it stutter / play badly / look wrong? | VLC at 4x on `.demosaic` files; `ProdalDoch.mp4` reports 90000 fps; blank thumbnails | codec, B-frames, key frame interval, variable frame rate - with what each means ([knowledge.md](../knowledge.md)) |
| Why did a job take so long? | `Clinika(eng).mp4`: "40% after 2 hours" | length × fps against the engine's speed |
| Cut a file in two at a time | `Dochki2` at 55:41 | ffmpeg without re-encoding, at the nearest key frame |
| Which subtitles fit, and name them | `HUBLK-003` | match by length / timing; rename; a copy for `.demosaic` |
| Is the result of Lada right? | every Lada job | whole, the same length as the source, sound present |

## A request it cannot handle

Iris is one of Pavel's agents, and they are **specialised**: its subject is **video files** - what is
in them, what is wrong with them, what to do with them; subtitles from speech stay VideoToSRT's,
model access AI Gateway's. A request outside it is not attempted.

Inside it, a request no known tool answers: Iris says so plainly and **passes it on to the
developers** - what was asked, in Pavel's words, and what would have to be added. A named gap beats
a 404 (*advertise gaps, not just capabilities*, `Projects Hub\docs\ecosystem-strategy.md`); the gaps
gather in one list, and that list is the plan of what to build next.

## Connections

- **AI Gateway - the models**, for Iris's own understanding of a request (the local model on the
  8 GB card for ordinary cases, a cloud one for hard ones) and for the tools.
- **AI Ecosystem - the memory**: the tools' schemas, the user's preferences, Iris's notes. Until its
  API exists (`ECOSYSTEM-2`) the notes live in this repository ([knowledge.md](../knowledge.md)).
  **Exactly one Ecosystem per Iris** (Pavel, 2026-11-13: one incarnation, one Ecosystem; work apart
  from personal is a second incarnation, not a second Ecosystem for this one). What Iris learns
  about its user: [design/learning-per-user.md](../design/learning-per-user.md).
- **Other Iris nodes** - an Iris may pass a request on in its own name and bring the answer back.

A capability has these states, and Iris says which:

| State | What Iris says |
|---|---|
| works | does it, through the tool |
| works through another Iris | passes the request on in its own name |
| not installed | which tool to install and where from |
| not implemented | a request for the developers |

## Not decided

- How a tool's schema looks (`MOSAIC-219`) and how AI Ecosystem keeps it (`ECOSYSTEM-3`).
- The code built inside Iris on 2026-10-24 (branches `feature/check-repair`, `feature/diagnose-compare`,
  `feature/folder-tasks`, not landed): check and repair went to Mosaic Remover's engine (`MOSAIC-153`);
  what of diagnose, compare and the folder questions becomes a tool, and whose, is open.
