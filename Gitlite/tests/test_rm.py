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
    print(stdout)
    
    assert "hello.txt" in stdout