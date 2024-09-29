using System.Security.Cryptography;
using MessagePack;
    
namespace Gitlite;

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
        byte[] serializedCommit = MessagePackSerializer.Serialize(commit);
        Utils.WriteContent(Path.Combine(Repository.COMMITS_DIR.ToString(), hash), serializedCommit);
        return hash;
    }
    
    public override string ToString()
    {
        return $"LogMessage: {LogMessage}, Timestamp: {Timestamp}";
    }
    
    public static string CreateInitialCommit()
    {
        DateTime unixEpoch = DateTime.UnixEpoch;
        return CreateCommit("initial commit", unixEpoch, null);
    }

    public static Commit Deserialize(string fileName)
    {
        byte[] byteValue = Utils.ReadContentsAsBytes(fileName);
        return MessagePackSerializer.Deserialize<Commit>(byteValue);
    }

    public static Commit GetHeadCommit()
    {
        string hashRef = Utils.ReadContentsAsString(Path.Combine(Repository.GITLITE_DIR.ToString(), "HEAD"));
        Console.WriteLine(hashRef);
        Commit commit = Deserialize(Path.Combine(Repository.COMMITS_DIR.ToString(), hashRef));

        return commit;
    }
}