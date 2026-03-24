using System.Reflection;

namespace PocketKnifeCore.Engine;

public class PluginEnvironment
{
    public PluginEnvironment()
    {
        RegisterPipelineOperator(typeof(StringBuiltins));
    }
    // public void LoadCommandProvider(string name...)
    public void RegisterPipelineOperator(Type type)
    {
        MethodInfo[] classMethods = type.GetMethods();

        // Loop through all methods in this class that are in the
        // MyMemberInfo array.
        for (int i = 0; i < classMethods.Length; i++)
        {
            var att = (PipelineOperator)Attribute.GetCustomAttribute(classMethods[i], typeof(PipelineOperator));
            if (att != null)
            {
                var method = classMethods[i];
                
                if (att.OnlyValidOn != null)
                {
                    if (method.GetParameters()[0].ParameterType != att.OnlyValidOn)
                    {
                        throw new Exception(
                            $"unable to register pipeline operator {att.Name} ({method.Name}). Type does not match attribute valid type.");
                    }
                    BuiltinPipes.PipelineProviders.Add(att.Name,
                        new Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, PKItem>>(a =>
                        {
                            return (args, item) =>
                            {
                                if (item.GetType() == att.OnlyValidOn)
                                {
                                    return (PKItem)method.Invoke(item, args);
                                }
                                else
                                {
                                    throw new Exception(
                                        $"Cannot call {att.Name} on item of type {item.GetType()}. {att.Name} only operates on {att.OnlyValidOn}.");
                                }
                            };
                        }));
                }
                else
                {
                    BuiltinPipes.PipelineProviders.Add(att.Name, new Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, PKItem>>(a =>
                     {
                         return (args, item) =>
                         {
                             return (PKItem)method.Invoke(item, args);
                         };
                     }));
                }
            }
        }
    }
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