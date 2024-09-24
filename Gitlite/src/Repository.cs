using MessagePack;

namespace GitLite;

/// <summary>
/// Class representing a GitLite repository.
/// </summary>
    public class Repository
{
    
    private static DirectoryInfo CWD = Directory.GetParent(AppContext.BaseDirectory);
    public static DirectoryInfo GITLITE_DIR = Utils.JoinDirectory(CWD, ".gitlite");
    public static DirectoryInfo COMMITS_DIR = Utils.JoinDirectory(GITLITE_DIR, "commits");
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
        
        // Creates the directory structure inside .gitlite
        CreateDirs();
        
        string hash = Commit.CreateInitialCommit();
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
        Dictionary<string, byte[]> forAddition = stagingArea.GetStagingForAddition();

        // using (FileStream file = File.Open(fileName, FileMode.Open))
        // {
        //     // Serializing is kind of useless i Guess
        //     // byte[] serializedFile = MessagePackSerializer.Serialize(file.ReadByte());
        //     
        //     if (!forAddition.ContainsKey(fileName))
        //     {
        //         forAddition.Add(fileName, File.ReadAllBytes(fileName));
        //         Console.WriteLine("Not yet staged");
        //     }
        //
        //     byte[] sameFileFromStagingArea = forAddition[fileName];
        //
        //     if (!serializedFile.SequenceEqual(sameFileFromStagingArea))
        //     {
        //         forAddition[fileName] = serializedFile;
        //         Console.WriteLine("Different file");
        //     }
        // }

        byte[] fileInByte = File.ReadAllBytes(fileName);
        
        if (!forAddition.ContainsKey(fileName))
        {
            forAddition.Add(fileName, File.ReadAllBytes(fileName));
            Console.WriteLine("Not yet staged");
        }
        
        byte[] sameFileFromStagingArea = forAddition[fileName];
        
        if (!fileInByte.SequenceEqual(sameFileFromStagingArea))
        {
            forAddition[fileName] = fileInByte;
            Console.WriteLine("Different file");
        }

        byte[] reserializedStagingArea = MessagePackSerializer.Serialize(stagingArea);
        Utils.WriteContent(StagingArea.STAGING_AREA, reserializedStagingArea);
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