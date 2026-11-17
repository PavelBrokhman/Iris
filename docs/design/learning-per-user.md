# Learning per user: what Iris remembers about its user, and when to lay the ground

Talked through 2026-11-13 (Pavel and Claude). **Not decided** - Pavel's words: he understands how
it works, what he is not sure of is *at what moment the foundation is laid*, with one user today
(himself) and, he hopes, a thousand later. Recorded so the next talk starts from here.

## Not training

Nothing here changes a model (`Projects Hub/docs/ecosystem-strategy.md` §6: "the instinct is right,
the word is not"). One model for everyone; what differs per user is **what reaches the prompt**:

1. Iris takes from AI Ecosystem what it knows about this user and what bears on the question;
2. puts it into the prompt with the question and what the tools measured;
3. answers;
4. when the user corrects or chooses, writes that back.

The model stays the same; every Iris is different because its memory is.

## What Iris learns about its user (on Pavel's own cases)

| Kind | Example | Comes from |
|---|---|---|
| Preference | subtitles go beside the video, plus a copy named `.demosaic` | said once |
| Correction | "that is not mosaic, it is noise" | the user corrected an answer |
| The user's world | "everything in that folder has mosaic", "720p is low for me" | talks, folder scans |
| Experience | "Nevestka4 was repaired by a cut, it worked" | past cases and how they ended |
| The user's words | "demosaic" = a Lada job | talks |

## Rules (paid for on Tiler, `ecosystem-strategy.md` §4)

- **Only actions teach.** Silence or an accepted default is not agreement; learning from its own
  unopposed answers, Iris drifts.
- **Observed never overwrites taught.** "720p is low", said, beats what Iris inferred; every fact
  carries where it came from: said / corrected / inferred.
- **Append-only, with author and time.** How a preference changed stays visible and can be undone.
- **The user sees what Iris knows about them** and can correct it.

## When to lay the foundation - Claude's answer, for Pavel to weigh

**A thousand users never reach one Iris.** An incarnation speaks with exactly one user
(`ecosystem-strategy.md` §2a, wording of 2026-11-13). A thousand users are a thousand Irises, each
with one user's memory - the same size as Pavel's today. What has to grow with the users is the
Ecosystem (storage, isolation, grants) and the Gateway (models), not Iris. So Iris is not rewritten
for a thousand users, provided the few things that are expensive to change later are in from the
start.

**Lay now - cheap today, expensive later:**

1. **Every fact has an owner** - the user and the Iris incarnation - even while there is one user.
   Adding it to a thousand users' records afterwards is a migration of everyone's memory.
2. **Every fact carries its source and time, and is never overwritten** - the rules above. A memory
   written without them cannot be given them later: nobody knows any more what was said and what
   was guessed.
3. **Facts go through the Ecosystem contract**, never into a local file of Iris - a file is the shape
   `CAREER_DATA_DIR` shows is costly to leave.
4. **"Give me what bears on this question" is one interface in Iris** whose first implementation is
   trivial: return everything. The rest of Iris calls only the interface.

**Leave for later - cheap to change behind those four:**

- How the relevant facts are found: everything into the prompt -> by topic or keyword -> embeddings
  from the Gateway. A new implementation behind point 4, nothing else changes. Needed when one
  user's facts no longer fit a prompt - a question of one user's volume, not of the user count.
- How the Ecosystem stores them (files, SQLite, a server) - its own business.
- The page "what Iris knows about me" - when there is a UI to put it in.
- Everything between users: shared experience, one Iris learning from another's cases, grants.

**The steps of learning are not all laid at once** (Pavel's chain, 2026-11-13: gather -> pick out
the main nodes -> gather the details around them -> RAG for fast search). Only the first cannot be
done later: what the user said today and was not written down is gone. The rest is computed from
what was gathered - nodes, groups, an index are a cache over the raw facts (`ecosystem-strategy.md`:
what an agent can recompute stays a cache) and can be run over the whole history when they are
built, as if they had run from the first day. One demand on the gathering follows: **each fact with
its context** - which file, folder or question it was about, who said it, when, how it ended.
Without it "that is not mosaic" means nothing a year later. Which nodes really appear is then seen
in the facts rather than guessed now.

**So "nothing about RAG in the code" is right today** - as long as points 1-4 are there. They are
the foundation; the search is a part that is swapped in later.

## Released and left for a year: the incarnation improves itself

Pavel's real question (2026-11-13): Iris is built, handed out and not touched for a year or two -
does it only gather, or does its code make the incarnation better over time? Or must the code
change every two months as the incarnations pile up data?

The answer above ("later") holds **while developing**, with one user. **For a release it does not**:
the released code carries the whole learning, every stage in it from the first release, each one
**switching itself on when the data is there** - not by a date, not by our change:

| Stage | On when | Does |
|---|---|---|
| Gathering | always | facts with their context |
| Choice for the prompt | always | everything, while it fits |
| Search (RAG) | the facts no longer fit a prompt | search by embeddings from the Gateway |
| Nodes and groups | N new facts since the last time | rebuilds nodes and groups over the whole history, in the background |
| Condensing | a node's group has grown large | condenses its details into short knowledge; the raw facts stay |

The thresholds are parameters, not code. An incarnation a month old runs the first two stages, one
two years old all of them - on the same code. The code changes when a **better way** is wanted (a
new grouping, a better model), never because the data grew; a new way recomputes from the raw facts.

Risks:
1. **Not checkable on live data now** - one user, little data. Tests on a **synthetic history** (a
   year of invented facts and corrections run through all stages: the thresholds fire, the
   knowledge does not rot) are a condition of the release; without them "release and forget" is a
   lottery.
2. **Grouping and condensing are done by a model, and it errs.** So: derived knowledge never
   replaces a raw fact, what the user said beats what was inferred, everything derived can be
   rebuilt from nothing.
3. **Which nodes will appear is not known now** - no node types fixed in the code (file, folder,
   codec): nodes are found in the data, or in a year what matters fits no type and that is a code
   change.
4. **The user sees and resets** what the incarnation learned - otherwise a wrong lesson is fixed only
   by a code change or by wiping the memory.

Order: **now** - gathering and the four points of the foundation, and on Pavel's own data see which
nodes really appear; **before the first release to others** - all the stages with switching on by
themselves, and the synthetic-history tests.

## First step, when Pavel decides to start

In Jira: ECOSYSTEM-2 (the minimal API, with facts added beside documents in its comment of
2026-11-13) blocks IRIS-82 (Iris remembers tools and preferences in the Ecosystem).

The record "a fact about the user" (what, source, time, what it bears on, owner) as Iris's first
schema in the Ecosystem, with the minimal contract to put and read it and the tests "how Iris uses
it" in the AI Ecosystem repository (the contract grows from its consumers; the tests are shared, so
the next agent cannot break this one). Before the tool description of `tools-by-description.md`:
the fact is what starts the learning.
