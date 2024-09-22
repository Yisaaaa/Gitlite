using MessagePack;

namespace GitLite;

[MessagePackObject]
public partial class StagingArea
{

    public static string STAGING_AREA = Path.Combine(Repository.GITLITE_DIR.ToString(), "staging");

    [Key(0)]
    private Dictionary<string, byte[]> stagingForAddition;
    [Key(1)]
    private Dictionary<string, byte[]> stagingForRemoval;

    public StagingArea()
    {
        stagingForAddition = new Dictionary<string, byte[]>();
        stagingForRemoval = new Dictionary<string, byte[]>();
    }

    public Dictionary<string, byte[]> GetStagingForAddition()
    {
        return stagingForAddition;
    }

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

    public static StagingArea GetDeserializedStagingArea()
    {
        byte[] serializedObj = Utils.ReadContentsAsBytes(STAGING_AREA);
        return Deserialize(serializedObj);
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
            byte[] serialized = MessagePackSerializer.Serialize(stagingAreaObject);
            Utils.WriteContent(staging.ToString(), serialized);
        }
    }
    
    
}