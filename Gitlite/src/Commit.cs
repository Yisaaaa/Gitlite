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
    [Key(3)] public string? ParentHashRef { get; set; }
    [Key(4)] public string Hash { get; set; }

    public Commit(string logMessage, DateTime timestamp, Dictionary<string, string> fileMapping, string? parentHashRef)
    {
        this.LogMessage = logMessage;
        this.Timestamp = timestamp;
        this.FileMapping = fileMapping;
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
        return CreateCommit(logMessage, timestamp, fileMapping, parentHashRef);
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
        return CreateCommit(logMessage, timestamp, fileMapping, parentHashRef);
    }
    
    public static string CreateCommit(string logMessage, DateTime timestamp, Dictionary<string, string> fileMapping, string? parentHashRef)
    {
        Commit commit = new Commit(logMessage, timestamp, fileMapping, parentHashRef);
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

    public static Commit Deserialize(string hash, string? errorMessage = null)
    {
        if (string.IsNullOrEmpty(hash))
        {
            throw new ArgumentNullException(nameof(hash), "Hash cannot be null or empty when deserializing.");
        }
        
        string path;
        
        if (hash.Length < 40)
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

    public static string? FindCompleteHash(string shortHash)
    {
        if (shortHash.Length < 5)
        {
            Utils.ExitWithError("Short hash must at least be 5 characters long.");
        } else if (shortHash.Length == 40)
        {
            return shortHash;
        }
        
        var (firstTwoDigits, rest) = Utils.SplitHashPath(shortHash);
        string dirPath = Path.Combine(Repository.COMMITS_DIR.ToString(), firstTwoDigits);

        if (!Directory.Exists(dirPath)) return null;
        
        var files = Directory.GetFiles(dirPath).Select(Path.GetFileName);
        List<string> matchingHashes = new List<string>();
        
        foreach (var file in files)
        {
            if (file.StartsWith(rest))
            {
                matchingHashes.Add(file);
            }
        }

        if (matchingHashes.Count == 0)
        {
            return null;
        }

        if (matchingHashes.Count > 1)
        {
            Utils.ExitWithError("Hash provided is not unique enough. Please provide a longer one.");
        }
        
        return firstTwoDigits + matchingHashes[0];
    } 

    public static Commit GetHeadCommit()
    {
        string hashRef = GetHeadCommitId();
        Commit commit = Deserialize(hashRef);
        return commit;
    }

    /// <summary>
    /// Retrieves the commit ID referenced by the HEAD pointer.
    /// </summary>
    /// <returns>
    /// A string representation of the commit ID
    /// If head points to a branch, the latest commit ID of that branch is returned.
    /// If head points to a commit ID already, returns the commit ID.
    /// </returns>
    public static string GetHeadCommitId()
    {
        string? branch = Branch.GetActiveBranch();

        if (branch != null)
        {
            return Utils.ReadContentsAsString(Path.Combine(Repository.BRANCHES.ToString(), branch));
        }
        
        return Utils.ReadContentsAsString(Path.Combine(Repository.GITLITE_DIR.ToString(), "HEAD"));
    }

}