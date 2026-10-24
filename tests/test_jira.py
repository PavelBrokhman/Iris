"""scripts/jira.py: which space each command goes to and what reaches the shared client.

The client is replaced by recorders - nothing here talks to Jira.
"""

import importlib.util
from pathlib import Path

import pytest

_SCRIPT = Path(__file__).resolve().parents[1] / "scripts" / "jira.py"


def _load():
    spec = importlib.util.spec_from_file_location("iris_jira", _SCRIPT)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


@pytest.fixture
def jira(monkeypatch):
    module = _load()
    calls = []

    def recorder(name, result=None):
        def record(*args, **kwargs):
            calls.append((name, args, kwargs))
            return result
        return record

    monkeypatch.setattr(module, "create_issue", recorder("create_issue", {"key": "IRIS-99"}))
    monkeypatch.setattr(module, "list_issues", recorder("list_issues", []))
    monkeypatch.setattr(module, "transition_issue", recorder("transition_issue"))
    monkeypatch.setattr(module, "add_comment", recorder("add_comment"))
    monkeypatch.setattr(module, "post", recorder("post", {}))
    module.calls = calls
    return module


def test_create_goes_to_iris_by_default(jira):
    jira.main(["create", "story", "A piece", "--epic", "IRIS-1"])
    name, _, kwargs = jira.calls[0]
    assert name == "create_issue"
    assert kwargs["project"] == "IRIS"
    assert kwargs["issue_type"] == "story"
    assert kwargs["epic_key"] == "IRIS-1"


def test_project_option_picks_another_space(jira):
    jira.main(["--project", "MOSAIC", "list", "--status", "In Review"])
    name, _, kwargs = jira.calls[0]
    assert name == "list_issues"
    assert kwargs["project"] == "MOSAIC"
    assert kwargs["status"] == "In Review"


def test_subtask_is_created_in_its_parent(jira):
    jira.main(["create", "subtask", "Tests pass", "--parent", "IRIS-7"])
    _, _, kwargs = jira.calls[0]
    assert kwargs["issue_type"] == "subtask"
    assert kwargs["parent_key"] == "IRIS-7"


def test_subtask_goes_to_the_space_of_its_parent(jira):
    jira.main(["create", "subtask", "Code", "--parent", "GATEWAY-2"])
    _, _, kwargs = jira.calls[0]
    assert kwargs["project"] == "GATEWAY"


def test_transit_moves_the_issue(jira):
    jira.main(["transit", "IRIS-7", "In Review"])
    assert jira.calls[0] == ("transition_issue", ("IRIS-7", "In Review"), {})


def test_comment_is_added(jira):
    jira.main(["comment", "IRIS-7", "--body", "Landed."])
    assert jira.calls[0] == ("add_comment", ("IRIS-7", "Landed."), {})


def test_link_reads_blocker_blocks_blocked(jira):
    # Jira shows a link as "<inwardIssue> blocks <outwardIssue>" - the other way round from the
    # names; on 2026-10-24 six links were made backwards this way.
    jira.main(["link", "GATEWAY-2", "IRIS-10"])
    name, args, _ = jira.calls[0]
    assert name == "post"
    path, body = args
    assert path == "/rest/api/3/issueLink"
    assert body == {
        "type": {"name": "Blocks"},
        "inwardIssue": {"key": "GATEWAY-2"},
        "outwardIssue": {"key": "IRIS-10"},
    }


def test_link_takes_another_type(jira):
    jira.main(["link", "IRIS-11", "MOSAIC-19", "--type", "Relates"])
    _, args, _ = jira.calls[0]
    assert args[1]["type"] == {"name": "Relates"}
