using System.Security.Cryptography;
using MessagePack;
    
namespace GitLite;

/// <summary>
/// Class that represents GitLite commits.
/// </summary>
[MessagePackObject]
public class Commit
{
    [Key(0)]
    public string logMessage;
    [Key(1)]
    public DateTime timestamp;


    /// <summary>
    /// Initializes a new Commit object.
    /// </summary>
    /// <param name="logMessage"></param>
    /// <param name="timestamp"></param>
    public Commit(string logMessage, DateTime timestamp)
    {
        this.logMessage = logMessage;
        this.timestamp = timestamp;
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
        File.Create(Path.Combine(Repository.COMMITS_DIR.ToString(), hash));
        return hash;
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
}