import os.path
import utils

def test_rm_on_tracked_file(setup_and_cleanup):
    """
    Tests rm on a tracked file.
    """
    utils.create_file("hello.txt", "heloo there")
    utils.run_gitlite_cmd("add hello.txt")
    utils.run_gitlite_cmd(["commit", "justa commit"])
    
    utils.run_gitlite_cmd("rm hello.txt")
    
    assert not os.path.exists("hello.txt")
    
    stdout, _ = utils.run_gitlite_cmd("read staging staging")
    
    assert "hello.txt" in stdout
    
    
def test_rm_on_staged_file(setup_and_cleanup):
    """
    Tests rm on a file that is staged for addition.
    """
    utils.create_file("hello.txt", "This file is staged for addition")
    
    # Adding the file and then removing it
    utils.run_gitlite_cmd("add hello.txt")
    utils.run_gitlite_cmd("rm hello.txt")
    stdout, _ = utils.run_gitlite_cmd("read staging staging")
    
    assert "hello.txt" not in stdout
    assert os.path.exists("hello.txt") # Untracked file must not be removed from the repo.
    
def test_failure_case(setup_and_cleanup):
    """
    Tests rom on a file that is neither tracked or staged.
    """
    utils.create_file("hello.txt", "This file is untracked and unstaged.")
    stdout, return_code = utils.run_gitlite_cmd("rm hello.txt")
    
    assert return_code != 0
    assert "No reason to remove the file: hello.txt" in stdout
