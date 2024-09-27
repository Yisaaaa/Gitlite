import os
import subprocess
import shutil

test_dir = "test_tmp_dir"

def setup(init=True):
    """
    Sets up the directory for testing.
    :param init: if True, also do "Gitlite init"
    """
    os.chdir("tests")
    
    if os.path.isdir(test_dir):
        shutil.rmtree(test_dir)    
 
    os.makedirs(test_dir)
    os.chdir(test_dir)
    subprocess.run(["cp", "../Gitlite", "./"])
    
    if init:
        run_gitlite_cmd("init")
        

def run_gitlite_cmd(cmd):   
    """
    Given a CMD argument, runs the Gitlite command corresponding to it.
    :param cmd: (str or a list of str) Command to run on Gitlite
    :return: stdout (output), return_code 
    """
    if isinstance(cmd, str):
        result = subprocess.run(["./Gitlite"] + cmd.split(), capture_output=True, text=True)
    else:
        result = subprocess.run(["./Gitlite"] + cmd, capture_output=True, text=True)
        
    return result.stdout.strip(), result.returncode

def clean_up():
    """
    Cleans up the testing directory.
    """
    os.chdir("../")
    shutil.rmtree(test_dir)
    os.chdir("../")