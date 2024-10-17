import os
import subprocess
import utils

def test_commit_no_log_message(setup_and_cleanup):
    """
    Using commit command and not providing a message is not allowed.
    """
    subprocess.run(["touch", "hello.txt", "'hello' >> hello.txt"])
    utils.run_gitlite_cmd("add hello.txt")
    stdout, return_code = utils.run_gitlite_cmd("commit")
    
    assert return_code != 0
    assert "Please provide a commit message." in stdout

def test_commit_no_changes(setup_and_cleanup):
    """
    Using commit command on a repo with no changes is not allowed.
    """
    stdout, return_code = utils.run_gitlite_cmd(["commit", "a commit with no changes"])
    
    assert return_code != 0
    assert "No changes added to the commit." in stdout
    
def test_commit_single_file(setup_and_cleanup):
    """
    Tests committing a single file.
    """
    with open(".gitlite/HEAD", "r") as file:
        hash_of_initial_commit = file.read().strip() 
    
    subprocess.run(["touch", "hello.txt"])
    subprocess.run(["echo 'hello' >> hello.txt"], shell=True)
    utils.run_gitlite_cmd("add hello.txt")
    stdout, return_code = utils.run_gitlite_cmd(["commit", "my first commit"])
    assert return_code == 0
    
    # HEAD pointer must be updated after committing.
    with open(".gitlite/HEAD", "r") as file:
        hash_of_new_commit = file.read().strip()
    
    assert hash_of_new_commit != hash_of_initial_commit
    
    # New commit object must exist on .gitlite/commits/
    first_two_digits, rest = hash_of_new_commit[0:2], hash_of_new_commit[2:]
    new_commit_path = os.path.join(".gitlite", "commits", first_two_digits, rest)
    assert os.path.exists(new_commit_path)
    
    # Master pointer must be updated
    with open(".gitlite/branches/master", "r") as file:
        content = file.read().strip()
        assert hash_of_new_commit == content
    
    # Checking if the blob tracked by the new commit exists and has the correct content
    # "Gitlite read" serializes commits into json and prints them
    stdout, _ = utils.run_gitlite_cmd(f"read {hash_of_new_commit} commit")
    blob = find_blob_from_commit(stdout, "hello.txt")
    blob_path = f".gitlite/blobs/{blob[1]}"
    assert os.path.exists(blob_path), "blob must be created for the added file"

    # Blob must have the correct content of the added file
    cmd = f"read {blob[1]} blob"
    stdout, _ = utils.run_gitlite_cmd(cmd)
    assert "hello" in stdout
    
    
def test_two_files_same_content(setup_and_cleanup):
    """
    Two files with the same content must point to a single blob as file reference.
    """
    subprocess.run(["touch a.txt && touch b.txt"], shell=True)
    subprocess.run(["echo 'hello' >> a.txt && echo 'hello' >> b.txt"], shell=True)
    utils.run_gitlite_cmd("add a.txt")
    utils.run_gitlite_cmd("add b.txt")
    utils.run_gitlite_cmd(["commit", "a commit"])

    with open(".gitlite/HEAD", "r") as file:
        hash_of_new_commit = file.read().strip()
    
    stdout, _ = utils.run_gitlite_cmd(f"read {hash_of_new_commit} commit")
    a = find_blob_from_commit(stdout, "a.txt")
    b = find_blob_from_commit(stdout, "b.txt")
    
    assert a[1].strip() == b[1].strip()
    

def find_blob_from_commit(commit_json_serialied, file_name):
    """
    Helper function that finds the blob map for a file
    of a commit given the file's name.
    :param commit_json_serialied: String version of the json serialized commit.
    :param file_name: Name of the file mapped to the blob.
    :return: A list like so, [name of the file, hash ref of the blob] 
    """
    commit_json_serialied = commit_json_serialied.split("\n")
    
    for line in commit_json_serialied:
        if file_name in line:
            return [i.strip().replace('"', "").replace(",", "") for i in line.split(":")]
        
        
