using System.Text.Json;
using MessagePack;

namespace GitLite;

public class Reader
{
    
    /// <summary>
    /// Reads an object like Commit, Blob, etc. Only for testing purposes.
    /// </summary>
    /// <param name="fileName">File name on the object to read.</param>
    /// <param name="objectType">Type of the object to read. (e.g. Commit, Blob)</param>
    public static void Read(string fileName, string objectType)
    {
        switch (objectType)
        {
            case "commit":
                Commit deserialized = Commit.Deserialize(Path.Combine(".gitlite/commits/", fileName));
                Console.WriteLine(JsonSerializer.Serialize(deserialized, new JsonSerializerOptions { WriteIndented = true }));
                break;
            
            case "blob":
                byte[] bytes = File.ReadAllBytes(Path.Combine(".gitlite/commits", fileName));
                string fileContents = System.Text.Encoding.UTF8.GetString(bytes);
                Console.WriteLine(fileContents);
                break;
            
            case "staging":
                StagingArea staging = StagingArea.GetDeserializedStagingArea();
                Console.WriteLine("staging");
                staging.StringValue();
                break;
            
            default:
                Utils.ExitWithError($"No object with name: {objectType}");
                break;
        }
    }
}