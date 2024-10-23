namespace Gitlite;

/*
 * TODO: Update the design document brought by the changes of file structure refactoring.
 * TODO: Refactor overloaded methods in Commit.
 *
 * TODO: Implement rm-branch
 */

/// <summary>
/// Class representing a GitLite repository.
/// </summary>
public class Repository
{
    
    // TODO: Might refactor and convert these into just strings
    public static DirectoryInfo CWD = Directory.GetParent(AppContext.BaseDirectory);
    public static DirectoryInfo GITLITE_DIR = Utils.JoinDirectory(CWD, ".gitlite");
    public static DirectoryInfo COMMITS_DIR = Utils.JoinDirectory(GITLITE_DIR, "commits");
    public static DirectoryInfo BLOBS_DIR = Utils.JoinDirectory(GITLITE_DIR, "blobs");
    public static DirectoryInfo BRANCHES = Utils.JoinDirectory(GITLITE_DIR, "branches");
    
    
    /* GITLITE MAIN COMMANDS */
    
    /// <summary>
    /// This initializes the GitLite repository on the Current Working Directory.
    /// </summary>
    public static void Init()
    {
        // Check if .gitlite folder already exists
        if (GitliteAlreadyInitialized())
        {
            Utils.ExitWithError("A Gitlet version-control system already exists in the current directory.");
        }
        
        // Creates the directory structure inside .gitlite
        CreateDirs();
        
        string hash = Gitlite.Commit.CreateInitialCommit();
        Gitlite.Branch.CreateBranch("master", hash);
        Utils.WriteContent(Path.Combine(GITLITE_DIR.ToString(), "HEAD"), "ref: master");
        
        Console.WriteLine($"Initialized a new GitLite at {CWD}");
    }
    
    public static void Add(string fileName)
    {
        if (!File.Exists(fileName))
        {
            Utils.ExitWithError("File does not exist.");
        }
        
        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();
        Dictionary<string, string> forAddition = stagingArea.GetStagingForAddition();
        Commit currentCommit = Gitlite.Commit.GetHeadCommit();
        byte[] content = File.ReadAllBytes(fileName);
        string contentHash = Utils.HashBytes(content);

        if (currentCommit.FileMapping.ContainsKey(fileName))
        {
            string blobRef = currentCommit.FileMapping[fileName];

            // Checks if content in commit and cwd are the same
            if (blobRef == contentHash && forAddition.ContainsKey(fileName))
            {
                // If it is, and we see it is and is currently staged for addition,
                // we just remove it. This can happen when file is changed, added,
                // and then changed back to original version.
                forAddition.Remove(fileName);
                stagingArea.Save();
                return;
            }
        }

        forAddition[fileName] = contentHash;
        Blob.SaveBlob(content);
        stagingArea.Save();
    }

    /// <summary>
    /// Commits the changes from the staging area with the provided log message.
    /// </summary>
    /// <param name="logMessage">The log message for the commit.</param>
    public static void Commit(string logMessage)
    {
        // Get staging area
        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();

        // Check for failure cases
        if (!stagingArea.IsThereStagedFiles())
        {
            Utils.ExitWithError("No changes added to the commit.");
        }
        
        Commit commitOnHEAD = Gitlite.Commit.GetHeadCommit();
        Dictionary<string, string> fileMapping = new Dictionary<string, string>(commitOnHEAD.FileMapping);

        // Invariant: COMMIT.FileMapping contains the mapping of file names to blobs.
        
        foreach (var keyValuePair in stagingArea.GetStagingForAddition())
        {
            fileMapping[keyValuePair.Key] = keyValuePair.Value;
        }
        
        foreach (string file in stagingArea.GetStagingForRemoval())
        {
            fileMapping.Remove(file);
        }
        
        // Clear the staging area then save
        stagingArea.Clear();
        stagingArea.Save();
        
        // Create and save the new commit
        string parent = Gitlite.Commit.GetHeadCommitId();
        string hashRef = Gitlite.Commit.CreateCommit(logMessage, DateTime.Now, fileMapping, parent);
        
        // Update branch pointer, only if we are in a branch.
        string branch = Gitlite.Branch.GetActiveBranch();
        if (branch != null)
        {
            Utils.WriteContent(Path.Combine(BRANCHES.ToString(), branch), hashRef);
        }
    }

