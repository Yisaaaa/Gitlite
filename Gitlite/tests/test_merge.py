from os import remove

import utils, subprocess

"""
Runs the development tests for merge. Not final.
"""

def simulate_diverging_branches(setup):
    """
    This simulates a diverging branches. It creates another branch 'feature'. It diverges them
    by modifying the file 'foo.txt' in different ways in the two branches.
    
    :return: None
    """
    
    utils.create_add_commit("foo.txt", "", "first commit")
    utils.run_gitlite_cmd("branch feature") # New branch feature

    # Diverging modifying the file differently in the two branches
    subprocess.run("echo 'foo' > foo.txt", shell=True)
    utils.add_and_commit(["foo.txt"], "add foo")

    utils.run_gitlite_cmd("checkout feature")
    utils.subprocess.run("echo 'bar' > foo.txt", shell=True)
    utils.add_and_commit(["foo.txt"], "add bar")
    
def test_untracked_file_conflict(setup):
    """
    A merge would cause a delete conflict with an untracked file if:
        - the file does is absent (untracked/unstaged) in both branches but exists in the split point
        - the file is present in the split point, absent in the current branch, and unmodified in the
          given branch
        
    And an overwrite conflict if:
        - the file is present in the split point, absent in the current branch, but modified in the 
          given branch
        - the file is not present in the split point, absent in the current branch, but present in
          the given branch
        
    :return: None 
    """
    
    # Untracked file is present in the split point:
    create_split_point("foo.txt", "hello", "feature", "split point")
    
    # and also absent in both branches but present in working dir.
    # Removing foo.txt on both branches
    remove_and_commit("foo.txt", "removed foo")
    utils.run_gitlite_cmd("checkout feature")
    remove_and_commit("foo.txt", "remove foo")
    subprocess.run("echo 'untracked foo' > foo.txt", shell=True)
    
    stdout, return_code = utils.run_gitlite_cmd("merge master")
    assert "There is an untracked file in the way; delete it, or add and commit it first." == stdout
    assert return_code != 0
    
    
    
def create_split_point(filename: str, content: str, branch_name: str, commit_msg: str):
    """
    Creates a split point with file that has the given content, committing this with a the
    commit_msg and creating a branch.
    
    :param filename: Name of the file to be added in commit.
    :param content: Content of the file to create
    :param branch_name: Name of the branch to create
    :param commit_msg: Message of the commit
    :return: None
    """
    
    utils.create_add_commit(filename, content, commit_msg)
    utils.run_gitlite_cmd(f"branch {branch_name}")
  
  
def remove_and_commit(filename: str, commit_msg: str):
    """
    Runs './Gitlite rm <filename>' and commits it.
    :param filename: 
    :param commit_msg: 
    :return: None
    """
    
    utils.run_gitlite_cmd(f"rm {filename}")
    utils.run_gitlite_cmd(["commit", commit_msg])