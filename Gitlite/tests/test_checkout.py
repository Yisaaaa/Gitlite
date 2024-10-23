import os.path
import subprocess
import utils

def test_checkout_only_file(setup_and_cleanup):
    """
    Tests checkout command on a single file argument.
    """
    
    original_content = "I am the original!"
    utils.create_file("a.txt", original_content)
    utils.run_gitlite_cmd("add a.txt")
    utils.run_gitlite_cmd(["commit", "first commit"])
    
    subprocess.run([f"echo 'No, I am!' > a.txt"], shell=True)
    _, return_code = utils.run_gitlite_cmd("checkout -- a.txt")
    
    assert return_code == 0
    with open("a.txt", "r") as test_file:
        content = test_file.read()
        assert original_content in content
        
def test_checkout_file_failure_cases(setup_and_cleanup):
    """
    Tests on the failure cases of checkout. Basically, checking out a non-existent file
    or file on a non-existent commit is not allowed. Commit ids must also be >= to 5
    hexadecimal characters. In theory, it can be a little less than that (> 2 at least),
    but in that case, it would not be unique enough and this is but a design decision I made.
    """
    utils.create_file("test.txt")
    utils.run_gitlite_cmd("add test.txt")
    utils.run_gitlite_cmd("commit first")
   
    # Trying on a random commit id that does not exist
    stdout, return_code = utils.run_gitlite_cmd("checkout 24a2145 -- test.txt")
    # Should fail
    assert "No commit with that id exists." in stdout
   
    commit_ids = get_commit_ids_from_log() # commit_ids[0] is the most recent commit
    # Trying on a random file that does not exist
    stdout, return_code = utils.run_gitlite_cmd(f"checkout {commit_ids[0]} -- dunno.txt")
    
    assert return_code != 0
    assert "File does not exist in that commit." in stdout
    
    # Trying to provide less than the min length of hash characters (5 is min)
    stdout, return_code = utils.run_gitlite_cmd(f"checkout {commit_ids[0][:4]} -- test.txt")
    
    assert return_code != 0
    assert "Short hash must at least be 5 characters long" in stdout

def test_checkout_with_commit_id(setup_and_cleanup):
    """
    Tests checkout a file from a commit given its id.
    """
    
    content_from_first_commit = "I am the first, and will be the last!"
    utils.create_file("diary.txt", content_from_first_commit)
    utils.run_gitlite_cmd("add diary.txt")
    utils.run_gitlite_cmd(["commit", "to the past"])
    
    subprocess.run(["echo 'All of it is just a burning memory' > diary.txt"], shell=True)
    utils.run_gitlite_cmd("add diary.txt")
    utils.run_gitlite_cmd(["commit", "wish we could go back"])
    
    with open("diary.txt", "r") as file:
        assert "All of it is just a burning memory" in file.read()
    
    commit_ids = get_commit_ids_from_log()
    first_commit = commit_ids[1].strip()
    
    stdout, stderr, return_code = utils.run_gitlite_cmd(f"checkout {first_commit} -- diary.txt", stderr=True)
    assert return_code == 0
    
    with open("diary.txt", "r") as file:
        assert "I am the first, and will be the last!" in file.read()

def test_checkout_a_branch(setup_and_cleanup):
    """
    Tests checking out a branch. Must fail if the branch does not exist, if the branch to checkout
    is the current branch, and if there is an untracked files on the current branch.
    """
    # Creating a branch
    _, return_code = utils.run_gitlite_cmd("branch cool-beans")
    assert return_code == 0
    assert os.path.exists(os.path.join(".gitlite", "branches", "cool-beans"))

    utils.create_file("wug.txt", "This is a wug.")

    # Checking out with an untracked file in the current branch
    stdout, stderr, return_code = utils.run_gitlite_cmd("checkout cool-beans", stderr=True)
    assert "There is an untracked file in the way; delete it, or add and commit it first." in stdout
    assert return_code != 0

    utils.add_and_commit(["wug.txt"], "Is it a wug?")

    # Creating a branch
    _, return_code = utils.run_gitlite_cmd("branch wug-society")
    assert return_code == 0
    assert utils.read_file(os.path.join(".gitlite", "HEAD")) == "ref: master"
    assert os.path.exists(os.path.join(".gitlite", "branches", "wug-society"))
    assert utils.read_file(os.path.join(".gitlite", "branches", "wug-society")) == \
           utils.read_file(os.path.join(".gitlite", "branches", "master"))

    # Checking out a branch that doesn't exist.
    stdout, return_code = utils.run_gitlite_cmd("checkout not-a-branch")
    assert return_code != 0
    assert "No such branch exists." in stdout

    _, stderr, return_code = utils.run_gitlite_cmd("checkout wug-society", stderr=True)
    assert return_code == 0

    head = os.path.join(".gitlite", "HEAD")

    assert utils.read_file(head) == "ref: wug-society",\
        "HEAD must reference the new branch after checkout."

    # Make a commit on the new branch
    subprocess.run(["echo 'This is definitely a wug' > wug.txt"], shell=True)
    utils.add_and_commit(["wug.txt"], "A real wug")

    # Checkout back to the master branch
    utils.run_gitlite_cmd("checkout master")
    assert utils.read_file(head) == "ref: master",\
        "HEAD must reference the master branch now."

    assert utils.read_file("wug.txt") == "This is a wug.",\
        "wug.txt must have the contents of the wug.txt before the commit in the new branch."

    subprocess.run(["echo 'This is not a wug' > wug.txt"], shell=True)
    utils.add_and_commit(["wug.txt"], "A fake wug")

    # Checkout back to the new branch
    utils.run_gitlite_cmd("checkout wug-society")
    assert utils.read_file("wug.txt") == "This is definitely a wug",\
        "wug.txt must have the contents of the wug.txt in the new branch"
    
def get_commit_ids_from_log():
    """
    Calls the log command of gitlite and takes all the commit id's.
    :return: Returns a list of commit id's.
    """
    commit_ids = []
    stdout, _ = utils.run_gitlite_cmd("log")
    for line in stdout.split("\n"):
        line_split = line.split(" ")
        if len(line_split) == 2 and "Commit:" == line_split[0]:
            commit_ids.append(line_split[1])
            
    return commit_ids