    /// <summary>
    /// Removes the file from staging or from being tracked.
    /// </summary>
    /// <param name="fileName">File to be removed.</param>
    public static void Rm(string fileName)
    {
        // Check the staging area
        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();
        Commit commitOnHEAD = Gitlite.Commit.GetHeadCommit();

        if (stagingArea.GetStagingForAddition().ContainsKey(fileName))
        {
            stagingArea.GetStagingForAddition().Remove(fileName);
        } 
        else if (commitOnHEAD.FileMapping.ContainsKey(fileName))
        {
            stagingArea.GetStagingForRemoval().Add(fileName);
            File.Delete(Path.Combine(CWD.ToString(), fileName));
        }
        else
        {
            Utils.ExitWithError($"No reason to remove the file: {fileName}");
        }
        
        stagingArea.Save();
    }

    /// <summary>
    /// Displays the commit history from the HEAD commit backwards until the initial
    /// commit.
    /// </summary>
    public static void Log()
    {
        Commit commit = Gitlite.Commit.GetHeadCommit();
        
        while (true)
        {
            
            /*
             * TODO: Merge case
             * I think we just need to print out the second parent after merging and
             * this should be updated in the Commit.ToString() method.
             */
            
            Console.WriteLine("===");
            Console.WriteLine(commit?.ToString());
            Console.WriteLine();

            if (commit?.ParentHashRef == null)
            {
                return;
            }
            
            // Get the parent commit
            Commit? parent = Gitlite.Commit.Deserialize(commit.ParentHashRef);
            commit = parent;
        }
    }

    /// <summary>
    /// Prints out all the commit in no order.
    /// </summary>
    public static void GlobalLog()
    {

        string[] commitDirs = Directory.GetDirectories(COMMITS_DIR.ToString());
        foreach (var dir in commitDirs)
        {
            foreach (string commitHash in Directory.GetFiles(dir).Select(Path.GetFileName))
            {
                Commit commit = Gitlite.Commit.Deserialize(Path.GetFileName(dir) + commitHash);
                Console.WriteLine("===");
                Console.WriteLine(commit);
            }

            Directory.GetFiles(dir);
               
        }
    }

    /// <summary>
    /// Prints out the details of all commits that have the given COMMIT MESSAGE.
    /// </summary>
    /// <param name="commitMessage">The commit message of the commit being looked for.</param>
    public static void Find(string commitMessage)
    {
        string[] dirs = Directory.GetDirectories(COMMITS_DIR.ToString());
        bool matchFound = false;

        foreach (var dir in dirs)
        {
            foreach (var commitHashRef in Directory.GetFiles(dir).Select(Path.GetFileName))
            {
                Commit  commit = Gitlite.Commit.Deserialize(Path.GetFileName(dir) + commitHashRef);

                if (commit.LogMessage.ToLower().Contains(commitMessage.ToLower()))
                {
                    Console.WriteLine("===");
                    Console.WriteLine(commit);
                    matchFound = true;
                }
            }
        }
        
        if (!matchFound)
        {
            Console.WriteLine("Found no commit with that message.");
        }
    }

