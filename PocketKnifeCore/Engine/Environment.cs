namespace PocketKnifeCore.Engine;

//The 
public class Environment
{
    public List<Context> AllContexts;

    private Stack<Context> _stack = new Stack<Context>();
    private Stack<IPKInputProvider> _inputProviders = new Stack<IPKInputProvider>();
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

    public void PushContext(Context context)
    {
        _stack.Push(context);
    }

    public void PopContext()
    {
        _stack.Pop();
    }

    public void PushInputProvider(IPKInputProvider input)
    {
        _inputProviders.Push(input);
    }

    public bool TryGetNextInput(out IPKInputProvider provider)
    {
        return _inputProviders.TryPop(out provider);
    }
}