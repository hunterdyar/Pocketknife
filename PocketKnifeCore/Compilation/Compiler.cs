using System.Diagnostics;

namespace PocketKnifeCore.Engine;

public class Compiler
{
    private PluginEnvironment _env;
    private PocketKnifeScript _script;
    
    public void CompileScript(PKScriptNode scriptNode)
    {
        _env = new PluginEnvironment(); //environment gets all of our loaded plugins, the current working directory, etc. We can reuse the script with/without the environment, and vise-versa.
        //we can rerun a compiled script in a few folders, recreating environments. but for now, they are just made at the same time.
        _script = new PocketKnifeScript(); 
        string input = "";
        foreach (var rootNode in scriptNode.RootNodes)
        {
            Walk(rootNode, _script);
        }
    }
    
    private void Walk(RootNode node, ProcessCollection branch)
    {
        switch (node)
        {
            case InputBranchNode inputBranch:
                var b = new PKInputToOutputBranch([], branch);
                Walk(inputBranch.Input, b);//this should set the provider.
               
                //debug
                if (branch is PKInputToOutputBranch iob)
                {
                    Debug.Assert(iob.InputProvider != null);
                }
                
                //add all the commands to this new branch.
                foreach (var command in inputBranch.CommandSet.Commands)
                {
                    Walk(command, b);
                }
                
                branch.AddProcess(b);
                
                break;
            case BranchNode subBranch:
                var sb = new SubBranch([],branch, subBranch.Label);
                if (subBranch.HasLabel)
                {
                    var label = subBranch.Label;
                    branch.RegisterNamedBranch(label, sb);
                }

                //add label to list?
                foreach (var nodes in subBranch.Commands.Commands)
                {
                    Walk(nodes,sb);
                }
                branch.AddProcess(sb);
                break;
            case PipeInCommandNode pipeInCommand:
                //the command is used to get an input provider.
                //we parsed it as a command instead of as an 'inputprovider' node. todo: frankly this is cleaner. should flatten out and get rid of InputProviderNode.
                var piArgs = WalkArguments(pipeInCommand.Arguments, branch);
                var piOpts = WalkOptions(pipeInCommand.Options, branch);
                var piInputProvider = _env.GetPipeInputProvider(pipeInCommand.Name, piOpts);

                var pi = new PKPipeInputToOutputBranch(piArgs, branch);
                pi.SetProvider(piInputProvider);
                foreach (var command in pipeInCommand.Commands)
                {
                    Walk(command, pi);
                }
                branch.AddProcess(pi);
                
                break;
            case InputProviderNode inputProvider:
                //create a new context set and run it.
                //>dir directoryPathString
                //go to our dictionary of InputProviders, which should take the arguments and return an IPKInputProvider
                    //so >dir path returns a PKDirectoryInfo(new DirectoryInfo(path))
                var arguments = WalkArguments(inputProvider.Arguments,branch);
                var options = WalkOptions(inputProvider.Options,branch);
                var input = _env.GetInputProvider(inputProvider.Name, arguments, options);
                //push it on the stack. Then start enumerating!
                if (branch is PKInputToOutputBranch pkio)
                {
                    pkio.SetProvider(input);
                }
                else
                {
                    throw new Exception("parsing error during compilation.");
                }
                //create a new context from the source and start enumerating the pkitems.
                break;
            case PipeOutNode pipeOutCommand:
                //todo pipeout compilation
                //there should be one argument which is a label expression. 
                
                //that's not enforced by the parser, because i'm... not sure it's true!
                Console.WriteLine($"|<{pipeOutCommand.ExplicitCommand}.");
                break;
            case PipelineCommandNode pipelineCommand:
                //call transformation and pass in the context object.
                var pipelineArgs = WalkArguments(pipelineCommand.Arguments,branch);
                var pipelineOpts = WalkOptions(pipelineCommand.Options,branch);
                var pipeline = _env.GetPipelineCommand(pipelineCommand.Name, pipelineArgs, pipelineOpts);
                branch.AddProcess(pipeline);
                break;
            case FilterCommandNode filterCommand:
                Console.WriteLine($"~{filterCommand.Name}.");
                var filterArgs = WalkArguments(filterCommand.Arguments,branch);
                var filterOpts = WalkOptions(filterCommand.Options,branch);
                var filter = _env.GetFilterCommand(filterCommand.Name, filterArgs, filterOpts);
                branch.AddProcess(filter);
                break;
            case SignalCommandNode signalCommand:
                Console.WriteLine($":{signalCommand.Name}.");
                break;
            case PipeSetLabelNode setLabel:
                Console.WriteLine($"|= Setting Label {setLabel.LabelNode.Name}");
                break;
           default:
                throw new Exception($"Unhandled node {node}");
                break;
            
        }
    }

    private Dictionary<string, PKItem>? WalkOptions(List<KeyValuePairNode>? parsedOptions, ProcessCollection parent)
    {
        Dictionary<string, PKItem> options = null;
        if (parsedOptions != null)
        {
            var opts = parsedOptions.Select(x=> new KeyValuePair(x.Key, WalkExpression(x.Value, parent).GetValue(null))).ToArray();
            options = new Dictionary<string, PKItem>();
            foreach (var opt in opts)
            {
                options[opt.Key] = opt.Value;
            }
            //todo: convert to key/value pairs as a runtime?
        }

        return options;
    }

    private RuntimeExpression[] WalkArguments(ExpressionNode[] arguments, ProcessCollection parent)
    {
        return arguments.Select(x=>WalkExpression(x, parent)).ToArray();
    }

    private RuntimeExpression WalkExpression(ExpressionNode expressionNode, ProcessCollection parent)
    {
        switch (expressionNode)
        {
            case IdentifierNode identifier:
                return new Constant(new PKString(identifier.Name));
            case StringLiteralNode stringLiteral:
                return new Constant(new PKString(stringLiteral.Value));
            case NumberNode number:
                return new Constant(new PKNumber(number.Value));
            case LabelNode label:
                if (parent.IsValidLabel(label.Name))
                {
                    return new LabelLookup(label.Name);
                }
                else
                {
                    throw new Exception($"Unable to find label with name @{label.Name}");
                }
                break;
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