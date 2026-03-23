namespace PocketKnifeCore.Engine;

public class PluginEnvironment
{
    // public void LoadCommandProvider(string name...)
    
    #region Runtime Method Access
    //_env loads our plugins and stuff. 

    //todo: split compile time (options) and runtime (arguments). we can get the input provider getter function, and then invoke that at runtime?
    
    public IPKInputProvider GetInputProvider(string callName, RuntimeExpression[] arguments, Dictionary<string, PKItem>? options = null)
    {
        if (BuiltinInputProviders.InputProviders.TryGetValue(callName, out var provider))
        {
            return provider.Invoke(arguments, options);
        }

        throw new Exception("Unknown Input Provider '" + callName + $"'. Supported names are: (todo)");
    }
    public FilterProcess GetFilterCommand(string filterName, RuntimeExpression[] arguments, Dictionary<string, PKItem> options)
    {
        if (BuiltinFilters.FilterProviders.TryGetValue(filterName, out Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, bool>> filter))
        {
            return new FilterProcess(arguments, filter.Invoke(options));
        }

        //i love extension methods, but WOOF it looks like i just wrote java? what the everloving fuck?
        throw new Exception("Unknown Filter '" + filterName + $"'. Supported names are: {BuiltinFilters.FilterProviders.KeyListString()}");
    }

    public PipelineProcess GetPipelineCommand(string pipeName, RuntimeExpression[] arguments, Dictionary<string, PKItem>? options)
    {
        if (BuiltinPipes.PipelineProviders.TryGetValue(pipeName, out Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, PKItem>> pipeFunc))
        {
            return new PipelineProcess(arguments, pipeFunc.Invoke(options));
        }

        //todo: replace all this with our poll-environment-for-supported in/out etc; the thing we will later use to write a gui...
        throw new Exception("Unknown Pipe '" + pipeName + $"'. Supported pipe operations: {BuiltinPipes.PipelineProviders.KeyListString()}");
    }

    #endregion
}