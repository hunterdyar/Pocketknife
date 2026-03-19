namespace PocketKnifeCore.Engine;

//todo: rename, this is now the plugin ecosystem runtime environment, not the variables-and-such VM environment.
public class Environment
{
    
    #region Runtime Method Access
    //_env loads our plugins and stuff.

    public IPKInputProvider GetInputProvider(string callName, PKItem[] arguments,
        Dictionary<string, PKItem>? options = null)
    {
        if (BuiltinInputProviders.InputProviders.TryGetValue(callName, out var provider))
        {
            return provider.Invoke(arguments, options);
        }

        throw new Exception("Unknown Input Provider '" + callName + $"'. Supported names are: {BuiltinInputProviders.InputProviders.Keys.KeyListString<string, Func<PKItem[], Dictionary<string, PKItem>, IPKInputProvider>>()}");
    }
    public Func<PKItem, bool> GetFilterCommand(string filterName, PKItem[] arguments, Dictionary<string, PKItem> options)
    {
        if (BuiltinFilters.FilterProviders.TryGetValue(filterName,
                out Func<PKItem[], Dictionary<string, PKItem>, Func<PKItem, bool>> filter))
        {
            return filter.Invoke(arguments, options);
        }

        //i love extension methods, but WOOF it looks like i just wrote java? what the everloving fuck?
        throw new Exception("Unknown Filter '" + filterName + $"'. Supported names are: {BuiltinFilters.FilterProviders.Keys.KeyListString<string, Func<PKItem[], Dictionary<string, PKItem>, Func<PKItem, bool>>>()}");
    }

    public Func<PKItem, PKItem> GetPipelineCommand(string pipeName, PKItem[] arguments, Dictionary<string, PKItem>? options)
    {
        if (BuiltinPipes.PipelineProviders.TryGetValue(pipeName, out Func<PKItem[], Dictionary<string, PKItem>, Func<PKItem, PKItem>> pipeFunc))
        {
            return pipeFunc.Invoke(arguments, options);
        }

        //todo: replace all this with our poll-environment-for-supported in/out etc; the thing we will later use to write a gui...
        throw new Exception("Unknown Pipe '" + pipeName +
                            $"'. Supported pipe operations: {BuiltinPipes.PipelineProviders.Keys.KeyListString<string, Func<PKItem[], Dictionary<string, PKItem>, Func<PKItem, PKItem>>>()}");
    }

    #endregion
}