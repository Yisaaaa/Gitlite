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

    private static string GetHash(Commit commit)
    {
        byte[] bytes = MessagePackSerializer.Serialize(commit);
        return Convert.ToHexString(SHA1.HashData(bytes));
    }
    
    /// <summary>
    /// Creates a new Commit object and returns it.
    /// </summary>
    /// <param name="logMessage"></param>
    /// <returns>A Commit object.</returns>
    public static Commit CreateCommit(string logMessage)
    {
        DateTime timeCreated = DateTime.Now;
        return new Commit(logMessage, timeCreated);
        
    }
    
}