// See https://aka.ms/new-console-template for more information

namespace GitLite;

/// <summary>
/// Class that runs GitLite.
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Utils.ExitWithError("Must have at least one argument");   
        }

        switch (args[0])
        {   
            case "init":
                Utils.ValidateArguments("init", args, 1);
                Repository.Init();
                break;
            default:
                Utils.ExitWithError($"Unknown command: {args[0]}");
                break;
        }
    }
}