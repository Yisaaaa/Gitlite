import subprocess

import utils

def test_checkout_single_file(setup_and_cleanup):
    """
    Tests Checkout command on a single file argument.
    """
    
    original_content = "I am the original!"
    utils.create_file("a.txt", original_content)
    utils.run_gitlite_cmd("add a.txt")
    utils.run_gitlite_cmd(["commit", "first commit"])
    
    subprocess.run([f"echo 'No, I am!' > a.txt"], shell=True)
    utils.run_gitlite_cmd("checkout -- a.txt")
    
    with open("a.txt", "r") as test_file:
        content = test_file.read()
        assert content.strip() == original_content