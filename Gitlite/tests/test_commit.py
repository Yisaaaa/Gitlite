import subprocess

import utils

def test_commit_no_log_message():
    """
    Using commit command and not providing a message is not allowed.
    """
    utils.setup()
    subprocess.run(["touch", "hello.txt", "'hello' >> hello.txt"])
    utils.run_gitlite_cmd("add hello.txt")
    stdout, return_code = utils.run_gitlite_cmd("commit")
    
    assert return_code != 0
    assert "Please provide a commit message."

def test_commit_no_changes():
    """
    Using commit command on a repo with no changes is not allowed.
    """
    
    utils.setup()