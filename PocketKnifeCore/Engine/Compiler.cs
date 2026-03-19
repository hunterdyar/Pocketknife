using System.Diagnostics;

namespace PocketKnifeCore.Engine;

public class Compiler
{
    private Environment _env;
    
    public void CompileScript(PKScriptNode scriptNode)
    {
        _env = new Environment();
        string input = "";
        foreach (var rootNode in scriptNode.RootNodes)
        {
            Walk(rootNode, new Context(_env,new PKString(input)));
        }
    }
    
    private void Walk(RootNode node, Context context)
    {
        switch (node)
        {
            case InputBranchNode inputBranch:
                Walk(inputBranch.Input, context);
                if (_env.TryGetNextInput(out var provider))
                {
                    if (provider.TraversalOrder == TraversalOrder.ItemByItem)
                    {
                        foreach (var item in provider.Enumerate())
                        {
                            var newContext = new Context(_env, item);
                            _env.PushContext(newContext);
                            foreach (var command in inputBranch.Commands)
                            {
                                Walk(command, newContext);
                            }

                            _env.PopContext();
                        }
                    }else if (provider.TraversalOrder == TraversalOrder.CommandByCommand)
                    {
                        throw new NotImplementedException();
                        //todo: put contexts into an array to grab them.
                        // foreach (var command in inputBranch.Commands)
                        // {
                        //     foreach (var item in provider.Enumerate())
                        //     {
                        //         var newContext = new Context(_env, item);
                        //         _env.PushContext(newContext);
                        //         Walk(command, newContext);
                        //         _env.PopContext();
                        //     }
                        // }

                    }
                    //else if order = command-by-command
                }
                else
                {
                    throw new Exception("input branch has no input command? or that command failed.");
                }
                break;
            case BranchNode branch:
                //walk the branch.
                var c = context.PushDuplicate();
                _env.PushContext(c);
                foreach (var nodes in branch.Commands)
                {
                    Walk(nodes,c);
                }
                _env.PopContext();
                context = c;
                break;
            case InputProviderNode inputProvider:
                //create a new context set and run it.
                //>dir directoryPathString
                //go to our dictionary of InputProviders, which should take the arguments and return an IPKInputProvider
                    //so >dir path returns a PKDirectoryInfo(new DirectoryInfo(path))
                var arguments = WalkArguments(inputProvider.Arguments);
                //if no provided arguments (>dir) then we pull from pipeline? todo: this should be |>dir actually?
                if (arguments.Length == 0)
                {
                    arguments = new []{context.Item};
                }

                var options = WalkOptions(inputProvider.Options);
                var input = _env.GetInputProvider(inputProvider.Name, arguments, options);
                //push it on the stack. Then start enumerating!
                _env.PushInputProvider(input);
                //create a new context from the source and start enumerating the pkitems.
                break;
            case PipeOutNode pipeOutCommand:
                //there should be one argument which is a label expression. 
                //that's not enforced by the parser, because i'm... not sure it's true!
                Console.WriteLine($"|<{pipeOutCommand.ExplicitCommand} on {context}");
                break;
            case PipelineCommandNode pipelineCommand:
                //call transformation and pass in the context object.
                Console.WriteLine($"|{pipelineCommand.Name} on {context}");
                break;
            case FilterCommandNode filterCommand:
                Console.WriteLine($"~{filterCommand.Name} on {context}");
                var filterArgs = WalkArguments(filterCommand.Arguments);
                var filterOpts = WalkOptions(filterCommand.Options);
                var filter = _env.GetFilterCommand(filterCommand.Name, filterArgs, filterOpts);
                
                //now we have the filter, but we don't need to process it for every single item.
                //i was trying to punt doing a "compile" stage, but it's needed. I mean, we need to check names of things even if theyre in unreached branches too.
                
                //we only need to do this part:
                context.KeepProcessing = filter.Invoke(context.Item);
                
                break;
            case SignalCommandNode signalCommand:
                Console.WriteLine($":{signalCommand.Name} on {context}");
                break;
            case PipeSetLabelNode setLabel:
                Console.WriteLine($"|= Setting Label {setLabel.LabelNode.Name}");
                break;
           default:
                throw new Exception($"Unhandled node {node}");
                break;
            
        }
    }

    private Dictionary<string, PKItem>? WalkOptions(List<KeyValuePairNode>? parsedOptions)
    {
        Dictionary<string, PKItem> options = null;
        if (parsedOptions != null)
        {
            var opts = parsedOptions.Select(x=> new KeyValuePair(x.Key,WalkExpression(x.Value))).ToArray();
            options = new Dictionary<string, PKItem>();
            foreach (var opt in opts)
            {
                options[opt.Key] = opt.Value;
            }
            //todo: convert to key/value pairs as a runtime?
        }

        return options;
    }

    private PKItem[] WalkArguments(ExpressionNode[] arguments)
    {
        return arguments.Select(x=>WalkExpression(x)).ToArray();
    }

    private PKItem WalkExpression(ExpressionNode expressionNode)
    {
        switch (expressionNode)
        {
            case IdentifierNode identifier:
                return new PKString(identifier.Name);
            case StringLiteralNode stringLiteral:
                return new PKString(stringLiteral.Value);
            case NumberNode number:
                return new PKNumber(number.Value);
            case LabelNode label:
                throw new NotImplementedException("label value lookup not yet implemented");
            default:
                throw new Exception($"Unhandled node {expressionNode}");
        }
    }

    private void WalkCommand(Command command)
    {
        switch (command)
        {
            
        }
    }
}