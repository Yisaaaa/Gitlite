using System.Security.Cryptography;
using MessagePack;
    
namespace Gitlite;

/*
 * TODO: Refactor CreateCommit method and reduce overloading
 * TODO: by using optional arguments instead.
 * 
 */

/// <summary>
/// Class that represents GitLite commits.
/// </summary>
[MessagePackObject]
public class Commit
{
    [Key(0)] public string LogMessage { get; set; }
    [Key(1)] public DateTime Timestamp { get; set; }
    [Key(2)] public Dictionary<string, string> FileMapping { get; set; }
    [Key(3)] public string Branch { get; set; }
    [Key(4)] public string? ParentHashRef { get; set; }
    [Key(5)] public string Hash { get; set; }

    public Commit(string logMessage, DateTime timestamp, Dictionary<string, string> fileMapping, string branch, string? parentHashRef)
    {
        this.LogMessage = logMessage;
        this.Timestamp = timestamp;
        this.FileMapping = fileMapping;
        this.Branch = branch;
        this.ParentHashRef = parentHashRef;
    }

    public static string GetHash(Commit commit)
    {
        byte[] bytes = MessagePackSerializer.Serialize(commit);
        return Utils.HashBytes(bytes);
    }
    public static string CreateCommit(string logMessage, string? parentHashRef)
    {
        DateTime timestamp = DateTime.Now;
        Dictionary<string, string> fileMapping = new Dictionary<string, string>();
        return CreateCommit(logMessage, timestamp, fileMapping, "master", parentHashRef);
    }

    /// <summary>
    /// Creates a Commit object with LOG MESSAGE and returns its hash.
    /// </summary>
    /// <param name="logMessage">Commit log message.</param>
    /// <param name="timestamp">Timestamp of when the commit was created.</param>
    /// <param name="parentHashRef">Hash reference of parent commit.</param>
    /// <returns>Hash string of the created Commit object.</returns>
    public static string CreateCommit(string logMessage, DateTime timestamp, string? parentHashRef)
    {
        Dictionary<string, string> fileMapping = new Dictionary<string, string>();
        return CreateCommit(logMessage, timestamp, fileMapping, "master", parentHashRef);
    }
    
    public static string CreateCommit(string logMessage, DateTime timestamp, Dictionary<string, string> fileMapping, string branch, string? parentHashRef)
    {
        Commit commit = new Commit(logMessage, timestamp, fileMapping, branch, parentHashRef);
        string hash = GetHash(commit);
        commit.Hash = hash;
        byte[] serializedCommit = MessagePackSerializer.Serialize(commit);
        commit.WriteCommit(serializedCommit);
        return hash;
    }

    private void WriteCommit(byte[] serializedCommit)
    {
        var (firstTwoDigits, rest) = Utils.SplitHashPath(Hash);
        string path = Path.Combine(Repository.COMMITS_DIR.ToString(), firstTwoDigits);
        Directory.CreateDirectory(path);
        path = Path.Combine(path, rest);
        Utils.WriteContent(path, serializedCommit);
    }
    
    public override string ToString()
    {
        return $"Commit: {Hash}\nDate: {Timestamp}\n{LogMessage}";
    }
    
    public static string CreateInitialCommit()
    {
        DateTime unixEpoch = DateTime.UnixEpoch;
        return CreateCommit("initial commit", unixEpoch, null);
    }

    public static Commit Deserialize(string hash, bool completeForm = true, string? errorMessage = null)
    {
        string path;

        if (!completeForm)
        {
            hash = FindCompleteHash(hash);
            if (hash == null)
            {
                Utils.ExitWithError(errorMessage);
            }
        }
            
        var (firstTwoDigits, rest) = Utils.SplitHashPath(hash);
        path = Path.Combine(Repository.COMMITS_DIR.ToString(), firstTwoDigits, rest);
        
        Utils.ValidateFile(path, message:errorMessage);
        byte[] commitAsByte = Utils.ReadContentsAsBytes(path);
        
        return MessagePackSerializer.Deserialize<Commit>(commitAsByte);
    }

    private static string? FindCompleteHash(string shortHash)
    {
        if (shortHash.Length < 6)
        {
            Utils.ExitWithError("Short hash must at least be 6 characters long.");
        }
        
        var (firstTwoDigits, rest) = Utils.SplitHashPath(shortHash);
        string dirPath = Path.Combine(Repository.COMMITS_DIR.ToString(), firstTwoDigits);

        if (!Directory.Exists(dirPath)) return null;
        
        string[] files = Directory.GetFiles(dirPath);
        foreach (var file in files)
        {
            if (file.StartsWith(rest))
            {
                return file;
            }
        }

        return null;
    } 

    public static Commit GetHeadCommit()
    {
        string hashRef = Utils.ReadContentsAsString(Repository.GITLITE_DIR.ToString(), "HEAD");
        Commit commit = Deserialize(hashRef);
        return commit;
    }
    
}