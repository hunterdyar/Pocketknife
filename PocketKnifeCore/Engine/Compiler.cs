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
            Walk(rootNode, _script.RootInputToOutputBranch);
        }
    }
    
    private void Walk(RootNode node, IProcessCollection branch)
    {
        switch (node)
        {
            case InputBranchNode inputBranch:
                var b = new PKInputToOutputBranch();
                Walk(inputBranch.Input, b);//this should set the provider.
               
                //debug
                if (branch is PKInputToOutputBranch iob)
                {
                    Debug.Assert(iob.InputProvider != null);
                }
                
                //add all the commands to this new branch.
                foreach (var command in inputBranch.Commands)
                {
                    Walk(command, b);
                }
                
                branch.AddProcess(b);
                
                break;
            case BranchNode subBranch:
                //walk the branch.
                var sb = new SubBranch(subBranch.Label);
                foreach (var nodes in subBranch.Commands)
                {
                    Walk(nodes,sb);
                }
                branch.AddProcess(sb);
                break;
            case InputProviderNode inputProvider:
                //create a new context set and run it.
                //>dir directoryPathString
                //go to our dictionary of InputProviders, which should take the arguments and return an IPKInputProvider
                    //so >dir path returns a PKDirectoryInfo(new DirectoryInfo(path))
                var arguments = WalkArguments(inputProvider.Arguments);
                var options = WalkOptions(inputProvider.Options);
                var input = _env.GetInputProvider(inputProvider.Name, arguments, options);
                //push it on the stack. Then start enumerating!
                branch.SetProvider(input);
                //create a new context from the source and start enumerating the pkitems.
                break;
            case PipeOutNode pipeOutCommand:
                //there should be one argument which is a label expression. 
                //that's not enforced by the parser, because i'm... not sure it's true!
                Console.WriteLine($"|<{pipeOutCommand.ExplicitCommand}.");
                break;
            case PipelineCommandNode pipelineCommand:
                //call transformation and pass in the context object.
                var pipelineArgs = WalkArguments(pipelineCommand.Arguments);
                var pipelineOpts = WalkOptions(pipelineCommand.Options);
                var pipeline = _env.GetPipelineCommand(pipelineCommand.Name, pipelineArgs, pipelineOpts);
                branch.AddProcess(pipeline);
                break;
            case FilterCommandNode filterCommand:
                Console.WriteLine($"~{filterCommand.Name}.");
                var filterArgs = WalkArguments(filterCommand.Arguments);
                var filterOpts = WalkOptions(filterCommand.Options);
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
                //this is runtime not compiletime, but we should be able to validate that the branch *exists* somewhere?
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