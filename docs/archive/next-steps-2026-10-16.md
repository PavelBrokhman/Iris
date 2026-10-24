# The "Next" list of STATUS.md, as it was until 2026-10-24

Archived on 2026-10-24 when the open work moved to Jira. Each item is now a story there:

| Item | Jira |
|---|---|
| 1. Merge the Claude memory | `IRIS-9` |
| 2. The main tasks first | `IRIS-10` |
| 3. The Gateway first | `GATEWAY-2` |
| 4. First recipes | `IRIS-11` |
| 5. Then `diagnose` and `compare` | `IRIS-12` |
| 6. A remote repository (GitHub) | done 2026-10-24 (`IRIS-5`) |

The text as it stood:

1. **Merge the Claude memory** of Mosaic Remover and VideoToSRT (and VLC Playlists) into the memory
   of this folder. A draft is shown to Pavel first: what goes in, what is dropped (state that is
   only a date's snapshot), what stays in a project.
   *2026-10-24:* copied as it was, not merged (Pavel: copy as is, decide later which way is better,
   then merge) - `C:\Users\brokh\.claude\projects\D--Projects-Iris\memory\`, one folder per project,
   one `MEMORY.md` over them; the originals untouched. Mosaic Remover, run without the hub's
   bootstrap, got a `CLAUDE.md` with its working rules and their reasons the same day. Known
   differences to settle: branch names (`feature/` there, `DEV-` in the hub); Mosaic's memory note
   "RAISE AT SESSION START" of 2026-10-13 is partly out of date.
2. **The main tasks first** (Pavel, 2026-10-16 - see "The main tasks" in
   [spec.md](../architecture/spec.md)): questions about a folder - which files have mosaic, which are
   already done, add to demosaic, which are of low resolution, is improving worth it. Ideas -
   reliability is not the point yet. What Iris cannot do it says so and passes on to the developers
   as a request (see spec.md).
3. **The Gateway first** (Iris owns the recipe, the Gateway the hands - spec.md): extend its
   `/probe` with codec, profile, B-frames, key frame interval, variable frame rate, bit rate,
   sound - in `D:\Projects\AI-Gateway`, on its own branch.
4. **First recipes**: move the recipe part of `MosaicRemover.Check` (`check`, `repair cut|freeze`)
   here; fix its weak spot first - when the end of the damage is not found, `cut` must stop with an
   error instead of cutting to the end of the file. Decide how the Mosaic Remover core is shared.
5. Then `diagnose` and `compare`.
