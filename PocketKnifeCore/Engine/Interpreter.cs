using System.Diagnostics;

namespace PocketKnifeCore.Engine;

public class Interpreter
{
    private Environment _env;
    
    public void RunScript(PKScript script)
    {
        _env = new Environment();
        string input = "";
        foreach (var rootNode in script.RootNodes)
        {
            Walk(rootNode, new Context(_env,new PKString(input)));
        }
    }
    
    private void Walk(RootNode node, Context context)
    {
        switch (node)
        {
            case InputBranch inputBranch:
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
            case Branch branch:
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
            case InputProvider inputProvider:
                //create a new context set and run it.
                //>dir directoryPathString
                //go to our dictionary of InputProviders, which should take the arguments and return an IPKInputProvider
                    //so >dir path returns a PKDirectoryInfo(new DirectoryInfo(path))
                var arguments = inputProvider.Arguments.Select(x=>WalkExpression(x)).ToArray();
                Dictionary<string, PKItem> options = null;
                //if no provided arguments (>dir) then we pull from pipeline? todo: this should be |>dir actually?
                if (arguments.Length == 0)
                {
                    arguments = new []{context.Item};
                }
                if (inputProvider.Options != null)
                {
                    var opts = inputProvider.Options.Select(x=> new KeyValuePair(x.Key,WalkExpression(x.Value))).ToArray();
                    options = new Dictionary<string, PKItem>();
                    foreach (var opt in opts)
                    {
                        options[opt.Key] = opt.Value;
                    }
                    //todo: convert to key/value pairs as a runtime?
                }
                var input = _env.GetInputProvider(inputProvider.Name, arguments, options);
                //push it on the stack. Then start enumerating!
                _env.PushInputProvider(input);
                //create a new context from the source and start enumerating the pkitems.
                break;
            case PipeOut pipeOutCommand:
                //there should be one argument which is a label expression. 
                //that's not enforced by the parser, because i'm... not sure it's true!
                Console.WriteLine($"|<{pipeOutCommand.ExplicitCommand} on {context}");
                break;
            case PipelineCommand pipelineCommand:
                //call transformation and pass in the context object.
                Console.WriteLine($"|{pipelineCommand.Name} on {context}");
                break;
            case FilterCommand filterCommand:
                Console.WriteLine($"~{filterCommand.Name} on {context}");
                //we need to the current iteration list now according to the func<bool> invoked
                break;
            case SignalCommand signalCommand:
                Console.WriteLine($":{signalCommand.Name} on {context}");
                break;
            case PipeSetLabel setLabel:
                Console.WriteLine($"|= Setting Label {setLabel.Label.Name}");
                break;
           default:
                throw new Exception($"Unhandled node {node}");
                break;
            
        }
    }

    private PKItem WalkExpression(Expression expression)
    {
        switch (expression)
        {
            case Identifier identifier:
                return new PKString(identifier.Name);
            case StringLiteral stringLiteral:
                return new PKString(stringLiteral.Value);
            case Number number:
                return new PKNumber(number.Value);
            case Label label:
                throw new NotImplementedException("label value lookup not yet implemented");
            default:
                throw new Exception($"Unhandled node {expression}");
        }
    }

    private void WalkCommand(Command command)
    {
        switch (command)
        {
            
        }
    }
}