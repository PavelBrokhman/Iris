# Scratch and video samples

Decided 2026-10-24 (Pavel).

**Scratch, PoCs and tools** go by Claude Hub's routing (`Projects Hub/tmp/<project>/`, `sandbox/`,
`shared/`), not to the Windows temp folder.

**Why:** a working file in a session's temporary folder is lost with it - Mosaic Remover's
`crlf.py` lived there until 2026-10-24.

**Video samples** - one folder in Projects Hub, `D:\Projects\Projects Hub\Video\Samples\`, a folder
per case with its own `README`; the READMEs are in Projects Hub's git, the media never. Nothing is
kept outside `D:\Projects` - what is kept goes into a project or into Projects Hub. Lada's samples
(`D:\Projects\lada\samples\`) moved to the first place on 2026-10-24 (`IRIS-8`), the references in
Mosaic Remover's docs and in the memory followed.

*Changed 2026-10-24:* first written as `D:\Video Samples\`; Pavel: `D:\Projects-Shared\Video\Samples\`.
*Changed 2026-11-25:* moved into Projects Hub (Pavel: nothing outside `D:\Projects`), with VideoToSRT's
test clips (`test-clips\`, `Shared\test-clips\`), which had lived inside that project.
Moving the samples was first left to Pavel; he said Claude may do it.

**Why:** samples, cut pieces and frames are large, and the hub's git is not the place for them.
