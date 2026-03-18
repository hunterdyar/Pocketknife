using System.Diagnostics;

namespace PocketKnifeCore;

public static class BuiltinInputProviders
{
    public static Dictionary<string, Func<PKItem[], IPKInputProvider>> InputProviders =
        new Dictionary<string, Func<PKItem[], IPKInputProvider>>()
        {
            {
                "dir", (a) =>
                {
                    //todo: make my own asserts
                    BuiltinHelpers.CheckArgumentCount(a, 1);
                    var path = BuiltinHelpers.GetArgument<PKString>(a[0], "directory path");
                    return new PKDirectoryInfo(path);
                }
            }
        };
}