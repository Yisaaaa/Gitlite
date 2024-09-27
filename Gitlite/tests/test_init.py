import os
import shutil
import utils

test_dir = "test_tmp_dir"
os.chdir("tests")

def test_init_on_fresh_dir():
    """
    Tests init on a fresh, uninitialized
    directory. 
    """
    utils.setup()

    stdout, return_code = utils.run_gitlite_cmd("init")
    assert return_code == 0
    assert "Initialized a new GitLite at" in stdout
    assert os.path.isdir(".gitlite")

    os.chdir(".gitlite")
    assert os.path.isdir("commits")
    assert os.path.isdir("blobs")
    assert os.path.isdir("branches")
    assert os.path.exists("branches/master")
    assert os.path.exists("HEAD")
    assert os.path.exists("staging")

    os.chdir("../../")
    shutil.rmtree(test_dir)
    
    
def test_init_on_initialized_dir():
    """
    Tests init command on an already initialized
    Gitlite repository
    """
    utils.setup()
    utils.run_gitlite_cmd("init")
    
    stdout, return_code = utils.run_gitlite_cmd("init")
    assert return_code != 0
    assert "A Gitlet version-control system already exists in the current directory." in stdout
