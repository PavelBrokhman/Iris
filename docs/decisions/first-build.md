# The first build of Iris: what it is made of, and in what order

Decided 2026-11-14 (Pavel). Kind: **project**.

Until then nothing of the agent was built (`STATUS.md`); the design was in `architecture/spec.md`,
`design/learning-per-user.md` and `decisions/tools-by-description.md`.

## Three choices

1. **Iris's backend is written in Python.** It holds no model - every model call goes through AI
   Gateway over HTTP - so the language is not about AI; Python is the language of the Gateway,
   VideoToSRT and Projects Hub's shared clients. Go was asked about and not taken: a third language with
   nothing gained here. C# was the other candidate (Mosaic Remover's).
2. **AI Ecosystem is the service in its own repository**, `D:\Projects\AI Ecosystem` - already
   decided in Projects Hub's `docs/decisions/ai-ecosystem-as-a-service.md`: the service owns the code
   that acts on its data, the career data is one registered instance's scope, the storage is the
   service's own choice. Iris's data is one more scope, one per incarnation.
3. **Iris's first UI is a chat in the browser**, a page served by Iris's backend: nothing to
   install, reachable from a phone; another UI connects to the same backend later.

## The order

| # | Step | Blocks |
|---|---|---|
| 1 | The three choices above | everything |
| 2 | Jira: close the IRIS items of 2026-10-24 that the rewritten spec made obsolete | - |
| 3 | AI Ecosystem's minimal API (`ECOSYSTEM-2`): facts with owner, source and time, append-only; documents; the shared contract tests in its repository | 4 |
| 4 | Iris's skeleton on the four points of the foundation (`design/learning-per-user.md`): the backend takes a request, understands it through a Gateway model, writes the fact to the Ecosystem; "what bears on this question" is one interface that returns everything; the chat page | 5 |
| 5 | The first tool: Mosaic Remover, whose backend gives its schema (`MOSAIC-219`). Iris keeps the schema (`IRIS-82`) and calls the backend itself - the "backend alone" case | - |
| 6 | The eight steps of `tools-by-description.md` on paper (`IRIS-94`), on VideoToSRT's description (`VIDEOTOSRT-183`) and the OCR host's entry in the Gateway's list (`GATEWAY-41`, `GATEWAY-46`) - after PaddleOCR and Whisper are services of their own | - |
| 7 | Before the first release to others: every learning stage switching itself on, and the synthetic-history tests | release |
