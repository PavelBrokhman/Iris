# Iris — what it does

Written 2026-10-16 from the video problems Pavel actually brought to Claude in the Mosaic Remover
sessions (2026-10-11 … 2026-10-16) and from VideoToSRT's notes. Each row is a real case, not a
guess at what might be useful.

## The main tasks (Pavel, 2026-10-16)

"For now my main tasks for these requests are" - questions about a **folder** of videos, in his
words, "something like this":

| # | The request | What it needs | Open |
|---|---|---|---|
| 1 | Which files in this folder have mosaic? | `scan`: look at frames spread over each video (not every frame) and say *mosaic / none / not sure*, with the times where it was seen | how to find mosaic reliably: Mosaic Remover's grid detector was never tried on real video; Lada's own detection network (`lada_mosaic_detection_model_v4_*.pt` in its build) is the strong one but needs its Python |
| 2 | …and for which of them demosaic is already done? | for each file with mosaic: is there a `<name>.demosaic.*` (or `.dm-*`) beside it or in the results folder; is it whole and the same length; optionally `scan` the result too - mosaic left? | where results may lie besides next to the source |
| 3 | Add this file / folder to demosaic | hand the files to Mosaic Remover: those with mosaic and no result yet | **how to hand over**: Mosaic Remover's queue is in `data\settings.json` of the copy that runs (Release), written by the window while it is open - writing it from outside would clash. Options: the command line `mosaic-remover process`, a hand-over file the window picks up, or opening the window with the files (it already takes files on its command line) |
| 4 | Which files are of low resolution? | `probe` the folder, sort by height; "low" = below a threshold (720p?) | the threshold |
| 5 | Can this file be improved - does it make sense? | `diagnose` + advice: height, bit rate per pixel, anime or filmed, whether it is a `.demosaic` result; the measured speed of each way (Real-ESRGAN plus: tens of hours for a 2-hour film); offer a trial piece through Mosaic Remover's *Improve picture* | the rules of the advice - from Pavel's own judgement on trial pieces |

These come first; the tasks below are the earlier one-file questions. The "Open" column is for the
developers, not a condition: these are ideas, and reliability is not the point yet (Pavel).

## Its subject, and what it does with a request it cannot handle

Iris is one of Pavel's agents, and they are **specialised**: each owns one subject area and
delegates everything else - unlike a general assistant, it does not try to solve every problem
(the ecosystem's rule, `claude-hub\docs\ecosystem-strategy.md`). Iris's subject is **video files**:
what is in them, what is wrong with them, what to do with them. Subtitles from speech stay
VideoToSRT's, model access AI Gateway's.

A request inside its subject that it cannot handle yet: it says so plainly - "I have not done this
before" - and **passes it on to the developers**: what was asked, in Pavel's words, and what would
have to be added (a new tool, a new rule, a missing piece of knowledge). This is the ecosystem's
*advertise gaps, not just capabilities*: a capability marked `not_implemented` with a `why_not`
tells a peer not to ask and a human what to build - a named gap beats a 404. The gaps gather in one
list, and that list is the plan of what to build next. A request outside its subject is not
attempted at all.

This is what makes it one node of the **distributed network of agents** Pavel is aiming at: every
agent says honestly what it can do (and what it cannot yet), and whoever asks chooses one of them
or several. So Iris publishes its capabilities - each with its status - through the ecosystem's
interface between agents, rather than only answering questions put to it directly.

**Two connections, and Iris itself stays thin** (Pavel, 2026-10-16). Iris owns the understanding of
video - the recipe: what to look at, what it means, what to do. It does not own the network, the
tools of a machine, or the storage:

- **AI Gateway - the network and the hands.** What a machine can do (ffmpeg, Lada, Video2X,
  VideoToSRT...) is what the Gateway on that machine offers, and reaching another machine is the
  Gateway's business too (the Mycel direction). The Gateway already opens onto media by path:
  `/probe`, `/extract-audio`, `/detect-silence`, `/extract-frames`, `/grab-frame` - "the one place
  that should know what this machine can do". Iris asks for capabilities; it does not run its own
  ffmpeg. (`/probe` today returns only size, length and fps; Iris's diagnoses need codec, B-frames,
  key frames, variable frame rate - that is an extension of the Gateway, not a copy inside Iris.)
- **AI Ecosystem - the data.** Iris's notes (what is known about video, Pavel's folders, the
  decisions taken, the list of gaps, the results of scanning folders) are data of a registered
  Iris instance in the Ecosystem, like Career Data belongs to a Tiler instance. Until the
  Ecosystem's service contract exists they live as files in this repository (`docs/knowledge.md`).

**Connectors.** An Iris works with **at least one Gateway and one Ecosystem**, and may have more of
each - several machines' Gateways, several Ecosystems - like one agent instance may connect to
several Ecosystems.

