# Scratch and video samples

Decided 2026-10-24 (Pavel).

**Scratch, PoCs and tools** go by Claude Hub's routing (`claude-hub/tmp/<project>/`, `sandbox/`,
`shared/`), not to the Windows temp folder.

**Why:** a working file in a session's temporary folder is lost with it - Mosaic Remover's
`crlf.py` lived there until 2026-10-24.

**Video samples** - never into a git: one folder `D:\Video Samples\`, a folder per case with its own
`README`. Lada's samples (`D:\Projects\lada\samples\`) move there (Pavel); then the references in
Mosaic Remover's `status.md` and in the memory are fixed (`IRIS-8`).

**Why:** samples, cut pieces and frames are large, and the hub's git is not the place for them.
