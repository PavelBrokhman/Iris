# check and repair: moved from Mosaic Remover, not shared

Proposed by Claude 2026-10-24 (`IRIS-11`), **waiting for Pavel's yes**.

**Iris does not reference Mosaic Remover's core.** The recipe part of `MosaicRemover.Core\Diagnostic`
- where a damaged place is, how a hole in the frames gives its edges, what is kept and what is cut,
which repair suits - is moved into `Iris.Core` and rewritten around one interface, `IVideoTools`:
everything that runs ffmpeg or ffprobe. The recipes only get data from it (frame times, key frames,
the decoder's complaints) and are tested on data, without ffmpeg.

**Why not a reference or a package:** the spec's direction is that the ffmpeg runs become Gateway
capabilities and the recipes live in Iris. Mosaic Remover's core holds both mixed (a class runs
ffmpeg and reads its lines in one method), and with a reference Iris would carry Lada, DeepMosaics
and the settings of a window it never uses. Mosaic Remover keeps its own copy until its piece is
rebuilt (`MOSAIC-21`, `MOSAIC-19`); then it can call Iris instead.

**What came along as it was:** the numbers (3 s between complaints = separate places, a hole of
0.5 s or 5 frames, 10 s before and 30 s after a place), the rule that a cut stops with an error when
the end of a place is not found, the cut ending at the key frame before the hole, the length check
of the joined video, freeze at CRF 16 (x264) or CQ 19 (NVENC).

**Where the files go - also waiting for Pavel:** the result next to the video
(`<name>.cut.mp4`, `<name>.frozen.mp4`), as Mosaic Remover does; the pieces of a cut in a `work`
folder next to `iris.exe` (Pavel's default for a program's data, 2026-10-13), deleted when the
repair ends, failed or not.
