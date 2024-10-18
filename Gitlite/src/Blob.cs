namespace Gitlite;

public static class Blob
{
    
    /// <summary>
    /// Saves CONTENTS as a blob file.
    /// </summary>
    /// <param name="contents">Contents to write as blob.</param>
    /// <param name="hash">Optional argument. The hash of CONTENTS.</param>
    public static void SaveBlob(byte[] contents, string? hash = null)
    {
        if (hash == null)
        {
            hash = Utils.HashBytes(contents);
        }
        
        var (firstTwoDigits, rest) = Utils.SplitHashPath(hash);
        string path = Path.Combine(Repository.BLOBS_DIR.ToString(), firstTwoDigits);
        Directory.CreateDirectory(path);
        path = Path.Combine(path, rest);
        if (!File.Exists(path))
        {
            Utils.WriteContent(path, contents);
        }
    }

    /// <summary>
    /// Compares a BLOB to a given FILE
    /// </summary>
    /// <param name="blobRef">Hash reference of the blob</param>
    /// <param name="otherFile">File path of the file to compare to</param>
    /// <returns>A boolean value if the blob and the file has the same content</returns>
    public static bool IsEqualToOtherFile(string blobRef, string otherFile)
    {
        byte[] otherFileContent = Utils.ReadContentsAsBytes(otherFile);
        return Utils.HashBytes(otherFileContent) == blobRef;
    }

    /// <summary>
    /// Reads the content of a blob given its hash reference.
    /// </summary>
    /// <param name="blobRef">Hash reference of the blob.</param>
    /// <returns>Content of the BLOB in string.</returns>
    public static string ReadBlobContentAsString(string blobRef)
    {
        var (firstTwoDigits, rest) = Utils.SplitHashPath(blobRef);
        string path = Path.Combine(Repository.BLOBS_DIR.ToString(), firstTwoDigits, rest);
        byte[] bytes = Utils.ReadContentsAsBytes(path);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}