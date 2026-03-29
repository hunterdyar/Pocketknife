using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace PocketKnifeCore.Engine;

public class PluginEnvironment
{
    //todo: should this be static?
    public static readonly Dictionary<string, Loader> AllLoaders = new Dictionary<string, Loader>();//le sigh
    public static readonly Dictionary<string, Saver> AllSavers = new Dictionary<string, Saver>();

    public static readonly Dictionary<string, PipelineMethodsProvider> PipelineMethods = new Dictionary<string, PipelineMethodsProvider>();
    public static readonly Dictionary<string, FilterMethodsWrapper> FilterMethods = new Dictionary<string, FilterMethodsWrapper>();
    public static readonly Dictionary<string, PipeInputsMethodsWrapper> PipeInputMethods = new Dictionary<string, PipeInputsMethodsWrapper>();
    public static readonly Dictionary<string, InputMethodsWrapper> InputMethods = new Dictionary<string, InputMethodsWrapper>();
    
    public static readonly Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem, PKItem[], IEnumerable<PKItem>>>> AllPipeInputProviders = new Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem, PKItem[], IEnumerable<PKItem>>>>(); 
    public PluginEnvironment()
    {
        RegisterOperations(typeof(StringBuiltins));
        RegisterOperations(typeof(FileInfoBuiltins));
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
                AllLoaders.Add(latt.Name, Loader.GetLoader(method));
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
        if (method.GetParameters()[0].ParameterType != att.OnlyValidOn)
        {
            throw new Exception($"unable to register filter operator {att.Name} ({method.Name}). Type does not match attribute valid type.");
        }
        
        var filter = new Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, bool>>(a=>
        {
            return (args, item) =>
            {
                if (item.GetType() == att.OnlyValidOn)
                {
                    return (bool)method.Invoke(null, [item, args]);
                }
                else
                {
                    throw new Exception($"Cannot call {att.Name} on item of type {item.GetType()}. {att.Name} only operates on {att.OnlyValidOn}.");
                }
            };
        });
        
        if (FilterMethods.ContainsKey(att.Name))
        {
            FilterMethods[att.Name].Add(att.OnlyValidOn, filter, att.OnlyValidOn);
        }
        else
        {
            var pm = new FilterMethodsWrapper(att.Name);
            pm.Add(att.OnlyValidOn, filter, att.OnlyValidOn);
            FilterMethods.Add(att.Name, pm);
        }
    }

    private void RegisterPipeInputMethod(PipeInputOperator att, MethodInfo method)
    {
        if (method.GetParameters()[0].ParameterType != att.OnlyValidOn)
        {
            throw new Exception(
                $"unable to register pipein operator {att.Name} ({method.Name}). Type does not match attribute valid type.");
        }

        var pipeInMethod = new Func<Dictionary<string, PKItem>, Func<PKItem, PKItem[], IEnumerable<PKItem>>>((opts)=>
            {
                return (item, args) =>
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
            });

        var genericArgs = method.ReturnType.GetGenericArguments();
        if (genericArgs.Length != 1)
        {
            throw new Exception($"unable to register pipe input method {method.Name}");
        }
        //todo: check if it's IEnumerator more specifically

        //IEnumerator<ProvidedType>
        var providedType = genericArgs[0];

        if (PipeInputMethods.ContainsKey(att.Name))
        {
            PipeInputMethods[att.Name].Add(att.OnlyValidOn, pipeInMethod, providedType);
        }
        else
        {
            var pm = new PipeInputsMethodsWrapper(att.Name);
            pm.Add(att.OnlyValidOn, pipeInMethod, providedType);
            PipeInputMethods.Add(att.Name, pm);
        }
    }

    private void RegisterPipelineMethod(PipelineOperator att, MethodInfo method)
    {
        if (method.GetParameters()[0].ParameterType != att.OnlyValidOn)
        {
            throw new Exception(
                $"unable to register pipeline operator {att.Name} ({method.Name}). Type does not match attribute valid type.");
        }

        var pipeMethod = new Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, PKItem>>(a =>
        {
            return (args, item) =>
            {
                if (item.GetType() == att.OnlyValidOn)
                {
                    return (PKItem)method.Invoke(null, [item, args]);
                }
                else
                {
                    throw new Exception(
                        $"Cannot call {att.Name} on item of type {item.GetType()}. {att.Name} only operates on {att.OnlyValidOn}.");
                }
            };
        });
        

        if (PipelineMethods.ContainsKey(att.Name))
        {
            PipelineMethods[att.Name].Add(att.OnlyValidOn, pipeMethod, method.ReturnType);
        }
        else
        {
            var pm = new PipelineMethodsProvider(att.Name);
            pm.Add(att.OnlyValidOn, pipeMethod, method.ReturnType);
            PipelineMethods.Add(att.Name, pm);
        }
    }

    #region Runtime Method Access
    //_env loads our plugins and stuff. 

    //todo: split compile time (options) and runtime (arguments). we can get the input provider getter function, and then invoke that at runtime?

