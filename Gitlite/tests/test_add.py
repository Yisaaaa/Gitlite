import os
import subprocess
import utils

def test_add_single_file():
    """
    Tests add command on a single file
    """
    utils.setup()
    utils.run_gitlite_cmd("init")
    subprocess.run(["touch", "hello.txt", "'Hello' >> hello.txt"])
    stdout, return_code = utils.run_gitlite_cmd("add hello.txt")
    
    assert return_code == 0
    
    utils.clean_up()
    
def test_add_on_non_existent_file():
    """
    Tests add command on a file that does not exist.
    """
    utils.setup()
    utils.run_gitlite_cmd("init")
    stdout, return_code = utils.run_gitlite_cmd("add hello.txt")
    
    assert "File does not exist." in stdout
    assert return_code != 0
    
    utils.clean_up()