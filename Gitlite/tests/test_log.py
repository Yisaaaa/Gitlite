import utils
import subprocess

def test_log(setup_and_cleanup):
    utils.create_file("hello", "I've lost all ambition")
    messages = ["First commit", "Second commit", "Third commit"]
    
    for i in range(len(messages)):
        subprocess.run([f"echo '{messages[i]}' >> hello"], shell=True)
        utils.run_gitlite_cmd("add hello")
        utils.run_gitlite_cmd(["commit", messages[i]])

        
    stdout, returncode = utils.run_gitlite_cmd("log")
    assert returncode == 0
    
    print(stdout)
    for i in range(len(messages)):
        assert messages[i] in stdout