import os.path

import utils
import subprocess

def test_status(setup_and_cleanup):
    """
    Tests status.
    """
    
    utils.create_file("a.txt")
    utils.run_gitlite_cmd("add a.txt")
    stdout,stderr, return_code = utils.run_gitlite_cmd("status", stderr=True)
    # print(stderr, stdout)
    
    # Checking for branch and staged file
    assert return_code == 0
    assert "=== Branches ===\n*master" in stdout
    assert "=== Staged Files ===\na.txt" in stdout

    utils.create_file("remove_me.txt")
    utils.run_gitlite_cmd("add remove_me.txt")
    utils.run_gitlite_cmd(["commit", "first commit"])
    
    utils.create_file("b.txt", "Im b!")
    utils.run_gitlite_cmd("rm remove_me.txt")
    stdout, stderr, return_code = utils.run_gitlite_cmd("status", stderr=True) 
    
    # Staged file should be empty after commit
    # Testing untracked file
    # print(stdout, stderr)
    assert return_code == 0
    assert "=== Staged Files ===\n\n=== Removed Files ===\nremove_me.txt" in stdout
    assert "=== Untracked Files ===\nb.txt" in stdout

    utils.run_gitlite_cmd("add b.txt")
    utils.run_gitlite_cmd(["commit", "second commit"])
    utils.run_gitlite_cmd("status")

    # Now let's test for each of the modified file cases
    # Case one: Tracked in the current commit, changed in the working directory, but not staged
    subprocess.run(["echo 'hello' >> a.txt"], shell=True)

    # Case two: Staged for addition, but with different contents than in the working directory
    utils.create_file("c.txt")
    utils.run_gitlite_cmd("add c.txt")
    subprocess.run("echo 'some random string' >> c.txt", shell=True)

    # Case three: Staged for addition, but deleted in the working directory
    utils.create_file("d.txt")
    utils.run_gitlite_cmd("add d.txt")
    subprocess.run("rm d.txt", shell=True)

    # Case four: Not staged for removal, but tracked in the current commit 
    # and deleted from the working directory
    subprocess.run(["rm", "b.txt"])

    stdout, stderr, return_code = utils.run_gitlite_cmd("status", stderr=True)
    assert return_code == 0
    assert not os.path.exists("remove_me.txt") # we used 'Gitlite rm' on this file
    assert "=== Modifications Not Staged For Commit ===\na.txt (modified)\nb.txt (deleted)\nc.txt (modified)\nd.txt (deleted)"