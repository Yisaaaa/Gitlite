using MessagePack;

namespace GitLite;

[MessagePackObject]
public partial class StagingArea
{
    [Key(0)]
    private Dictionary<string, string> stagingForAddition;
    [Key(1)]
    private Dictionary<string, string> stagingForRemoval;

    public StagingArea()
    {
        stagingForAddition = new Dictionary<string, string>();
        stagingForRemoval = new Dictionary<string, string>();
    }

    public Dictionary<string, string> GetStagingForAddition()
    {
        return stagingForAddition;
    }

    public Dictionary<string, string> GetStagingForRemoval()
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
    /// Creates the staging area under .gitlite directory if it does not exist.
    /// </summary>
    public static void CreateStagingArea()
    {
        FileInfo staging = new FileInfo(Path.Combine(Repository.GITLITE_DIR.ToString(), "staging"));

        if (!staging.Exists)
        {
            StagingArea stagingAreaObject = new StagingArea();
            byte[] serialized = MessagePackSerializer.Serialize(stagingAreaObject);
            Utils.WriteContent(staging.ToString(), serialized);
        }
    }
}