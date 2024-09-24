using System.Security.Cryptography;
using MessagePack;
    
namespace GitLite;

/// <summary>
/// Class that represents GitLite commits.
/// </summary>
[MessagePackObject]
public class Commit
{
    [Key(0)] public string LogMessage { get; set; }

    [Key(1)] public DateTime Timestamp { get; set; }


    /// <summary>
    /// Initializes a new Commit object.
    /// </summary>
    /// <param name="logMessage"></param>
    /// <param name="timestamp"></param>
    public Commit(string logMessage, DateTime timestamp)
    {
        this.LogMessage = logMessage;
        this.Timestamp = timestamp;
    }

    public static string GetHash(Commit commit)
    {
        byte[] bytes = MessagePackSerializer.Serialize(commit);
        return Convert.ToHexString(SHA1.HashData(bytes)).ToLower();
    }
    
    /// <summary>
    /// Creates a Commit object with LOG MESSAGE and returns its hash.
    /// </summary>
    /// <param name="logMessage">Commit log message</param>
    /// <param name="timestamp">Timestamp of when the commit was created.</param>
    /// <returns>Hash string of the created Commit object.</returns>
    private static string CreateCommit(string logMessage, DateTime timestamp)
    {
        Commit commit = new Commit(logMessage, timestamp);
        string hash = GetHash(commit);
        // File.CreateText(Path.Combine(Repository.COMMITS_DIR.ToString(), hash));
        byte[] serializedCommit = MessagePackSerializer.Serialize(commit);
        Utils.WriteContent(Path.Combine(Repository.COMMITS_DIR.ToString(), hash), serializedCommit);
        return hash;
    }
    
    public override string ToString()
    {
        return $"LogMessage: {LogMessage}, Timestamp: {Timestamp}";
    }

    public static string CreateCommit(string logMessage)
    {
        DateTime timestamp = DateTime.Now;
        return CreateCommit(logMessage, timestamp);
    }

    public static string CreateInitialCommit()
    {
        DateTime unixEpoch = DateTime.UnixEpoch;
        return CreateCommit("initial commit", unixEpoch);
    }

    public static Commit Deserialize(string fileName)
    {
        byte[] byteValue = Utils.ReadContentsAsBytes(fileName);
        return MessagePackSerializer.Deserialize<Commit>(byteValue);
    }
}