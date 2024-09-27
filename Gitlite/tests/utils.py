import os
import subprocess

test_dir = "test_tmp_dir"

def setup():
    """
    Sets up the directory for testing 
    """
    os.makedirs(test_dir, exist_ok=True)
    os.chdir(test_dir)
    subprocess.run(["cp", "../Gitlite", "./"])
    
def run_gitlite_cmd(cmd):
    result = subprocess.run(["./Gitlite"] + cmd.split(), capture_output=True, text=True)
    return result.stdout.strip(), result.returncode
    