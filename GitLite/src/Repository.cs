namespace GitLite;

/// <summary>
/// Class representing GitLite repository.
/// </summary>
public class Repository
{
    
    private static DirectoryInfo CWD = Directory.GetParent(AppContext.BaseDirectory);
    private static DirectoryInfo GITLITE_DIR = Utils.JoinDirectory(CWD, ".gitlite");
    private static DirectoryInfo COMMITS_DIR = Utils.JoinDirectory(GITLITE_DIR, "commits");
    private static DirectoryInfo BLOBS_DIR = Utils.JoinDirectory(GITLITE_DIR, "blobs");
    
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
        
        GITLITE_DIR.Create();
        COMMITS_DIR.Create();
        BLOBS_DIR.Create();
        
        Console.WriteLine($"Initialized a new GitLite at {CWD.ToString()}");
    }

    private static bool GitliteAlreadyInitialized()
    {
        return GITLITE_DIR.Exists;
    }
}