namespace GitLite;

public class Utils
{
    
    
    /* FILE & DIR UTILS*/

    public static DirectoryInfo JoinDirectory(string first, string second)
    {
        return new DirectoryInfo(Path.Combine(first, second));
    }

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
    
    
    public static void ValidateArguments(string cmd, string[] args, int n)
    {
        if (args.Length != n)
        {
            throw new ArgumentException($"Invalid number of arguments for: {cmd}");
        }
    }
}