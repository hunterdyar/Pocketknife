namespace PocketKnifeCore.Engine;

//The 
public class Environment
{
    //a comtext is an item on the top of a stack...
    //it's also the list that we came from. List->Item,item,item 
    public IPKInputProvider GetInputProvider(string callName, PKItem[] arguments)
    {
        if (BuiltinInputProviders.InputProviders.TryGetValue(callName, out var provider))
        {
            return provider.Invoke(arguments);
        }
        throw new Exception("Unknown Input Provider '" + callName + $"'. Supported names are: {BuiltinInputProviders.InputProviders.Keys}");
    }
}

public class Context
{
    public PKItem Top;
}