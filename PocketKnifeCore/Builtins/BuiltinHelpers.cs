using System.Text;
using System.Management.Automation;

namespace PocketKnifeCore;

public static class BuiltinHelpers
{
    public static void CheckArgumentCount(PKItem[] args, int count)
    {
        if (args.Length != count)
        {
            throw new Exception("Invalid argument count.");
        }
    }

    public static T GetArgument<T>(PKItem inputItem, string argName) where T : PKItem
    {
        if (inputItem is T requestedType)
        {
            return requestedType;
        }
        throw new Exception($"Argument {argName} not of type {typeof(T).Name}");
    }

    public static T GetArgument<T>(PKItem[] args, int argPos, string argName) where T : PKItem
    {
        if (args[argPos] is T requestedType)
        {
            return requestedType;
        }

        throw new Exception($"Argument {argName} (position {argPos}) is not of type {typeof(T).Name}");
    }

    public static string KeyListString<T1, T2>(this Dictionary<T1, T2>.KeyCollection collection)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in collection)
        {
            sb.Append(item.ToString());
            sb.Append(", ");
        }

        sb.Remove(sb.Length-2,2);
        return sb.ToString();
    }

    public static string KeyListString(this Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, bool>>?> dict) 
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in dict.Keys)
        {
            sb.Append(item.ToString());
            sb.Append(", ");
        }

        sb.Remove(sb.Length - 2, 2);
        return sb.ToString();
    }

    public static string KeyListString(this Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, PKItem>>?> dict)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in dict.Keys)
        {
            sb.Append(item.ToString());
            sb.Append(", ");
        }

        sb.Remove(sb.Length - 2, 2);
        return sb.ToString();
    }

    public static void ExecuteCommand(string fullcomamnd, bool async = false)
    {
        PowerShell ps = PowerShell.Create();
        ps.AddCommand(fullcomamnd);
        ps.Invoke();

        if (ps.HadErrors)
        {
            throw new Exception($"command ({fullcomamnd}) had errors:\n{ps.Streams.Error.ReadAll()}");
        }
        //later, when we have a sideways logging system. we'll add error output or full output.
        
        //also, this is for 'signal', exec-and-forget. we also need to read the standard input back into a string and return it for a pipeline process.
        
        //end if windows
    }
}