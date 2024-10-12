namespace Gitlite;

/*
 * TODO: Gitlite status
 * TODO: Refactor overloaded methods in Commit
 */

/// <summary>
/// Class representing a GitLite repository.
/// </summary>
public class Repository
{
    
    private static DirectoryInfo CWD = Directory.GetParent(AppContext.BaseDirectory);
    public static DirectoryInfo GITLITE_DIR = Utils.JoinDirectory(CWD, ".gitlite");
    public static DirectoryInfo COMMITS_DIR = Utils.JoinDirectory(GITLITE_DIR, "commits");
    public static DirectoryInfo BLOBS_DIR = Utils.JoinDirectory(GITLITE_DIR, "blobs");
    private static DirectoryInfo BRANCHES = Utils.JoinDirectory(GITLITE_DIR, "branches");
    
    
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
        CreateBranch("master", hash);
        Utils.WriteContent(Path.Combine(GITLITE_DIR.ToString(), "HEAD"), hash);
        
        Console.WriteLine($"Initialized a new GitLite at {CWD.ToString()}");
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
        Blob.SaveAsBlob(content);
        stagingArea.Save();
    }

    public static void Commit(string logMessage)
    {
        // Get staging area
        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();

        // Check for failure cases
        if (!stagingArea.IsThereStagedFiles())
        {
            Utils.ExitWithError("No changes added to the commit.");
        }
        
        Dictionary<string, string> forAddition = stagingArea.GetStagingForAddition();
        List<string> forRemoval = stagingArea.GetStagingForRemoval();
        Commit commitOnHEAD = Gitlite.Commit.GetHeadCommit();
        Dictionary<string, string> fileMapping = commitOnHEAD.FileMapping;

        // Invariant: COMMIT.FileMapping contains the mapping of file names to blobs.
        
        foreach (var keyValuePair in forAddition)
        {
            fileMapping[keyValuePair.Key] = keyValuePair.Value;
        }
        
        foreach (string file in forRemoval)
        {
            fileMapping.Remove(file);
        }
        
        // Clear the staging area then save
        forAddition.Clear();
        forRemoval.Clear();
        stagingArea.Save();
        
        // Create and save the new commit
        string parent = Utils.ReadContentsAsString(GITLITE_DIR.ToString(), "HEAD");
        string hashRef = Gitlite.Commit.CreateCommit(logMessage, DateTime.Now, fileMapping, commitOnHEAD.Branch, parent);
        
        // Update HEAD and branch pointer
        Utils.WriteContent(Path.Combine(GITLITE_DIR.ToString(), "HEAD"), hashRef);
        Utils.WriteContent(Path.Combine(BRANCHES.ToString(), commitOnHEAD.Branch), hashRef);
        
    }

    /// <summary>
    /// Removes the file from staging or from being tracked.
    /// </summary>
    /// <param name="fileName">File to be removed.</param>
    public static void Rm(string fileName)
    {
        // Check the staging area
        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();
        Dictionary<string, string> forAddition = stagingArea.GetStagingForAddition();
        List<string> forRemoval = stagingArea.GetStagingForRemoval();
        Commit commitOnHEAD = Gitlite.Commit.GetHeadCommit();

        if (forAddition.ContainsKey(fileName))
        {
            forAddition.Remove(fileName);
        } 
        else if (commitOnHEAD.FileMapping.ContainsKey(fileName))
        {
            forRemoval.Add(fileName);
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
            Commit? parent = Gitlite.Commit.Deserialize(COMMITS_DIR.ToString(), commit.ParentHashRef);
            commit = parent;
        }
    }

    /// <summary>
    /// Prints out all the commit in no order.
    /// </summary>
    public static void GlobalLog()
    {
        string[] commits = Directory.GetFiles(COMMITS_DIR.ToString());
        foreach (string commitHash in commits)
        {
            Commit commit = Gitlite.Commit.Deserialize(commitHash);
            Console.WriteLine("===");
            Console.WriteLine(commit);
        }
    }

    /// <summary>
    /// Prints out the details of all commits that have the given COMMIT MESSAGE.
    /// </summary>
    /// <param name="commitMessage">The commit message of the commit being looked for.</param>
    public static void Find(string commitMessage)
    {
        string[] commitHashRefs = Directory.GetFiles(COMMITS_DIR.ToString());
        bool matchingCommitFound = false;
        
        foreach (string commitRef in commitHashRefs)
        {
            Commit deserializedCommit = Gitlite.Commit.Deserialize(commitRef);
            if (deserializedCommit?.LogMessage.ToLower().Contains(commitMessage.ToLower()) == true)
            {
                Console.WriteLine("===");
                Console.WriteLine(deserializedCommit);
                matchingCommitFound = true;
            }
        }

        if (!matchingCommitFound)
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
        string[] branches = GetExistingBranches();
        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();
        Dictionary<string, string> stagedFiles = stagingArea.GetStagingForAddition();
        List<string> removedFiles = stagingArea.GetStagingForRemoval();
        string[] filesInCWD = Directory.GetFiles(CWD.ToString());
        Dictionary<string, string> filesInCurrentCommit = Gitlite.Commit.GetHeadCommit().FileMapping;
        List<string> untrackedFiles = new List<string>();
        List<string> modifiedButNotStaged = new List<string>();
               
        foreach (string file in filesInCWD)
        {
            if (!filesInCurrentCommit.ContainsKey(file) && !stagedFiles.ContainsKey(file))
            {
                untrackedFiles.Add(file);
            } else if (IsModifiedButNotStaged(file,
                           filesInCurrentCommit,
                           stagedFiles,
                           removedFiles,
                           stagingArea))
            {
                modifiedButNotStaged.Add(file);
            }
        }

        foreach (var VARIABLE in COLLECTION)
        {
            
        }
        
        
    }

    private static bool IsModifiedButNotStaged(string file,
        Dictionary<string, string> commitFileMapping,
        Dictionary<string, string> stagedFiles,
        List<string> removedFiles,
        StagingArea stagingArea)
    {
        /*
         * A file in the working directory is “modified but not staged” if it is
           
           Tracked in the current commit, changed in the working directory, but not staged; or
           Staged for addition, but with different contents than in the working directory; or
           Staged for addition, but deleted in the working directory; or
           Not staged for removal, but tracked in the current commit and deleted from the working directory.
         */

        if (commitFileMapping.ContainsKey(file))
        {
            if (!Blob.CompareToOtherFile(commitFileMapping[file], file) && !stagedFiles.ContainsKey(file))
            {
                return true;
            }

            if (!removedFiles.Contains(file) && commitFileMapping.ContainsKey(file) &&
                !File.Exists(Path.Combine(CWD.ToString(), file)))
            {
                return true;
            }

            return false;
        }

        if (stagedFiles.ContainsKey(file) && stagingArea.CompareStagedFileToOtherFile(stagedFiles[file], file))
        {
            return true;
        }

        if (stagedFiles.ContainsKey(file) && !File.Exists(Path.Combine(CWD.ToString(), file)))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Helper function that gets all existing branches and marks the active
    /// branch with '*'
    /// </summary>
    /// <returns>A string[] of all branches</returns>
    private static string[] GetExistingBranches()
    {
        string[] branches = Utils.GetFilesSorted(BRANCHES.ToString());
        for (int i = 0; i < branches.Length; i++)
        {
            if (IsCurrentBranch(branches[i]))
            {
                branches[i] = "*" + branches[i];
            }
        }

        return branches;
    }

    /// <summary>
    /// Checks if given BRANCH is the active branch
    /// </summary>
    /// <param name="branch">Branch name</param>
    /// <returns>A boolean value if BRANCH is active</returns>
    private static bool IsCurrentBranch(string branch)
    {
        string HEADhash = Utils.ReadContentsAsString(GITLITE_DIR.ToString(), "HEAD");
        string branchHash = Utils.ReadContentsAsString(BRANCHES.ToString(), branch);

        return HEADhash == branchHash;
    }

    /// <summary>
    /// Creates a GitLite branch.
    /// </summary>
    /// <param name="name">Name of the branch</param>
    /// <param name="commitHashRef">Commit hash reference that the branch points to</param>
    private static void CreateBranch(string name, string commitHashRef)
    {
        string branch = Path.Combine(BRANCHES.ToString(), name);
        Utils.WriteContent(branch, commitHashRef);
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