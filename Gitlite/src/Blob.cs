namespace Gitlite;

public static class Blob
{
    /// <summary>
    /// Saves CONTENTS as a blob file.
    /// </summary>
    /// <param name="contents">Contents to write as blob.</param>
    /// <returns>The hash value of CONTENTS.</returns>
    public static string SaveAsBlob(byte[] contents)
    {
        string hash = Utils.HashBytes(contents);
        if (!File.Exists(Path.Combine(Repository.BLOBS_DIR.ToString(), hash)))
        {
            Utils.WriteContent(Path.Combine(Repository.BLOBS_DIR.ToString(), hash), contents);
        }
        return hash;
    }

    /// <summary>
    /// Compares a BLOB to a given FILE
    /// </summary>
    /// <param name="blobRef">Hash reference of the blob</param>
    /// <param name="otherFile">File path of the file to compare to</param>
    /// <returns>A boolean value if the blob and the file has the same content</returns>
    public static bool CompareToOtherFile(string blobRef, string otherFile)
    {
        byte[] blobContent = Utils.ReadContentsAsBytes(Repository.BLOBS_DIR.ToString(), blobRef);
        byte[] otherFileContent = Utils.ReadContentsAsBytes(otherFile);
        return blobContent.SequenceEqual(otherFileContent);
    }
}