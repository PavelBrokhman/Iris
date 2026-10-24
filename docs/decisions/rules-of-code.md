# Rules of code

Decided 2026-10-24 (Pavel).

**Every dependency has an interface from the first day** - ffmpeg, the Gateway, the disk, Lada,
time; instance classes, not static.
**Why:** a project too small for a test is rare, and a test is already a second implementation
(Pavel). Pure recipe logic needs none - it is tested on data directly, with fewer fakes and more
real code in the tests.

**A number with a meaning is always a named constant; a setting only when someone will change it.**
**Why:** Mosaic Remover's "no magic numbers" against the hub's YAGNI - both kept, each where it
belongs.

Also, from both sides:

- do not widen the scope - offer more in one line, do not do it;
- a comment only for a non-obvious why, none for removed code;
- validate at the boundaries (input, external APIs), trust internal code;
- keep CRLF in CRLF files (Mosaic Remover's `scripts\crlf.py`);
- never rewrite a source file through PowerShell without `-Encoding`;
- push back on "a chto esli" with YAGNI.
