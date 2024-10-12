using MessagePack;

namespace Gitlite;

[MessagePackObject]
public partial class StagingArea
{

    public static string STAGING_AREA = Path.Combine(Repository.GITLITE_DIR.ToString(), "staging");

    /// <summary>
    /// Mapping of files for addition. (fileName, contents in byte[])
    /// </summary>
    [Key(0)]
    private Dictionary<string, string> StagingForAddition { get; set; }
    
    /// <summary>
    /// Mapping of files for addition.
    /// </summary>
    [Key(1)]
    private List<string> StagingForRemoval { get; set; }

    public StagingArea()
    {
        StagingForAddition = new Dictionary<string, string>();
        StagingForRemoval = new List<string>();
    }

    /// <summary>
    /// Returns the mapping of files for addition.
    /// </summary>
    /// <returns>A Dictionary with (string, byte[]) pair</returns>
    public Dictionary<string, string> GetStagingForAddition()
    {
        return StagingForAddition;
    }

    /// <summary>
    /// Returns the mapping of files for removal.
    /// </summary>
    /// <returns>A Dictionary with (string, byte[]) pair</returns>
    public List<string> GetStagingForRemoval()
    {
        return StagingForRemoval;
    }

    /// <summary>
    /// Deserializes a serialized StagingArea Object.
    /// </summary>
    /// <param name="serializedStagingArea">Serialized byte array</param>
    /// <returns>A Deserialized StagingArea Object</returns>
    private static StagingArea Deserialize(byte[] serializedStagingArea)
    {
        return MessagePackSerializer.Deserialize<StagingArea>(serializedStagingArea);
    }

    /// <summary>
    /// Returns the deserialized staging area.
    /// </summary>
    /// <returns>A StagingArea object</returns>
    public static StagingArea GetDeserializedStagingArea()
    {
        byte[] serializedObj = Utils.ReadContentsAsBytes(STAGING_AREA);
        return Deserialize(serializedObj);
    }

    public void Save()
    {
        byte[] serialized = MessagePackSerializer.Serialize(this);
        Utils.WriteContent(STAGING_AREA, serialized);
    }

    public void Clear()
    {
        StagingForAddition.Clear();
        StagingForRemoval.Clear();
    }

    /// <summary>
    /// Creates the staging area under .gitlite directory if it does not exist.
    /// </summary>
    public static void CreateStagingArea()
    {
        FileInfo staging = new FileInfo(STAGING_AREA);

        if (!staging.Exists)
        {
            StagingArea stagingAreaObject = new StagingArea();
            stagingAreaObject.Save();
        }
    }

    /// <summary>
    /// Prints out the string representation of the staging area.
    /// </summary>
    public void StringValue()
    {
        Console.WriteLine("For addition:");
        foreach (var keyValuePair in StagingForAddition)
        {
            Console.WriteLine(keyValuePair.Key);
            Console.WriteLine(keyValuePair.Value);
            Console.WriteLine("");
        }

        Console.WriteLine("For removal:");
        foreach (string file in StagingForRemoval)
        {
            Console.WriteLine(file);
            Console.WriteLine("");
        }
    }

    public bool IsThereStagedFiles()
    {
        return StagingForAddition.Count != 0 || StagingForRemoval.Count != 0;
    }


    /// <summary>
    /// Compares a STAGED FILE to a given FILE
    /// </summary>
    /// <param name="stagedFile">Name of the staged file</param>
    /// <param name="otherFile">Path of the other File</param>
    /// <returns>A boolean value if the staged file and the file has the same content</returns>
    public bool CompareStagedFileToOtherFile(string stagedFile, string otherFile)
    {
        byte[] stagedFileContent = Utils.ReadContentsAsBytes(StagingForAddition[stagedFile]);
        byte[] otherFileContent = Utils.ReadContentsAsBytes(otherFile);
        return stagedFileContent.SequenceEqual(otherFileContent);
    }
    
}