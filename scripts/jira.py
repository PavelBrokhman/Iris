"""
Iris Jira wrapper - thin layer over claude-hub/shared/jira/jira_client.py (Milo's pattern).

Credentials from Iris's own .env, gitignored (JIRA_PROJECT_KEY, JIRA_EMAIL, JIRA_API_KEY).
The space is IRIS unless --project names another space of the umbrella
(MOSAIC, VIDEOTOSRT, GATEWAY, HUB). A subtask always goes to the space of its parent.

CLI usage:
    python scripts/jira.py create epic    "Agent" [--desc "..."]
    python scripts/jira.py create story   "A piece" --epic IRIS-2
    python scripts/jira.py create subtask "Tests pass" --parent IRIS-7
    python scripts/jira.py get      IRIS-7
    python scripts/jira.py list     [--type epic|story|subtask] [--status "In Review"]
    python scripts/jira.py update   IRIS-7 [--summary "New"] [--desc "New desc"]
    python scripts/jira.py transit  IRIS-7 "In Progress"
    python scripts/jira.py statuses IRIS-7
    python scripts/jira.py comment  IRIS-7 --body "Landed."
    python scripts/jira.py link     GATEWAY-2 IRIS-10 [--type Relates]   (GATEWAY-2 blocks IRIS-10)
    python scripts/jira.py delete   IRIS-7
    python scripts/jira.py --project MOSAIC list --status "In Review"
"""

import argparse
import os
import sys
from pathlib import Path

from dotenv import load_dotenv

# The shared client reads the environment when it is imported, so the file is loaded first.
_ENV_FILE = Path(__file__).resolve().parents[1] / ".env"
load_dotenv(_ENV_FILE, override=True)
sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "claude-hub"))

from shared.jira.jira_client import (  # noqa: E402
    _post as post,
    add_comment,
    create_issue,
    delete_issue,
    get_issue,
    get_transitions,
    list_issues,
    transition_issue,
    update_issue,
)

IRIS_PROJECT = "IRIS"
LINK_PATH = "/rest/api/3/issueLink"


def _space_of(issue_key: str) -> str:
    return issue_key.rsplit("-", 1)[0]


def link(blocker: str, blocked: str, link_type: str = "Blocks") -> None:
    # Jira shows a link as "<inwardIssue> blocks <outwardIssue>" - the other way round from the
    # names; six links were made backwards on 2026-10-24 before this was seen.
    post(LINK_PATH, {
        "type": {"name": link_type},
        "inwardIssue": {"key": blocker},
        "outwardIssue": {"key": blocked},
    })


def _print_issue(issue: dict) -> None:
    f = issue.get("fields", {})
    print(f"[{issue['key']}] {f.get('summary', '')}")
    print(f"  Type   : {f.get('issuetype', {}).get('name', '')}")
    print(f"  Status : {f.get('status', {}).get('name', '')}")
    parent = f.get("parent")
    if parent:
        print(f"  Parent : {parent['key']}")
    for sub in f.get("subtasks", []):
        print(f"  - [{sub['key']}] [{sub['fields']['status']['name']}] {sub['fields']['summary']}")
    for lnk in f.get("issuelinks", []):
        if "outwardIssue" in lnk:
            print(f"  Link   : {lnk['type']['outward']} {lnk['outwardIssue']['key']}")
        else:
            print(f"  Link   : {lnk['type']['inward']} {lnk['inwardIssue']['key']}")


def _build_parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(
        description=f"Iris Jira CLI (space: {IRIS_PROJECT} unless --project)",
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    p.add_argument("--project", default=IRIS_PROJECT, metavar="KEY")
    sub = p.add_subparsers(dest="command", metavar="command")

    cr = sub.add_parser("create", help="Create an issue")
    cr.add_argument("type", choices=["epic", "story", "task", "subtask"])
    cr.add_argument("summary")
    cr.add_argument("--desc", default="", metavar="TEXT")
    cr.add_argument("--epic", default="", metavar="EPIC-KEY", help="Parent epic (for stories)")
    cr.add_argument("--parent", default="", metavar="ISSUE-KEY", help="Parent issue (for subtasks)")

    g = sub.add_parser("get", help="Get an issue with its subtasks and links")
    g.add_argument("key")

    ls = sub.add_parser("list", help="List issues of the space")
    ls.add_argument("--type", default="", dest="issue_type", metavar="TYPE")
    ls.add_argument("--status", default="", metavar="STATUS")
    ls.add_argument("--max", type=int, default=50)

    up = sub.add_parser("update", help="Update summary or description")
    up.add_argument("key")
    up.add_argument("--summary", default="")
    up.add_argument("--desc", default="", metavar="TEXT")

    tr = sub.add_parser("transit", help="Move an issue to a status")
    tr.add_argument("key")
    tr.add_argument("status", metavar="STATUS")

    st = sub.add_parser("statuses", help="List available transitions for an issue")
    st.add_argument("key")

    cm = sub.add_parser("comment", help="Add a comment to an issue")
    cm.add_argument("key")
    cm.add_argument("--body", required=True, metavar="TEXT")

    lk = sub.add_parser("link", help="Link two issues: BLOCKER blocks BLOCKED")
    lk.add_argument("blocker")
    lk.add_argument("blocked")
    lk.add_argument("--type", default="Blocks", dest="link_type", metavar="TYPE",
                    help="Blocks, Relates, Duplicate, Cloners")

    dl = sub.add_parser("delete", help="Delete an issue (irreversible)")
    dl.add_argument("key")

    return p


def main(argv: list[str] | None = None) -> None:
    parser = _build_parser()
    args = parser.parse_args(argv)

    if args.command == "create":
        project = _space_of(args.parent) if args.parent else args.project
        result = create_issue(
            summary=args.summary,
            issue_type=args.type,
            project=project,
            description=args.desc,
            epic_key=args.epic,
            parent_key=args.parent,
        )
        key = result.get("key")
        jira_url = os.environ.get("JIRA_PROJECT_KEY", "").split("/jira/", 1)[0]
        print(f"Created: {key}  ->  {jira_url}/browse/{key}")

    elif args.command == "get":
        _print_issue(get_issue(args.key))

    elif args.command == "list":
        issues = list_issues(project=args.project, issue_type=args.issue_type,
                             status=args.status, max_results=args.max)
        if not issues:
            print("No issues found.")
        for issue in issues:
            f = issue["fields"]
            print(
                f"[{issue['key']}]"
                f" [{f.get('issuetype', {}).get('name', '')}]"
                f" [{f.get('status', {}).get('name', '')}]"
                f"  {f.get('summary', '')}"
            )

    elif args.command == "update":
        update_issue(args.key, summary=args.summary, description=args.desc)
        print(f"Updated: {args.key}")

    elif args.command == "transit":
        transition_issue(args.key, args.status)
        print(f"{args.key} -> {args.status}")

    elif args.command == "statuses":
        print(f"Available transitions for {args.key}:")
        for t in get_transitions(args.key):
            print(f"  {t['name']}")

    elif args.command == "comment":
        add_comment(args.key, args.body)
        print(f"Comment added to {args.key}")

    elif args.command == "link":
        link(args.blocker, args.blocked, args.link_type)
        print(f"Linked: {args.blocker} {args.link_type} {args.blocked}")

    elif args.command == "delete":
        confirm = input(f"Delete {args.key}? This is irreversible. Type 'yes' to confirm: ")
        if confirm.strip().lower() == "yes":
            delete_issue(args.key)
            print(f"Deleted: {args.key}")
        else:
            print("Cancelled.")

    else:
        parser.print_help()


if __name__ == "__main__":
    main()
