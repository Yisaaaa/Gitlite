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
    private Dictionary<string, byte[]> stagingForAddition { get; set; }
    
    /// <summary>
    /// Mapping of files for addition.
    /// </summary>
    [Key(1)]
    private Dictionary<string, byte[]> stagingForRemoval { get; set; }

    public StagingArea()
    {
        stagingForAddition = new Dictionary<string, byte[]>();
        stagingForRemoval = new Dictionary<string, byte[]>();
    }

    /// <summary>
    /// Returns the mapping of files for addition.
    /// </summary>
    /// <returns>A Dictionary with (string, byte[]) pair</returns>
    public Dictionary<string, byte[]> GetStagingForAddition()
    {
        return stagingForAddition;
    }

    /// <summary>
    /// Returns the mapping of files for removal.
    /// </summary>
    /// <returns>A Dictionary with (string, byte[]) pair</returns>
    public Dictionary<string, byte[]> GetStagingForRemoval()
    {
        return stagingForRemoval;
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
        foreach (var keyValuePair in stagingForAddition)
        {
            Console.WriteLine(keyValuePair.Key);
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(keyValuePair.Value));
            Console.WriteLine("");
        }

        Console.WriteLine("For removal:");
        foreach (var keyValuePair in stagingForRemoval)
        {
            Console.WriteLine(keyValuePair.Key);
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(keyValuePair.Value));
            Console.WriteLine("");
        }
    }

    public bool IsThereStagedFiles()
    {
        return stagingForAddition.Count != 0 || stagingForRemoval.Count != 0;
    }
    
}