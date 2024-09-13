namespace GitLite;

public class Utils
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
    /// <param name="cmd">Command</param>
    /// <param name="args">The arguments provided along with the COMMAND</param>
    /// <param name="n">The number of arguments required.</param>
    /// <exception cref="ArgumentException">Returns an ArgumentException if invalid.</exception>
    public static void ValidateArguments(string cmd, string[] args, int n)
    {
        if (args.Length != n)
        {
            throw new ArgumentException($"Invalid number of arguments for: {cmd}");
        }
    }
}