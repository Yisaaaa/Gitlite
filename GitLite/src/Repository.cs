namespace GitLite;

public class Repository
{
    
    private static DirectoryInfo CWD = Directory.GetParent(AppContext.BaseDirectory);
    private static DirectoryInfo GITLITE_DIR = Utils.JoinDirectory(CWD, ".gitlite");
    /// <summary>
    /// This initializes the GitLite repository on the Current Working Directory.
    /// </summary>
    public static void Init()
    {
        // TODO: Include other folders such as commits, blobs, etc.
        
        GITLITE_DIR.Create();
        Console.WriteLine($"Initialized GitLite at {CWD.ToString()}");
    } 
}