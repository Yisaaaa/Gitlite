namespace GitLite;

/// <summary>
/// A blob represents the contents of a file at a particular point
/// in time. E.g. The different versions of a file.
/// </summary>
public class Blob
{
    public string hashName;
    public string fileName;
    public byte[] serializedContent;
}