namespace GitLite;

/// <summary>
/// Class representing a GitLite repository.
/// </summary>
    public class Repository
{
    
    private static DirectoryInfo CWD = Directory.GetParent(AppContext.BaseDirectory);
    private static DirectoryInfo GITLITE_DIR = Utils.JoinDirectory(CWD, ".gitlite");
    private static DirectoryInfo COMMITS_DIR = Utils.JoinDirectory(GITLITE_DIR, "commits");
    private static DirectoryInfo BLOBS_DIR = Utils.JoinDirectory(GITLITE_DIR, "blobs");
    private static DirectoryInfo BRANCHES = Utils.JoinDirectory(GITLITE_DIR, "branches");
    
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
        
        // Creates the directory structures inside .gitlite
        CreateDirs();
        
        string hash = CreateCommit("initial commit");
        CreateBranch("master", hash);
        Utils.WriteContent(Path.Combine(GITLITE_DIR.ToString(), "HEAD"), hash);
        
        Console.WriteLine($"Initialized a new GitLite at {CWD.ToString()}");
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
    /// Creates the directory structures inside of .gitlite directory
    /// </summary>
    private static void CreateDirs()
    {
        GITLITE_DIR.Create();
        COMMITS_DIR.Create();
        BLOBS_DIR.Create();
        BRANCHES.Create();
    }

    /// <summary>
    /// Creates a Commit object with LOG MESSAGE and returns its hash.
    /// </summary>
    /// <param name="logMessage">Commit log message</param>
    /// <returns>Hash string of the created Commit object.</returns>
    private static string CreateCommit(string logMessage)
    {
        Commit initialCommit = Commit.CreateCommit(logMessage);
        string hash = Commit.GetHash(initialCommit);
        File.Create(Path.Combine(COMMITS_DIR.ToString(), hash));
        return hash;
    }

    /// <summary>
    /// Checks if GitLite has already been initialized.
    /// </summary>
    /// <returns>true if already initialized otherwise false</returns>
    private static bool GitliteAlreadyInitialized()
    {
        return GITLITE_DIR.Exists;
    }
}