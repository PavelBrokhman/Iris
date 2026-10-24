# Iris

Pavel's video agent, and the umbrella over his video projects (Mosaic Remover, VideoToSRT,
VLC Playlists).

The agent looks at video files and says what is wrong with them and what to do: is a file whole,
which of two versions is better, why a player stutters, where to cut, how to repair a damaged
place, which subtitles fit. Most of the work is ordinary code around ffmpeg; a language model
(through AI Gateway) only understands the question, chooses the tools and explains the result.

- What it does and how it is built: [docs/architecture/spec.md](docs/architecture/spec.md)
- What is known about video: [docs/knowledge.md](docs/knowledge.md)
- Where it stands: [docs/STATUS.md](docs/STATUS.md)
- Decisions and their reasons: [docs/decisions/](docs/decisions/README.md)

Not built yet - documents only (2026-10-16).
