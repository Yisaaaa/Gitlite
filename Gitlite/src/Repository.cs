namespace Gitlite;

/*
 * TODO: Update the design document brought by the changes of file structure refactoring.
 * TODO: Refactor overloaded methods in Commit.
 * TODO: Refactors things on lingq and conditions with ternary if possible.
 *
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
        if (!stagingArea.HasStagedFiles())
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
    /// Takes the version of the file in the head commit and puts it in the working directory,
    /// overwriting the file there if it exists. Does not stage the new file.
    /// </summary>
    /// <param name="commitId">Commit id of the commit to checkout from.</param>
    /// <param name="filename">Name of the file.</param>
    private static void CheckoutWithFileAndCommit(string filename, string? commitId = null)
    {
        Commit commit;
        if (commitId == null)
        {
            commit = Gitlite.Commit.GetHeadCommit();    
        }
        else
        {
            commit = Gitlite.Commit.Deserialize(commitId, "No commit with that id exists.");
        }
        
        if (!commit.FileMapping.ContainsKey(filename))
        {
            Utils.ExitWithError("File does not exist in that commit.");
        }
        
        string fileContentInHeadCommit = Blob.ReadBlobContentAsString(commit.FileMapping[filename]);
        Utils.WriteContent(filename, fileContentInHeadCommit);
    }

    private static void CheckoutWithBranch(string branchName)
    {
        string branchPath = Path.Combine(BRANCHES.ToString(), branchName);
        if (!Gitlite.Branch.Exists(branchPath))
        {
            Utils.ExitWithError("No such branch exists.");
        } else if (Gitlite.Branch.IsCurrentBranch(branchName))
        {
            Utils.ExitWithError("No need to checkout the current branch.");
        }
        
        // latest commit in the branch to checkout
        Commit branchToCheckout = Gitlite.Commit.Deserialize(Utils.ReadContentsAsString(branchPath));
        Commit currentHeadCommit = Gitlite.Commit.GetHeadCommit();
        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();
        List<string> untrackedFiles = GetUntrackedFiles(stagingArea, currentHeadCommit);
        
        // Checking if there is an untracked that would get overwritten as a result of checkout
        CheckUntrackedOverwrite(branchToCheckout.FileMapping, untrackedFiles);
        
        // Checkout all files from the head of the given branch
        CheckoutAllFilesWithCommit(branchToCheckout, untrackedFiles, stagingArea);
        
        // Update HEAD
        Utils.WriteContent(Path.Combine(GITLITE_DIR.ToString(), "HEAD"), $"ref: {branchName}");
    }

    private static void CheckoutAllFilesWithCommit(Commit commit, List<string> excludedFiles, StagingArea stagingArea)
    {
        // Writing files from the checked-out branch commit to the working directory 
        foreach (var file in commit.FileMapping)
        {
            string content = Blob.ReadBlobContentAsString(file.Value);
            Utils.WriteContent(Path.Combine(CWD.ToString(), file.Key), content);
        }
        
        // Removing files not present in the checked-out branch commit
        RemoveFilesNotInFileMappingInCwd(commit.FileMapping, excludedFiles);
        
        // Clear the staging area
        stagingArea.Clear();
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
        if (!Gitlite.Branch.Exists(path))
        {
            Utils.ExitWithError("A branch with that name does not exist.");
        } else if (Gitlite.Branch.IsCurrentBranch(branchName))
        {
            Utils.ExitWithError("Cannot remove the current branch.");
        }
        
        File.Delete(path);
    }
    
    public static void Reset(string commitId)
    {
        string completeCommitId = Gitlite.Commit.FindCompleteHash(commitId);
        Commit commit = Gitlite.Commit.Deserialize(completeCommitId, "No commit with that id exists.");
        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();
        Commit currentHeadCommit = Gitlite.Commit.GetHeadCommit();
        List<string> untrackedFiles = GetUntrackedFiles(stagingArea, currentHeadCommit);   
        
        // Check if there's an untracked overwrite conflict error
        CheckUntrackedOverwrite(commit.FileMapping, untrackedFiles);
        
        // Checkout all files if there are no untracked files overwrite conflict.
        CheckoutAllFilesWithCommit(commit, untrackedFiles, stagingArea);
        
        // Update branch pointer
        string branch = Gitlite.Branch.GetActiveBranch() ?? throw new InvalidOperationException("Not in a branch.");
        Utils.WriteContent(Path.Combine(BRANCHES.ToString(), branch), completeCommitId);
    }
    
    public static void Merge(string branchName)
    {
        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();
        
        if (stagingArea.HasStagedFiles())
        {
            Utils.ExitWithError("You have uncommitted changes.");
        } else if (!Gitlite.Branch.Exists(branchName))
        {
            Utils.ExitWithError("A branch with that name does not exist.");
        } else if (Gitlite.Branch.IsCurrentBranch(branchName))
        {
            Utils.ExitWithError("Cannot merge a branch with itself.");
        }
        
        List<string> untrackedFiles = GetUntrackedFiles(stagingArea);
        Commit currBranchHead = Gitlite.Commit.GetHeadCommit();
        
        
    }
    
    private static void ValidateCheckoutSeparator(string[] args, int index)
    {
        if (args[index] != "--")
        {
            Utils.ExitWithError("Invalid arguments specified.");
        }
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

    /// <summary>
    /// Gets the untracked files in the working directory.
    /// </summary>
    /// <param name="stagingArea">Staging area object.</param>
    /// <param name="head">HEAD commit object.</param>
    /// <returns>A list of untracked files if there's any. Returns null otherwise</returns>
    private static List<string> GetUntrackedFiles(StagingArea? stagingArea = null, Commit? head = null)
    {
        if (stagingArea == null)
        {
            stagingArea = StagingArea.GetDeserializedStagingArea();
        }

        if (head == null)
        {
            head = Gitlite.Commit.GetHeadCommit();
        }

        List<string> files = new List<string>();
        
        foreach (var file in Directory.GetFiles(CWD.ToString()).Select(Path.GetFileName))
        {   // An untracked file exists in the current branch.
            if (IsFileUntracked(file, stagingArea, head))
            {
                files.Add(file);
            }
        }

        return files;
    }

    private static bool IsFileUntracked(string file, StagingArea stagingArea, Commit currHeadCommit)
    {
        return !stagingArea.GetStagingForAddition().ContainsKey(file) && !currHeadCommit.FileMapping.ContainsKey(file);
    }

    /// <summary>
    /// A helper method that removes all files in the CWD that are not in the given
    /// FILE MAPPING.
    /// </summary>
    /// <param name="fileMapping">File mapping of files to not be removed in the CWD.</param>
    private static void RemoveFilesNotInFileMappingInCwd(Dictionary<string, string> fileMapping, List<string> excludedFiles)
    {
        excludedFiles.Add("Gitlite");
        foreach (var file in Directory.GetFiles(CWD.ToString()).Select(Path.GetFileName))
        {
            if (!fileMapping.ContainsKey(file) && !excludedFiles.Contains(file))
            {
                File.Delete(file);
            }
        }
    }

    /// <summary>
    /// Given a list of files and a dictionary of file mapping, checks if a file from files
    /// exists in the file mapping.
    /// </summary>
    /// <param name="fileMapping"></param>
    /// <param name="files"></param>
    /// <returns>True if a file from files is in file mapping</returns>
    private static bool FileExistsInFileMapping(Dictionary<string, string> fileMapping,
        List<string> files) => files.Any(fileMapping.ContainsKey);


    // /// <summary>
    // /// Validates if there's going to be an untracked file conflict. This means
    // /// an untracked file would get overwritten because of some operation
    // /// (e.g. checkout or merge).
    // /// </summary>
    // /// <param name="headCommit">HEAD commit of the current branch.</param>
    // /// <param name="stagingArea">Staging area of the gitlite repository.</param>
    // private static void CheckOverwriteConflict(Commit headCommit, StagingArea stagingArea)
    // {
    //     List<string> untrackedFiles = GetUntrackedFiles(stagingArea, headCommit);
    //     
    //     if (untrackedFiles.Count > 0 && FileExistsInFileMapping(headCommit.FileMapping, untrackedFiles))
    //     {
    //         Utils.ExitWithError("There is an untracked file in the way; delete it, or add and commit it first.");
    //     }
    // }

    private static void CheckUntrackedOverwrite(Dictionary<string, string> fileMapping, List<string> untrackedFiles)
    {
        if (untrackedFiles.Count > 0 && FileExistsInFileMapping(fileMapping, untrackedFiles))
        {
            Utils.ExitWithError("There is an untracked file in the way; delete it, or add and commit it first.");
        } 
    }

    private static void CheckMergeUntrackedConflict(Commit givenBranchHead, Commit splitPoint)
    {
        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();
        Commit currentHeadCommit = Gitlite.Commit.GetHeadCommit();

        foreach (string file in GetUntrackedFiles(stagingArea, currentHeadCommit))
        {
            if ()
        }
    }
    
    /// <summary>
    /// Given two branches, finds the split point in the commit tree.
    /// THe split point of the two branches is their latest common ancestor in
    /// the commit tree.
    /// </summary>
    /// <param name="currentBranch">Current branch head.</param>
    /// <param name="givenBranch">Given branch head.</param>
    /// <returns></returns>
    private static Commit FindSplitPoint(Commit currentBranch, Commit givenBranch) 
    {
        // Store all the commits of one of the branches in a hashset.
        // Doesn't matter which. Here I am choosing the given branch.
        // Since we are mainly concerned in lookups, hashset is better
        // than a list.
        
        
        HashSet<Commit> givenBranchCommits = new HashSet<Commit>();
        Commit pointer = givenBranch;
        while (pointer.ParentHashRef != null)
        {
            givenBranchCommits.Add(pointer);
            pointer = Gitlite.Commit.Deserialize(pointer.ParentHashRef);
        }
        // This is the initial commit. Remember, the initial commit is always
        // created when we initialize a gitlite repo.
        givenBranchCommits.Add(pointer);
        
        // Now we can traverse the other branch from the latest commit backwards
        // and see if there exists a (latest) common ancestor.
        Commit splitPoint;
        pointer = currentBranch;
        while (pointer.ParentHashRef != null)
        {
            if (givenBranchCommits.Contains(pointer))
            {
                splitPoint = pointer;
            }
        }
        // Otherwise, we reach the initial commit here. Since we know for a fact
        // that all commits share the initial commit. If we did not find
        // any common ancestor before that, it means the common ancestor is the initial
        // commit.
        splitPoint = pointer;

        return splitPoint;
    }
}