    public IPKInputProvider GetPipeInputProvider(string callName, Type givenType, Dictionary<string, PKItem>? options = null)
    {
        if (PipeInputMethods.TryGetValue(callName, out var pipeInputMethods))
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

            if (pipeInputMethods.TryGetMethod(givenType, out var func, out var provided))
            {
                var result = func.Invoke(options);
                return new GenericPipelineInputProvider(result, order, provided);
            }
            else
            {
                throw new Exception($"Unable to use input provider {callName}. Wrong input type ({givenType} is invalid. Valid options: {pipeInputMethods.GetValidTypesStringList()}");
            }
        }

        throw new Exception("Unknown Input Provider '" + callName + $"'. Supported names are: (todo)");
    }
    public IPKInputProvider GetInputProvider(string callName, RuntimeExpression[] arguments, Dictionary<string, PKItem>? options = null)
    {
        if (BuiltinInputProviders.InputProviders.TryGetValue(callName, out var provider))
        {
            //todo: load isn't showing us the transformation!
            var t = provider.Method.ReturnType;
            return provider.Invoke(arguments, options);
        }

        throw new Exception("Unknown Input Provider '" + callName + $"'. Supported names are: (todo)");
    }
    public FilterProcess GetFilterCommand(string filterName, RuntimeExpression[] arguments, Dictionary<string, PKItem> options, ref PKTypeTracker typeTracker)
    {
        if (BuiltinFilters.FilterProviders.TryGetValue(filterName, out Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, bool>> filter))
        {
            return new FilterProcess(arguments, filter.Invoke(options));
        }else if (FilterMethods.TryGetValue(filterName, out var fmethods))
        {
            var t = typeTracker.Current;
            if (fmethods.TryGetMethod(t, out var func, out var ret))
            {
                typeTracker.Filter(t);
                Debug.Assert(t == ret);//we know this is true, just from how we do registering.
                return new FilterProcess(arguments, func.Invoke(options));
            }
        }

        //i love extension methods, but WOOF it looks like i just wrote java? what the everloving fuck?
        throw new Exception("Unknown Filter '" + filterName + $"'. Supported names are: {BuiltinFilters.FilterProviders.KeyListString()}");
    }

    public RuntimeProcess GetPipelineCommand(string pipeName, RuntimeExpression[] arguments, Dictionary<string, PKItem>? options, ref PKTypeTracker typeTracker)
    {
        var t = typeTracker.Current;

        if (pipeName == "load")
        {
           return GetPipelineLoadCommand(arguments, options, ref typeTracker);
           
        }else if (PipelineMethods.TryGetValue(pipeName, out var methods))
        {
            if(methods.TryGetMethod(t, out var func, out var ret))
            {
                var process = func.Invoke(options);
                typeTracker.Pipeline(t, ret);
                return new PipelineProcess(arguments, process);
            }//else, try on context below... but we want to throw the right error about 'this name exists but it's the wrong type...'
            else
            {
                throw new Exception("Pipe '" + pipeName + $"' does not support type {typeTracker.Current}. Valid types for {pipeName} are {methods.GetValidTypesStringList()} ");
                Debug.WriteLine($"found pipeline method {pipeName} but didn't find the registered? is the type wrong? we need better errors.");
            }
        }
        
        if (NativePipes.OnContextPipelineProviders.TryGetValue(pipeName, out var contextPipeFunc))
        {
            return new OnContextPipelineProcess(arguments, contextPipeFunc.Invoke(options));
        }

        //todo: replace all this with our poll-environment-for-supported in/out etc; the thing we will later use to write a gui...
        throw new Exception("Unknown Pipe '" + pipeName + $"'. Supported pipe operations: ");
    }

    //literally just |load
    private RuntimeProcess GetPipelineLoadCommand(RuntimeExpression[] arguments, Dictionary<string, PKItem>? options, ref PKTypeTracker typeTracker)
    {
        var t = typeTracker.Current;
        string loadType = "";
        if (arguments[0] is Constant constant)
        {
            if (!constant.GetValueCompileTime().TryGetString(out loadType))
            {
                throw new Exception("invalid type for |load loadtype. loadtype must be a string or an identifier.");
            }
        }
        else
        {
            throw new Exception("LoadType property of |load loadtype must be a compile-time constant. (e.g. no @labels)");
        }

        if (string.IsNullOrEmpty(loadType))
        {
            throw new Exception("missing load type on |load");
        }
            
        Type retType = null;
        if (AllLoaders.TryGetValue(loadType, out var loader))
        {
            retType = loader.ProvidedType;
        }
        else
        {
            throw new Exception($"Invalid or unknown loader {loadType}");
        }
        
        var loadProcess = new Func<PKItem[], PKItem, PKItem>((arguments, item) =>
        {
            //we checked arguments already at compile time.
            if (item is PKFileInfo fileInfo)
            {
                return loader.DoLoad(fileInfo.Value);
            }
            else
            {
                throw new Exception("RUNTIME invalid input type. |load must take a fileInfo type. The type checker failed!");
            }
        });
        
        typeTracker.Pipeline(t, retType);
        return new PipelineProcess(arguments, loadProcess);
    }

    #endregion
}