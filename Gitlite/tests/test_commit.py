import os
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
    assert "Please provide a commit message." in stdout
    
    utils.clean_up()

def test_commit_no_changes():
    """
    Using commit command on a repo with no changes is not allowed.
    """
    utils.setup()
    stdout, return_code = utils.run_gitlite_cmd(["commit", "a commit with no changes"])
    
    assert return_code != 0
    assert "No changes added to the commit." in stdout
    
    utils.clean_up()
    
def test_commit_single_file():
    """
    Tests committing a single file.
    """
    utils.setup()
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
    new_commit_path = f".gitlite/commits/{hash_of_new_commit}"
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
    
    utils.clean_up()


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
            return [i.strip().replace('"', "") for i in line.split(":")]
        
        
