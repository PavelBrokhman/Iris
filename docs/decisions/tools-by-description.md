# How Iris gets a tool: by its description

Decided 2026-11-11 (Pavel), on the example "I need the hardcoded subtitles of a video as an SRT".
Kind: **project** - Iris's own way of working; the Gateway's side of it is in AI-Gateway's
`docs/design/connector-and-engine-hosts.md`.

**Not now.** Recorded to keep the idea in step (Pavel); first PaddleOCR and Whisper become separate
services like Ollama, installation and the connector side come after. The work: VIDEOTOSRT-183,
GATEWAY-46, IRIS-94.

## The way it goes

1. Iris is installed and **empty**: no tools.
2. The user asks for something in its subject: "I need the OCR subtitles out of a video".
3. Iris says plainly that it has **no tool for that**.
4. The user points it to one: "I heard VideoToSRT does it" - and hands over an installation
   document, a link to GitHub, a link to such a document, or a link to **another Iris**.
5. **A document** (or a link to one) carries what Iris can read: how to install the tool, whether
   it needs AI Gateway, and what exactly it takes from the Gateway - which contracts or which
   version of the Gateway, which parameters it needs, what it passes itself, what it returns.
   It also says **in which variants the tool installs** - browser or desktop (a UI with its
   backend) and daemon (the backend alone) - and **how its backend is reached**. Iris can then
   advise a variant and offer what fits: a UI and the backend reached - it asks "will you do it
   yourself, or shall I do it for you?"; a UI and the backend not reached - it guides the user
   through the UI; the backend alone - it does the work itself (Pavel, 2026-11-14;
   `architecture/spec.md`, Three parts).
6. **A link to another Iris**: Iris asks the user what is wanted from it -
   - the same installations as that Iris has, or
   - only the tool's installation and the models installed there for it, or
   - that Iris does the work (Iris passes the request on in its own name, as in `architecture/spec.md`).
7. What the tool needs and is not there yet (here: AI Gateway and AI Ecosystem) - Iris gives the
   instructions to install it.
8. When all is in place, the **Gateway lists the models it can work with, by category, and knows
   how to install them**. Iris reads the list, matches it against the parameters the tool needs,
   and gives the user the instructions to install a model - or asks whether the user wants to give
   credentials for an online one (those are in the Gateway's list too).

For two of Pavel's own projects the match will usually be there; for a third-party tool it needs
thought (below).

## Iris gives instructions; it does not install

The user installs. Iris's part is to **warn beforehand what will be needed**: a tool's description
says that it works with the Gateway and through which contracts (or which Gateway version), and that
it will need models from the Gateway - so Iris can say "after this install there will be more steps:
the Gateway, then a model for OCR".

## What "the experience of another Iris" is

The user chooses: **all the installations** of that Iris, or **only the tool's installation and the
models installed there for it**.

## Claude's notes, not yet decided

Raised in the same talk; Pavel answered the three questions above, not these.

- **The Gateway knows the machine.** A recommendation depends on the card, its memory (8 GB here,
  shared by Ollama, Whisper and OCR), the driver, free disk, the OS. The Gateway is the hands on that
  machine, so "tell me about this machine" would be one of its capabilities.
- **A tool's needs apart from Iris's.** The tool needs the Gateway and named capabilities; the
  Ecosystem is what *Iris* needs to remember. A backend does not know the Ecosystem
  (`what-iris-is`), so a tool's description should not require it.
- **One vocabulary of capabilities** (`ocr.detect`, `ocr.recognize` with a language, `chat`,
  `media.probe`, ...) on both sides, the tool's description and the Gateway's list, or nothing can be
  matched. For a third-party tool: Iris drafts the description from its README, marks it unchecked,
  the user confirms, a trial run checks it.
- **A chain with versions**: tool -> Gateway contract -> engine host -> CUDA -> driver. Iris walks it
  like a package manager.
- **Trust.** A document from a link, or another Iris's experience, says "install this from there".
  Iris shows what and from where before the user does it, the more so when the source is not the
  user's own.
- **Keys go to the Gateway directly**, never through Iris or its language model.

## First step

Write two descriptions by hand: **VideoToSRT's** (what it needs from the Gateway, its parameters,
its results) and **the OCR host's entry in the Gateway's list** (GATEWAY-41). The format is found on
these two real cases, then steps 1-8 are walked through on paper.

`architecture/spec.md` (the "not installed" state: "what to install and where from") is to be
rewritten to this.
