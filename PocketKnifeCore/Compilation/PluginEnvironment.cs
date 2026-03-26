using System.Collections;
using System.Reflection;

namespace PocketKnifeCore.Engine;

public class PluginEnvironment
{
    //todo: should this be static?
    public static readonly Dictionary<string, Func<FileInfo, PKItem>> AllLoaders = new Dictionary<string, Func<FileInfo, PKItem>>();

    public static readonly Dictionary<string,Saver> AllSavers = new Dictionary<string, Saver>();

    public static readonly Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem, PKItem[], IEnumerable<PKItem>>>> AllPipeInputProviders = new Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem, PKItem[], IEnumerable<PKItem>>>>(); 
    public PluginEnvironment()
    {
        RegisterOperations(typeof(StringBuiltins));
        RegisterOperations(typeof(CSVBuiltins));
    }

    public void RegisterOperations(Type type)
    {
        RegisterCommands(type);
        RegisterSaversLoaders(type);
    }

    public void RegisterSaversLoaders(Type type)
    {
        MethodInfo[] classMethods = type.GetMethods();
        for (int i = 0; i < classMethods.Length; i++)
        {
            var latt = (LoaderAttribute)Attribute.GetCustomAttribute(classMethods[i], typeof(LoaderAttribute));
            if (latt != null)
            {
                var method = classMethods[i];
                AllLoaders.Add(latt.Name, fi =>
                {
                    if (!fi.Exists)
                    {
                        throw new Exception($"File {fi} does not exist. Can't |load");
                    }
                    
                    return (PKItem)method.Invoke(null, [fi]);
                });
            }

            var satt = (SaverAttribute)Attribute.GetCustomAttribute(classMethods[i], typeof(SaverAttribute));
            if (satt != null)
            {
                var method = classMethods[i];
                // method = method.MakeGenericMethod(typeof(FileStream), typeof(PKItem));
                var p = method.GetParameters();
                if (p[0].ParameterType != typeof(FileStream))
                {
                    throw new Exception($"saver attribute must have type 'filestream, pkitem (or descendent). error on {method}");
                }else if (p[1].ParameterType != typeof(PKItem))
                {
                    if(!p[1].ParameterType.IsSubclassOf(typeof(PKItem))){
                        throw new Exception(
                            $"saver attribute must have type 'filestream, pkitem (or descendent). error on {method}");
                    }
                }
                
                var s = new Saver()
                {
                    DefaultExtension = satt.DefaultExtension,
                    OnlyValidOn = satt.OnlyValidOn,
                    Name = satt.Name,
                    Writer = (fs, item) => { method.Invoke(null, [fs, item]); }
                };
                AllSavers.Add(satt.Name,s);
            }
        }
        
    }
    
    public void RegisterCommands(Type type)
    {
        MethodInfo[] classMethods = type.GetMethods();

        // Loop through all methods in this class that are in the
        for (int i = 0; i < classMethods.Length; i++)
        {
            var pattr = (PipelineOperator)Attribute.GetCustomAttribute(classMethods[i], typeof(PipelineOperator));
            if (pattr != null)
            {
                var method = classMethods[i];
                RegisterPipelineMethod(pattr, method);
            }

            var fattr = (FilterOperator)Attribute.GetCustomAttribute(classMethods[i], typeof(FilterOperator));
            if (fattr != null)
            {
                var method = classMethods[i];
                RegisterFilterMethod(fattr, method);
            }

            var piattr = (PipeInputOperator)Attribute.GetCustomAttribute(classMethods[i], typeof(PipeInputOperator));
            if (piattr != null)
            {
                var method = classMethods[i];
                RegisterPipeInputMethod(piattr, method);
            }
        }
    }