**Or it is a representative.** An Iris with no Gateway and no Ecosystem of its own is still a
useful node: it keeps a list of other Iris nodes and **passes a request on in its own name**, as
their representative, bringing the answer back. The one who asked deals with this Iris only.

So **an empty Iris** - just installed, nothing connected - only says what it needs: "connect me to
a Gateway and an Ecosystem, or give me other Iris nodes to speak for". Once connected, a capability
has these states:

| State | Meaning | What Iris says |
|---|---|---|
| works | one of its Gateways offers the tool - on this machine or another | does it |
| works through another Iris | an Iris on its list can do it | passes the request on in its own name, brings the answer back |
| not installed | nobody reachable offers it; Iris knows how | what to install and where from |
| not implemented | nobody has built it yet | a request for the developers |

Which machine or node does the work is chosen by the one who asks, from what the nodes say about
themselves. A video file is large, so whether it travels or is read where it lies is part of that
choice - and the Gateway's paths only mean something while both sides see the same disk.
## The one-file tasks

| Tool | The question | Real case | How it was answered |
|---|---|---|---|
| `compare` | Are these the same film? Which version is better? | `Nevestka4.mp4` against `vid_720p (7).mp4` (same film, 2:55:15; the new one 30 real fps, no duplicated frames, sharper - it replaced the old one); `HUBLK-003` 720p against 1080p | length, size, fps, bit rate; duplicated frames (`mpdecimate`); frames from the same moments side by side |
| `check` | Is the file whole? Where is it damaged? | `Nevestka4.mp4`: no frame decodes for ~3 minutes around 2:23:09 - Lada stopped there and wrote nothing | decode the whole file with ffmpeg, list the places with errors (rough to tens of seconds) |
| `repair` | Fix the damaged place | the same file | *cut* (no re-encoding, from the key frame before to the key frame after) or *freeze* (encode again, the last good frame fills the hole) |
| `diagnose` | Why does it stutter / play badly / look wrong? | VLC at 4x on `.demosaic` files (HEVC with B-frames, key frame every 2.5 s, against H.264 without B-frames in the original); `ProdalDoch.mp4` reports 90000 fps (variable frame rate) so Lada shows "?"; blank thumbnails in Explorer (a key frame only every ~10 s) | codec, profile, B-frames, key frame interval, constant or variable frame rate, bit rate - with what each means for players and tools |
| `diagnose` | Why did a job take so long? | `Clinika(eng).mp4`: "40% after 2 hours" - 3 h 09 min of 1280×720, about 5 hours in all | length × fps against the engine's speed; how much of the video itself is done |
| `split` | Cut a file in two at a time | `Dochki2` at 55:41 | ffmpeg without re-encoding, at the nearest key frame |
| `subs` | Which subtitles fit, and name them | `HUBLK-003`: subtitles of the 720p version moved to the 1080p one, renamed, copies with `.demosaic` in the name for the result | match by length / timing; rename; copy for `.demosaic` (the timing is not checked in a player yet) |
| `check` + `compare` | Is the result of Lada right? | every Lada job | whole, the same length as the source, sound present |
| `diagnose` | Is it worth improving the picture? | a 360p `.demosaic` video | size, bit rate, a trial piece through Mosaic Remover's *Improve picture* |

## How it is built

- **The recipes are ordinary code** that turn what the Gateway returns into conclusions, not raw
  numbers: "HEVC, 2 B-frames, a key frame every 2.5 s - hard for a player to seek fast" rather than
  an ffprobe dump. This is most of the work and the reliable part. C# for now (Pavel's choice for
  himself); the boundaries are HTTP / JSON and command lines, so a public version can be written in
  whatever suits it without changing the other side.
- **A language model only joins them up**: understands the question, picks the tools, explains
  the answer. Through AI Gateway - the local model (Qwen on the 8 GB card) for ordinary cases,
  a cloud model for hard ones. Whatever has no rule behind it is passed on honestly, not guessed.
- **Its notes are its memory**: Pavel's folders, the decisions taken (such as the `.demosaic`
  copy of subtitles), what was found before - [knowledge.md](../knowledge.md).
- **What exists already**: Mosaic Remover's core (`Ffmpeg.Probe`, running external programs
  through `IExternalTool`) and its draft `MosaicRemover.Check` - `check` and `repair cut|freeze`
  with interfaces and a factory, tried on one damaged file only. Its recipe part (where the hole
  is, where to cut, which repair) moves here; the ffmpeg runs it makes become Gateway capabilities.

## Not decided

- One program with commands, or a service like the other agents. Start with a command-line
  program; whether it becomes a service is decided when there is something to serve.
- How the Mosaic Remover core is shared (a project reference, a copy, a package).