    /// <summary>
    /// Displays the current status of the Gitlite repository.
    /// This includes branches and active branch, staged and removed files,
    /// as well as modified and untracked files in the working directory.
    /// </summary>
    public static void Status()
    {
        List<string> notStagedAndModified = new List<string>();
        List<string> untrackedFiles = new List<string>();
        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();
        Dictionary<string, string> stagedFiles = stagingArea.GetStagingForAddition();
        Commit currentCommit = Gitlite.Commit.GetHeadCommit();
        
        
        foreach (var file in Directory.GetFiles(CWD.ToString()))
        { // Checkpoint: Doing untracked and modified case 1
            string filename = Path.GetFileName(file);
            
            if (filename == "Gitlite") continue;
            
            if (!stagedFiles.ContainsKey(filename))
            {
                // If a file is not staged and not tracked in the commit,
                // then it's untracked.
                if (!currentCommit.FileMapping.ContainsKey(filename))
                {
                    untrackedFiles.Add(filename);
                }
                else if (currentCommit.FileMapping.ContainsKey(filename))
                {
                    if (!Blob.IsEqualToOtherFile(currentCommit.FileMapping[filename], file)) 
                    {
                        Console.WriteLine(filename);
                        notStagedAndModified.Add(filename + " (modified)");
                    }
                }
            }
        }

        foreach (var file in currentCommit.FileMapping)
        {
            if (!File.Exists(Path.Combine(CWD.ToString(), file.Key)) && !stagingArea.GetStagingForRemoval().Contains(file.Key))
            {
                notStagedAndModified.Add(file.Key + " (deleted)");
            }
        }
        
        // Display all branches
        Console.WriteLine("=== Branches ===");
        foreach (var branch in Gitlite.Branch.GetExistingBranches())
        {
            if (Gitlite.Branch.IsCurrentBranch(branch))
            {
                Console.WriteLine("*" + branch);
            } else Console.WriteLine(branch);
        }
        
        Console.WriteLine("\n=== Staged Files ===");
        foreach (var file in stagedFiles.OrderBy(f => f.Key))
        {

            // Modified: Case 3
            if (!File.Exists(Path.Combine(CWD.ToString(), file.Key)))
            {
                notStagedAndModified.Add(file.Key + " (deleted)");
            }
            // Modified: Case 2
            else if (!stagingArea.IsStagedFileEqualToOtherFile(file.Value, file.Key))
            {
                notStagedAndModified.Add(file.Key + " (modified stg)");
            }
            else
            {
                Console.WriteLine(file.Key);
            }
        }

        Console.WriteLine("\n=== Removed Files ===");
        foreach (var file in stagingArea.GetStagingForRemoval().OrderBy(f => f))
        {
            Console.WriteLine(file);
        }

        Console.WriteLine("\n=== Modifications Not Staged For Commit ===");
        foreach (var file in notStagedAndModified.OrderBy(f => f))
        {
            Console.WriteLine(file);
        }

        Console.WriteLine("\n=== Untracked Files ===");
        foreach (var file in untrackedFiles.OrderBy(f => f))
        {
            Console.WriteLine(file);
        }
    }

    /// <summary>
    /// Has three possible use cases:
    /// 1. checkout -- [filename]
    ///         - checks out a file from the head commit, overwriting the one in the
    ///           working directory.
    /// 2. checkout [commit id] -- [filename]
    ///         - checks out a file from a commit given its id, overwriting the one in the
    ///           working directory.
    /// 3. checkout [branch name]
    ///         - checks out all file from the commit at the head of the given branch,
    ///           overwriting the files that are already there and deleting the files not
    ///           in the head of the checked-out branch.
    ///                           
    /// </summary>
    /// <param name="args"></param>
    public static void Checkout(string[] args)
    {
        if (args.Length == 3)
        { // Checkout a file from the current commit
            ValidateCheckoutSeparator(args, 1);
            CheckoutWithFileAndCommit(args[2]);
        } else if (args.Length == 4)
        { // Checkout a file from the given commit id
            ValidateCheckoutSeparator(args, 2);
            CheckoutWithFileAndCommit(args[3], args[1]);
        } else if (args.Length == 2)
        { // Checkout all files from the given branch
            CheckoutWithBranch(args[1]);
        }
        else
        {
            Utils.ExitWithError("Invalid number of arguments for checkout.");
        }
    }

    /// <summary>
    /// Creates a Gitlite branch with the given name.
    /// </summary>
    /// <param name="branchName">Name of the branch to be created.</param>
    public static void Branch(string branchName)
    {
        if (File.Exists(Path.Combine(BRANCHES.ToString(), branchName)))
        {
            Utils.ExitWithError("A branch with that name already exists.");
        }
        
        Gitlite.Branch.CreateBranch(branchName, Gitlite.Commit.GetHeadCommitId());
    }

