namespace PocketKnifeCore.Engine;

//The 
public class Environment
{
    public List<Context> AllContexts;

    private Stack<Context> _stack = new Stack<Context>();
    private Stack<IPKInputProvider> _inputProviders = new Stack<IPKInputProvider>();
    //a comtext is an item on the top of a stack...
    //it's also the list that we came from. List->Item,item,item 
    

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
        //todo: move inputProvider to Context, i think?
        _inputProviders.Push(input);
    }

    public bool TryGetNextInput(out IPKInputProvider provider)
    {
        return _inputProviders.TryPop(out provider);
    }


    #region Runtime Method Access
    //_env loads our plugins and stuff.

    public IPKInputProvider GetInputProvider(string callName, PKItem[] arguments,
        Dictionary<string, PKItem>? options = null)
    {
        if (BuiltinInputProviders.InputProviders.TryGetValue(callName, out var provider))
        {
            return provider.Invoke(arguments, options);
        }

        throw new Exception("Unknown Input Provider '" + callName + $"'. Supported names are: {BuiltinInputProviders.InputProviders.Keys}");
    }
    public Func<PKItem, bool> GetFilterCommand(string filterName, PKItem[] arguments, Dictionary<string, PKItem> options)
    {
        if (BuiltinFilters.FilterProviders.TryGetValue(filterName,
                out Func<PKItem[], Dictionary<string, PKItem>, Func<PKItem, bool>> filter))
        {
            return filter.Invoke(arguments, options);
        }

        throw new Exception("Unknown Filter '" + filterName + $"'. Supported names are: {BuiltinFilters.FilterProviders.Keys}");
    }

    #endregion

}