import os
import subprocess
import shutil
from os import pathconf
from tkinter.messagebox import RETRY

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
        

def run_gitlite_cmd(cmd, stderr=False):   
    """
    Given a CMD argument, runs the Gitlite command corresponding to it.
    :param cmd: (str or a list of str) Command to run on Gitlite
    :param stderr: Also returns stderr if True.
    :return: stdout (output), return_code 
    """
    if isinstance(cmd, str):
        result = subprocess.run(["./Gitlite"] + cmd.split(), capture_output=True, text=True)
    else:
        result = subprocess.run(["./Gitlite"] + cmd, capture_output=True, text=True)
        
    if stderr:
        return result.stdout.strip(), result.stderr.strip(), result.returncode
    
    return result.stdout.strip(), result.returncode

def clean_up():
    """
    Cleans up the testing directory.
    """
    os.chdir("../")
    shutil.rmtree(test_dir)
    os.chdir("../")
    
    
def create_file(name, content=""):
    """
    Creates file named NAME with CONTENT as contents.
    :param name: File name
    :param content: File content
    :return: None 
    """
    
    if isinstance(name, str) and isinstance(content, str):
        subprocess.run(["touch", name])
        subprocess.run([f"echo '{content}' >> {name}"], shell=True)
        
    elif isinstance(name, list) and isinstance(content, list):
        assert len(name) == len(content), "Must have equal length."
        
        for i in range(len(name)):
            subprocess.run(["touch", name[i]])
            subprocess.run([f"echo '{content[i]}' >> {name[i]}"])
        
def split_hash(hash_ref):
    """
    Splits the hash to its first two digits but the rest, and the rest
    :param hash_ref: hash reference to split
    :return: Tuple (first two digits, rest)
    """
    
    return hash_ref[:2], hash_ref[2:]

def add_and_commit(files_to_add: list, commit_msg: str) -> None:
    """
    Adds the given files and make a commit with the given message.
    :param files_to_add: Files to add.
    :param commit_msg: Commit message
    :return: None
    """
    assert isinstance(files_to_add, list), "files_to_add must be a list."
    
    for file in files_to_add:
        
        stdout, stderr, return_code = run_gitlite_cmd("add " + file, stderr=True)
        if return_code != 0:
            print(stdout, stderr)

    stdout, stderr, return_code = run_gitlite_cmd(["commit", commit_msg], stderr=True)
    if return_code != 0:
        print("commit part")
        print(stdout, stderr, return_code)
    
def read_file(filepath: str) -> str:
    """
    Reads a file given its file and retrieves its contents.
    :param filepath: File path.  
    :return: String contents of the file.
    """
    with open(filepath, "r") as file:
        return file.read().strip()