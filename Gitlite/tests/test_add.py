import os
import subprocess
import utils

def test_add_single_file(setup_and_cleanup):
    """
    Tests add command on a single file
    """
    subprocess.run(["touch", "hello.txt", "'Hello' >> hello.txt"])
    stdout, return_code = utils.run_gitlite_cmd("add hello.txt")
    assert return_code == 0
    
    stdout, return_code = utils.run_gitlite_cmd("read staging staging")
    assert return_code == 0
    assert "hello.txt" in stdout
    
def test_add_on_non_existent_file(setup_and_cleanup):
    """
    Tests add command on a file that does not exist.
    """
    stdout, return_code = utils.run_gitlite_cmd("add hello.txt")
    
    assert "File does not exist." in stdout
    assert return_code != 0
    