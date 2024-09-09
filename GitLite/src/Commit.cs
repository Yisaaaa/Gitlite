namespace GitLite;

/// <summary>
/// Class that represents GitLite commits.
/// </summary>
public class Commit
{
    public string hashName;
    public string logMessage;
    public DateTime timestamp;
    public string parentHashRef;
    public string? secondParentHashRef;


    /// <summary>
    /// Initializes a new Commit object.
    /// </summary>
    /// <param name="hashName"></param>
    /// <param name="logMessage"></param>
    /// <param name="timestamp"></param>
    /// <param name="parentHashRef"></param>
    /// <param name="secondParentHashRef"></param>
    public Commit(string hashName, string logMessage, DateTime timestamp, string parentHashRef,
        string? secondParentHashRef)
    {
        this.hashName = hashName;
        this.logMessage = logMessage;
        this.timestamp = timestamp;
        this.parentHashRef = parentHashRef;
        this.secondParentHashRef = secondParentHashRef;
    }

    /// <summary>
    /// Creates a new Commit object and returns it.
    /// </summary>
    /// <param name="logMessage"></param>
    /// <param name="parentHashRef"></param>
    /// <param name="secondParentHashRef"></param>
    /// <returns></returns>
    public static Commit CreateCommit(string logMessage, string parentHashRef, string? secondParentHashRef)
    {
        throw new NotImplementedException();
    }
    
}