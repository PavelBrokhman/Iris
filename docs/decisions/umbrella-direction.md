# The umbrella's direction

Decided 2026-10-24 (Pavel).

The target is Claude Hub's shape: tools through AI Gateway, data through AI Ecosystem, the UI apart
from the core. Reached piece by piece, never all at once: take one piece (of Mosaic Remover first),
rebuild it, check it works, then the next. Meanwhile each project keeps working as it is - Mosaic
Remover with its own ffmpeg. Each project's pieces are its backlog in Jira ([jira.md](jira.md)).

**Where the Gateway stands (2026-10-24).** It already runs ffmpeg (`/probe`, `/extract-audio`,
`/detect-silence`, `/extract-frames`, `/grab-frame` - made for VideoToSRT). For Iris `/probe` says
too little and replaces an unknown fps with 25, and there is nothing for `check` / `cut` / `freeze`.

**Not decided:** while the Gateway lacks a command, may Iris run ffmpeg itself for a while
(`IRIS-11`)?
