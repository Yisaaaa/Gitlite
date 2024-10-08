import utils
import subprocess

def test_find(setup_and_cleanup):
    """
    Tests "./Gitlite find" command
    """
    hashes = [] # where the new commits' hashes are stored  
    commitMessages = ["This is a commit message and we are testing.",
                      "A commit message must be descriptive",
                      "Each commit must have a commit message"
                      ] 
    
    utils.create_file("test_file", "this is a file")
    
    for i in commitMessages:
        subprocess.run([f"echo '{i}' >> test_file"], shell=True)
        utils.run_gitlite_cmd("add test_file")
        utils.run_gitlite_cmd(["commit", i])
        
        # store the new commit's hash
        with open(".gitlite/HEAD", "r") as file:
            hashes.append(file.read().strip())

    # Should print: "Found no commit with that message.", when no commit with matching message is found
    stdout, return_code = utils.run_gitlite_cmd(["find", "can't find me"]) 
    assert return_code == 0
    assert "Found no commit with that message." in stdout
    
    # Should print the matching commit if there is
    stdout, return_code = utils.run_gitlite_cmd(["find", "commit message"])
    assert return_code == 0
    for i in hashes:
        assert i in stdout

    stdout, return_code = utils.run_gitlite_cmd(["find", "testing"])
    assert return_code == 0
    assert hashes[0] in stdout
    assert hashes[1] not in stdout