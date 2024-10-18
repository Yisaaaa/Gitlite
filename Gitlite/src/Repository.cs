namespace Gitlite;

/*
 * TODO: Refactor file structure. Adopt the hash table like datastructure
 * TODO: only that it is implemented with file system.
 * TODO (DONE): First thing to do is to update all things (e.g. methods) related to commit files.
 * TODO: Refactor checking if directory exists when writing files.
 * TODO: Update all things related to blobs
 * 
 * TODO: Update the design document brought by the changes of file structure refactoring.
 * TODO: Refactor overloaded methods in Commit.
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
        foreach (var branch in GetExistingBranches())
        {
            if (IsCurrentBranch(branch))
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
    /// Helper function that gets all existing branches and marks the active
    /// branch with '*'
    /// </summary>
    /// <returns>A string[] of all branches</returns>
    private static string[] GetExistingBranches()
    {
        string[] branches = Utils.GetFilesSorted(BRANCHES.ToString());
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