    /// <summary>
    /// Deletes a branch given its name. A branch really is just a pointer to a commit node
    /// and deleting a branch will just remove this pointer. It does not delete all the commits
    /// created while in or under the branch.
    /// </summary>
    /// <param name="branchName">Name of the branch to delete.</param>
    public static void RmBranch(string branchName)
    {
        string path = Path.Combine(BRANCHES.ToString(), branchName);
        if (!File.Exists(path))
        {
            Utils.ExitWithError("A branch with that name does not exist.");
        } else if (Gitlite.Branch.IsCurrentBranch(branchName))
        {
            Utils.ExitWithError("Cannot remove the current branch.");
        }
        
        File.Delete(path);
    }
    

    private static void ValidateCheckoutSeparator(string[] args, int index)
    {
        if (args[index] != "--")
        {
            Utils.ExitWithError("Invalid arguments specified.");
        }
    }

    /// <summary>
    /// Takes the version of the file in the head commit and puts it in the working directory,
    /// overwriting the file there if it exists. Does not stage the new file.
    /// </summary>
    /// <param name="commitId">Commit id of the commit to checkout from.</param>
    /// <param name="filename">Name of the file.</param>
    private static void CheckoutWithFileAndCommit(string filename, string commitId = "")
    {
        Commit commit;
        if (commitId == "")
        {
            commit = Gitlite.Commit.GetHeadCommit();    
        }
        else
        {
            commit = Gitlite.Commit.Deserialize(commitId, completeForm:false, "No commit with that id exists.");
        }
        
        if (!commit.FileMapping.ContainsKey(filename))
        {
            Utils.ExitWithError("File does not exist in that commit.");
        }
        
        Console.WriteLine(commit.Hash);
        
        string fileContentInHeadCommit = Blob.ReadBlobContentAsString(commit.FileMapping[filename]);
        Utils.WriteContent(filename, fileContentInHeadCommit);
    }

    private static void CheckoutWithBranch(string branchName)
    {
        string branchPath = Path.Combine(BRANCHES.ToString(), branchName);
        if (!File.Exists(branchPath))
        {
            Utils.ExitWithError("No such branch exists.");
        } else if (Gitlite.Branch.IsCurrentBranch(branchName))
        {
            Utils.ExitWithError("No need to checkout the current branch.");
        }
        
        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();
        Commit headCommit = Gitlite.Commit.GetHeadCommit();
        
        // latest commit in the branch to checkout
        Commit branchToCheckout = Gitlite.Commit.Deserialize(Utils.ReadContentsAsString(branchPath));
        
        foreach (var file in Directory.GetFiles(CWD.ToString()).Select(Path.GetFileName))
        {   // An untracked file exists in the current branch.
            if (!stagingArea.GetStagingForAddition().ContainsKey(file) && !headCommit.FileMapping.ContainsKey(file) && file != "Gitlite")
            {
                Utils.ExitWithError("There is an untracked file in the way; delete it, or add and commit it first.");
            }
        }
        
        // Writing files from the checked-out branch commit to the working directory 
        foreach (var file in branchToCheckout.FileMapping)
        {
            string content = Blob.ReadBlobContentAsString(file.Value);
            Utils.WriteContent(Path.Combine(CWD.ToString(), file.Key), content);
        }
        
        // Removing files not present in the checked-out branch commit
        foreach (var file in Directory.GetFiles(CWD.ToString()).Select(Path.GetFileName))
        {
            if (!branchToCheckout.FileMapping.ContainsKey(file) && file != "Gitlite")
            {
                File.Delete(file);
            }
        }
        
        // Update HEAD pointer and clear the staging area
        Utils.WriteContent(Path.Combine(GITLITE_DIR.ToString(), "HEAD"), $"ref: {branchName}");
        stagingArea.Clear();
    }

    /// <summary>
    /// Creates the directory structures inside the .gitlite directory
    /// </summary>
    private static void CreateDirs()
    {
        GITLITE_DIR.Create();
        COMMITS_DIR.Create();
        BLOBS_DIR.Create();
        BRANCHES.Create();
        StagingArea.CreateStagingArea();
    }

    /// <summary>
    /// Checks if GitLite has already been initialized.
    /// </summary>
    /// <returns>true if already initialized otherwise false</returns>
    public static bool GitliteAlreadyInitialized()
    {
        return GITLITE_DIR.Exists;
    }
}