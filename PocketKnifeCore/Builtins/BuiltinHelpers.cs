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
}