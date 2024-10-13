// See https://aka.ms/new-console-template for more information

namespace Gitlite;

/// <summary>
/// Class that runs GitLite.
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Utils.ExitWithError("Please enter a command.");   
        }

        if (args[0] == "init")
        {
            Utils.ValidateArguments("init", args, 1);
            Repository.Init();
            return;
        }

        if (!Repository.GitliteAlreadyInitialized())
        {
            Utils.ExitWithError("Not in an initialized GitLite directory.");
        }
            
        switch (args[0])
        {
            case "add":
                Utils.ValidateArguments("add", args, 2);
                Repository.Add(args[1]);
                break;
            
            case "commit":
                Utils.ValidateArguments("commit", args, 2, "Please provide a commit message.");
                Repository.Commit(args[1]);
                break;
            
            case "read":
                Utils.ValidateArguments("read", args, 3);
                Reader.Read(args[1], args[2]);
                break;
            
            case "rm":
                Utils.ValidateArguments("rm", args, 2);
                Repository.Rm(args[1]);
                break;
            
            case "log":
                Utils.ValidateArguments("log", args, 1);
                Repository.Log();
                break;
            
            case "global-log":
                Utils.ValidateArguments("global-log", args, 1);
                Repository.GlobalLog();
                break;
            
            case "find":
                Utils.ValidateArguments("find", args, 2);
                Repository.Find(args[1]);
                break;
            
            case "status":
                Utils.ValidateArguments("status", args, 1);
                Repository.Status();
                break;
                
            default:
                Utils.ExitWithError($"No command with such name exists: {args[0]}");
                break;
        }
    }
}