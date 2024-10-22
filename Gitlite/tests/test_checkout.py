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