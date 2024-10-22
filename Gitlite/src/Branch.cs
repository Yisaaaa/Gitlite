namespace Gitlite;

public static class Branch
{
    
    /// <summary>
    /// Helper function that gets all existing branches and marks the active
    /// branch with '*'
    /// </summary>
    /// <returns>A string[] of all branches</returns>
    public static string[] GetExistingBranches()
    {
        string[] branches = Utils.GetFilesSorted(Repository.BRANCHES.ToString());
        return branches;
    }
    
    /// <summary>
    /// Creates a GitLite branch.
    /// </summary>
    /// <param name="name">Name of the branch</param>
    /// <param name="commitHashRef">Commit hash reference that the branch points to</param>
    public static void CreateBranch(string name, string commitHashRef)
    {
        string branch = Path.Combine(Repository.BRANCHES.ToString(), name);
        Utils.WriteContent(branch, commitHashRef);
    }
    
    /// <summary>
    /// Checks if given BRANCH is the active branch
    /// </summary>
    /// <param name="branch">Branch name</param>
    /// <returns>A boolean value if BRANCH is active</returns>
    public static bool IsCurrentBranch(string branch)
    {
        string HEADhash = Commit.GetHeadCommitId();
        string branchHash = Utils.ReadContentsAsString(Repository.BRANCHES.ToString(), branch);

        return HEADhash == branchHash;
    }

    /// <summary>
    /// Returns the name of the current active branch from the HEAD pointer.
    /// </summary>
    /// <returns>String name of the current active branch.</returns>
    public static string? GetActiveBranch()
    {
        string head = Utils.ReadContentsAsString(Repository.GITLITE_DIR.ToString(), "HEAD");

        if (head.StartsWith("ref: "))
        {
            return head.Split("ref: ")[1];
        }

        return null;
    }
}