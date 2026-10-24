# What is known about video

Facts found while working on the video projects, with where and when they came from, and whether
they were checked. A fact is moved here when it concerns more than one project or video in general;
what belongs to one project stays in that project.

## Players and seeking

- **Explorer thumbnails come from a key frame.** A video with a key frame only every ~8-10 s gets a
  blank thumbnail (the very first frame); one every 2 s gets a picture. The codec does not matter.
  *Checked by Pavel, 2026-10-12* (Mosaic Remover, `samples\thumb-test`). Mosaic Remover now writes a
  key frame at least every 2 s / 60 frames.
- **VLC stutters at 4x and on ← / → with Lada's results** (`Negr.demosaic.mp4`: HEVC Main, 2
  B-frames, key frame every 2.5 s; the original: H.264 High, no B-frames, key frame every 3 s; same
  sound). Likely cause: HEVC with B-frames decoded on the processor. **Not checked** - the next step
  is VLC's hardware decoding (Tools → Preferences → Input/Codecs → Direct3D11) and the processor
  load. Lada's default preset `hevc-nvidia-gpu-hq` makes such files; an H.264 preset would avoid
  it. (Mosaic Remover, 2026-10-15)
- **A video can report a frame rate of 90000** (`r_frame_rate` of a variable-frame-rate file,
  `ProdalDoch.mp4`, really ~24). Tools that trust it show nonsense; `avg_frame_rate` is right.
  (Mosaic Remover / Lada, 2026-10-14)

## Damaged files

- **No frame decodes for minutes** (`Nevestka4.mp4`, ~2:23:09-2:26): Lada gives up after 10 bad
  frames in a row, prints "Error on export" and still ends with exit code 0, writing nothing. A
  whole copy of the film from another source replaced it. Sample: `D:\Projects\lada\samples\
  damaged-h264-no-frames.mp4`. (Mosaic Remover, 2026-10-14)
- **Damaged sound stops ffmpeg 8.1.1 half way** (exit -22), so a check decodes the picture only.
  (Mosaic Remover, 2026-10-14)

## Cutting and joining

- **Copied sound and a cut**: cutting a part with `-ss` before `-i` and the sound copied, mp4 keeps
  picture and sound in step (an edit list); **mkv shifted the sound by about a second** in a test.
  (Mosaic Remover, 2026-10-16, checked with ffprobe)

## Tools

- **Lada** (v0.11.0, `tools\lada` of Mosaic Remover): self-contained (Python, PyTorch, ffmpeg and
  the weights inside). Reports progress in bursts - its frame counter stands, then jumps. The
  weights are on Hugging Face (`ladaapp/lada`), not in the source repository - keep the packaged
  build. (2026-10-14/15)
- **Video2X 6.4.0** (`tools\video2x` of Mosaic Remover, AGPL-3.0): takes the video whole, no
  pictures on the disk. With Real-ESRGAN / Real-CUGAN it **crashes on exit (0xC0000005) after the
  file is complete** - the file decodes whole. Real-ESRGAN plus on 640×360 → 2560×1440: 0.44 fps on
  the RTX 2070 SUPER (card partly shared) - a 2-hour film would take tens of hours. (2026-10-16)
- **Grid mosaic detection** takes flat squares with sharp edges for mosaic (ffmpeg's colour-bar test
  picture), and a mosaic over a flat area is invisible - test pictures need fine detail and grain.
  (Mosaic Remover tests, 2026-10-16)

## Subtitles

- **Naming**: subtitles of a film go next to it as `<name>.en.srt` / `<name>.ja.srt`, plus copies
  as `<name>.demosaic.en.srt` for the result of Lada, so players pick them up for both.
  (`HUBLK-003`, 2026-10-15; timing not checked in a player)