    //todo: dictionaries for only-on filters/pipes/etc. Right now we can only have one 'length' defined... but |length should convert to a number, and ~length 10 should match items. and strings and lists should both have ~length! so we need multiple methods.
    //if the type is different, it should be fine; and we can figure it out! dictionaries in dictionaries. can't wait for this thing to be slow as shit lol.
        //i think figuring out the "input->output" types at compile-time and tracking the validity of it is going to have to be a v1.0 thing, but im happy to kick that (very annoying) can down the road.
    private void RegisterFilterMethod(FilterOperator att, MethodInfo method)
    {
        if (att.OnlyValidOn != null)
        {
            if (method.GetParameters()[0].ParameterType != att.OnlyValidOn)
            {
                throw new Exception($"unable to register filter operator {att.Name} ({method.Name}). Type does not match attribute valid type.");
            }

            BuiltinFilters.FilterProviders.Add(att.Name,
                new Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, bool>>(a =>
                {
                    return (args, item) =>
                    {
                        if (item.GetType() == att.OnlyValidOn)
                        {
                            return (bool)method.Invoke(null, [item, args]);
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
            BuiltinFilters.FilterProviders.Add(att.Name,
                new Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, bool>>(a =>
                {
                    return (args, item) => (bool)method.Invoke(null, [item, args]);
                }));
        }
    }

    private void RegisterPipeInputMethod(PipeInputOperator att, MethodInfo method)
    {
        if (att.OnlyValidOn != null)
        {
            if (method.GetParameters()[0].ParameterType != att.OnlyValidOn)
            {
                throw new Exception(
                    $"unable to register pipein operator {att.Name} ({method.Name}). Type does not match attribute valid type.");
            }

            AllPipeInputProviders.Add(att.Name,
                new Func<Dictionary<string, PKItem>, Func<PKItem, PKItem[], IEnumerable<PKItem>>>((opts)=>
                {
                    return (args, item) =>
                    {
                        if (item.GetType() == att.OnlyValidOn)
                        {
                            return (IEnumerable<PKItem>)method.Invoke(null, [item, args]);
                        }
                        else
                        {
                            throw new Exception(
                                $"Cannot call pipein |>{att.Name} on item of type {item.GetType()}. {att.Name} only operates on {att.OnlyValidOn}.");
                        }
                    };
                }));
        }
        else
        {
            AllPipeInputProviders.Add(att.Name, new Func<Dictionary<string, PKItem>, Func<PKItem, PKItem[], IEnumerable<PKItem>>>((opts) =>
            {
                return (a, args)=>
                {
                    return (IEnumerable<PKItem>)method.Invoke(null, [a,args]);
                };
            }));
        }
    }

    private void RegisterPipelineMethod(PipelineOperator att, MethodInfo method)
    {
        if (att.OnlyValidOn != null)
        {
            if (method.GetParameters()[0].ParameterType != att.OnlyValidOn)
            {
                throw new Exception(
                    $"unable to register pipeline operator {att.Name} ({method.Name}). Type does not match attribute valid type.");
            }

            NativePipes.PipelineProviders.Add(att.Name,
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
            NativePipes.PipelineProviders.Add(att.Name,
                new Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, PKItem>>(a =>
                {
                    return (args, item) => { return (PKItem)method.Invoke(item, args); };
                }));
        }
    }

    #region Runtime Method Access
    //_env loads our plugins and stuff. 

    //todo: split compile time (options) and runtime (arguments). we can get the input provider getter function, and then invoke that at runtime?

    public IPKInputProvider GetPipeInputProvider(string callName, Dictionary<string, PKItem>? options = null)
    {
        if (AllPipeInputProviders.TryGetValue(callName, out var provider))
        {
            TraversalOrder order = TraversalOrder.ItemByItem;

            //this is during compiliation. we pass in options, but we can handle traversalOrder universally for all [pipe] input providers.
            //todo: helper for getting properties from the dict
            if (options != null && options.TryGetValue("order", out var orderVal))
            {
                //string or identifier should become tostring
                var orderProvided = orderVal.ToString()?.ToLower();
                if (orderProvided == "item")
                {
                    order = TraversalOrder.ItemByItem;
                }
                else if (orderProvided == "command")
                {
                    order = TraversalOrder.CommandByCommand;
                }
            }
            
            var result = provider.Invoke(options);
            return new GenericPipelineInputProvider(result, order);
        }

        throw new Exception("Unknown Input Provider '" + callName + $"'. Supported names are: (todo)");
    }
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

    public RuntimeProcess GetPipelineCommand(string pipeName, RuntimeExpression[] arguments, Dictionary<string, PKItem>? options)
    {
        if (NativePipes.PipelineProviders.TryGetValue(pipeName, out Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, PKItem>> pipeFunc))
        {
            return new PipelineProcess(arguments, pipeFunc.Invoke(options));
        }else if (NativePipes.OnContextPipelineProviders.TryGetValue(pipeName, out var contextPipeFunc))
        {
            return new OnContextPipelineProcess(arguments, contextPipeFunc.Invoke(options));
        }

        //todo: replace all this with our poll-environment-for-supported in/out etc; the thing we will later use to write a gui...
        throw new Exception("Unknown Pipe '" + pipeName + $"'. Supported pipe operations: {NativePipes.PipelineProviders.KeyListString()}");
    }

    #endregion
}