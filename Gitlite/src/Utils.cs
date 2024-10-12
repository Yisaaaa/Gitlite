using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using static Gitlite.Repository;

namespace Gitlite;

public static class Utils
{
    
    /* READING & WRITING */
    /// <summary>
    /// Writes a single string content to a file.
    /// </summary>
    /// <param name="file">File name</param>
    /// <param name="content">Content to write into the file</param>
    public static void WriteContent(string file, string content)
    {
        using (StreamWriter sw = new StreamWriter(file))
        {
            sw.Write(content);
        }
    }

    /// <summary>
    /// Writes byte array content to a file.
    /// </summary>
    /// <param name="file">File name</param>
    /// <param name="content">Content in to write represented in byte array into the file</param>
    public static void WriteContent(string file, byte[] content)
    {
        File.WriteAllBytes(file, content);
    }

    /// <summary>
    /// Reads the given file as bytes[]
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="name">File name</param>
    /// <returns>A bytes[] representation of the FILE content</returns>
    public static byte[] ReadContentsAsBytes(string path, string? name = null)
    {
        if (name != null)
        {
            path = Path.Combine(path, name);
        }
        
        ValidateFile(path);
        return File.ReadAllBytes(path);
    }

    /// <summary>
    /// Reads the file as string
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="name">File name</param>
    /// <returns>Content of FILE in string</returns>
    public static string ReadContentsAsString(string path, string? name = null)
    {
        if (name != null)
        {
            path = Path.Combine(path, name);
        }
        ValidateFile(path);
        return File.ReadAllText(path);
    }
    
    /// <summary>
    /// Saves CONTENTS as a blob file.
    /// </summary>
    /// <param name="contents">Contents to write as blob.</param>
    /// <returns>The hash value of CONTENTS.</returns>
    public static string SaveAsBlob(byte[] contents)
    {
        string hash = HashBytes(contents);
        if (!File.Exists(Path.Combine(BLOBS_DIR.ToString(), hash)))
        {
            WriteContent(Path.Combine(BLOBS_DIR.ToString(), hash), contents);
        }
        return hash;
    }
    
    /* FILE & DIR UTILS*/
  
    /// <summary>
    /// Joins the first and second directory path together.
    /// </summary>
    /// <param name="first">String representation of the first path</param>
    /// <param name="second">String representation of the second path to add
    /// to the FIRST path</param>
    /// <returns>A DirectoryInfo referencing to the joined directory.</returns>
    public static DirectoryInfo JoinDirectory(string first, string second)
    {
        return new DirectoryInfo(Path.Combine(first, second));
    }

    /// <summary>
    /// Joins a DirectoryInfo with a string together.
    /// </summary>
    /// <param name="first">DirectoryInfo representation of the first path</param>
    /// <param name="second">String representation of the second path to add
    /// to the FIRST path</param>
    /// <returns>A DirectoryInfo referencing to the joined directory.</returns>
    public static DirectoryInfo JoinDirectory(DirectoryInfo first, string second)
    {
        return JoinDirectory(first.ToString(), second);
    }

    /// <summary>
    /// Returns a sorted list of all file names in a directory. 
    /// </summary>
    /// <param name="path">Directory path</param>
    /// <returns>Returns a sorted string[] of all file names</returns>
    public static string[] GetFilesSorted(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new ArgumentException("Invalid path: ", path);
        }
        
        string[] files = Directory.GetFiles(path);
        return files.OrderBy(file => file).ToArray();
    }
    
    
    /* MESSAGES & ERROR REPORTING*/
    
    /// <summary>
    /// Exits the program and prints out MESSAGE.
    /// </summary>
    /// <param name="message">Message to print out</param>
    public static void ExitWithError(string? message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            Console.WriteLine(message);
        }

        Environment.Exit(-1);
    }
    
    /// <summary>
    /// Checks if the number of arguments provided with CMD(command) is valid.
    /// </summary>
    /// <param name="cmd">Command name.</param>
    /// <param name="args">The arguments provided along with the COMMAND.</param>
    /// <param name="n">The number of arguments required.</param>
    /// <param name="message">Message to print if command is not valid.</param>>
    public static void ValidateArguments(string cmd, string[] args, int n, string? message=null)
    {
        if (args.Length == n) return;
        
        if (string.IsNullOrEmpty(message))
        {
            ExitWithError($"Invalid number of arguments for: {cmd}");
        }
            
        ExitWithError(message);
    }

    public static void ValidateFile(string path, string? name = null)
    {
        if (name != null)
        {
            path = Path.Combine(path, name);
        }
        
        if (!File.Exists(path))
        {
            throw new ArgumentException("Invalid file: ", path);
        }
    }

    
    /* HASHING */
    public static string HashBytes(byte[] bytes)
    {
        return Convert.ToHexString(SHA1.HashData(bytes)).ToLower();
    }
}