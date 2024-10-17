using System.Security.Cryptography;
using System.Text;

namespace Gitlite;

public static class AdHocTest
{
    public static void AdHocTesting()
    {
        string test = "strinalkjsdf";
        string hash = Utils.HashBytes(Encoding.ASCII.GetBytes(test));
        var (first, second) = Utils.SplitHashPath(hash);
        Console.WriteLine(first);
        Console.WriteLine(second);
        Console.WriteLine(hash);
    }
}