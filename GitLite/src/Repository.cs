namespace GitLite;

public class Repository
{
    
    private static DirectoryInfo CWD = Directory.GetParent(AppContext.BaseDirectory);
    
    /// <summary>
    /// This initializes the GitLite repository on the Current Working Directory.
    /// </summary>
    public static void Init()
    {
        Console.WriteLine($"Initialized GitLite at {CWD.ToString()}");
    } 
}