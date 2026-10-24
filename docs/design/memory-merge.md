# Merging the Claude memory of the video projects - a draft

`IRIS-9`, 2026-10-24. A proposal for Pavel, kept as written. **The outcome is in `IRIS-9`:** what
this draft calls "the top level" went to Claude Hub (`Bootstraps/max-mode/user-profile.md`,
`tooling-notes.md`, `shared/jira/README.md`), not to Iris's memory - preferences hold for every
project, and a project's memory is kept as small as possible.

On 2026-10-24 the memory of Mosaic Remover, VideoToSRT and VLC Playlists was copied as it was into
Iris's memory, a folder per project (Pavel: copy as is, decide later, then merge). The originals
stay in the projects' own memory and are not touched by this draft.

## The shape after the merge

- **The top level** - what holds for all video work with Pavel, whichever project: how he works,
  how to talk to him, facts of this machine.
- **A folder per project** - only what matters when working on that project.
- **Nothing that `docs/` or Jira already holds.** A memory that repeats a decision or a task goes;
  if it holds the story of *why* that the decision file lacks, the story moves into the file first.

## File by file

### To the top level (merged where marked)

| New | From | Why |
|---|---|---|
| `pavel.md` (user) | mosaic `user-working-style`, videotosrt `user-git-is-new` | one profile: Russian in translit → answer in Cyrillic, **everything written to him** (the in-between lines too); C#; git is new, explained through TFVC; Pavel judges picture quality of the explicit test videos, Claude only technical facts; quality over file size; never kill a running job for a build |
| `read-the-mode.md` (feedback) | mosaic `question-means-answer-only`, videotosrt `feedback-no-work-without-agreement`, Iris `work-through-backlog` | the three say one thing from three sides and partly contradict each other ("a question gets words only" against "autonomy by default"). Merged: a question or thinking aloud gets words; a design remark is discussed, not obeyed; a decided process is worked through without asking - story by story to In Review |
| `one-question-at-a-time.md` | mosaic, as is | holds everywhere |
| `discuss-where-data-lives.md` | mosaic, as is | holds everywhere |
| `measure-before-concluding.md` | videotosrt, as is | holds everywhere; the examples come from VideoToSRT, the rule does not |
| `take-from-named-file-verbatim.md` | videotosrt, as is | holds everywhere |
| `csharp-interfaces.md` | mosaic `csharp-no-static-swappable-classes`, shortened | the rule itself is now in [decisions/rules-of-code.md](../decisions/rules-of-code.md); the memory keeps the episode and "say so if the code around is static, do not copy it silently" |
| `powershell-utf8-mojibake.md` | mosaic, as is | one line of it is in rules-of-code; the memory keeps the commands that do it right |
| `gateway-shared-clone.md` | videotosrt `shared-clone-branch-hazard`, as is | Iris will work in the Gateway too |
| `this-machine.md` (reference) | videotosrt `timeout-and-deployment-facts` (the GPU and models half), `wpf-compile-check` | RTX 2070 SUPER 8 GB, which models fit, `dotnet build` works on the host - Iris's spec counts on the local model |

### Stays in its project's folder

- **VideoToSRT:** `host-pre-existing-test-failures`, `longform-transcription-collapse`,
  `transcription-quality-plan`, `ocr-junk-text-not-filtered`, `translation-runaway-no-output-cap`,
  `runtime-instance-separate-from-dev` (D:\Prod), `gateway-whisper-cuda-on-host`,
  `timeout-and-deployment-facts` (the timeouts half), `design-gateway-as-library`.
- **VLC Playlists:** both - `project-layout-root-solution`, `vlc-labels-lua-sd`.
- **Mosaic Remover:** nothing left (below).

### Dropped from Iris's memory

| File | Why |
|---|---|
| mosaic `untested-close-tray-queue` ("RAISE AT SESSION START") | every item is in Jira now (`MOSAIC-2`, `-3`, `-18`, `-5`, `-6`, `-19`); "no remote" is out of date. **Except one:** fixing Lada itself (`D:\Projects\lada`, three spots: the 10-bad-frames limit, exit code 0 on failure, "?" from `r_frame_rate` 90000) has no story - it gets one in `MOSAIC` before the memory goes |
| mosaic `user-ai-ecosystem` | `CLAUDE.md` names the Gateway and the hub with their paths |
| videotosrt `sandbox-shared-convention` | replaced by [decisions/scratch-and-samples.md](../decisions/scratch-and-samples.md) - always on now, not an opt-in |
| videotosrt `host-finalize-branch-state` | a resume point of 2026-06; `DEV-host` was closed on 2026-09-14 (`runtime-instance-separate-from-dev`); two of its items are not in the `VIDEOTOSRT` space (checked 2026-10-24) - the seek polishing in `FileConfigWindow` and the OCR click-through (browse → configure → queue → SRT); they get stories there before the memory goes |
| videotosrt `ai-text-to-movie-exploration` | not video tooling - its work lives in Claude Hub; it stays in VideoToSRT's own memory |

## Not decided here

- Whether the projects' own memories get the top-level files too (a session started in a project
  folder does not read Iris's memory). Left alone for now - the originals stay as they are.
