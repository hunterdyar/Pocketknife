using System.Diagnostics;

namespace PocketKnifeCore.Engine;

public class Compiler
{
    private PluginEnvironment _env;
    public PocketKnifeScript Script => _script;
    private PocketKnifeScript _script;
    
    public PocketKnifeScript CompileScript(PKScriptNode scriptNode)
    {
        _env = new PluginEnvironment(); //environment gets all of our loaded plugins, the current working directory, etc. We can reuse the script with/without the environment, and vise-versa.
        //we can rerun a compiled script in a few folders, recreating environments. but for now, they are just made at the same time.
        _script = new PocketKnifeScript(); 
        string input = "";
        PKTypeTracker tracker = new PKTypeTracker(typeof(PKString));
        foreach (var rootNode in scriptNode.RootNodes)
        {
            Walk(rootNode, _script, ref tracker);
        }

        return _script;
    }
    
    private void Walk(RootNode node, ProcessCollection branch, ref PKTypeTracker typeTracker)
    {
        switch (node)
        {
            case InputBranchNode inputBranch:
                var n = inputBranch.Input;
                var arguments = WalkArguments(n.Arguments, branch);
                var options = WalkOptions(n.Options, branch);
                var input = _env.GetInputProvider(n.Name, arguments, options);
                var b = new PKInputToOutputBranch(arguments, branch, input);
                
                //add all the commands to this new branch.
                
                typeTracker.StartBranch(input.ProvidedType);

                foreach (var command in inputBranch.CommandSet.Commands)
                {
                    Walk(command, b, ref typeTracker);
                }

                typeTracker.EndBranch();
                
                branch.AddProcess(b);
                break;
            case BranchNode subBranch:
                var sb = new SubBranch([],branch, subBranch.Label);
                if (subBranch.HasLabel)
                {
                    var label = subBranch.Label;
                    branch.RegisterNamedBranch(label, sb);
                }
                //just a simple branch...
                typeTracker.StartBranch();
                
                foreach (var nodes in subBranch.Commands.Commands)
                {
                    Walk(nodes,sb, ref typeTracker);
                }
                branch.AddProcess(sb);
                
                typeTracker.EndBranch();

                break;
            case PipeInCommandNode pipeInCommand:
                //the command is used to get an input provider.
                //we parsed it as a command instead of as an 'inputprovider' node. todo: frankly this is cleaner. should flatten out and get rid of InputProviderNode.
                var piArgs = WalkArguments(pipeInCommand.Arguments, branch);
                var piOpts = WalkOptions(pipeInCommand.Options, branch);
                var givenType = typeTracker.Current;
                var piInputProvider = _env.GetPipeInputProvider(pipeInCommand.Name, givenType, piOpts);

                var pi = new PKPipeInputToOutputBranch(piArgs, branch);
                pi.SetProvider(piInputProvider);
                
                typeTracker.StartBranch(givenType, piInputProvider.ProvidedType);
                foreach (var command in pipeInCommand.Commands)
                {
                    Walk(command, pi, ref typeTracker);
                }
                typeTracker.EndBranch();
                
                branch.AddProcess(pi);
                
                break;
            case InputProviderNode inputProvider:
                throw new Exception("this should only get called from within InputToOutput Node");
                break;
            case PipeOutNode pipeOutCommand:
                //todo pipeout compilation. is this just named branches now?
                //there should be one argument which is a label expression? 
                Console.WriteLine($"|<{pipeOutCommand.ExplicitCommand}.");
                throw new NotImplementedException();
                break;
            case PipelineCommandNode pipelineCommand:
                //call transformation and pass in the context object.
                var pipelineArgs = WalkArguments(pipelineCommand.Arguments,branch);
                var pipelineOpts = WalkOptions(pipelineCommand.Options,branch);
                var pipeline = _env.GetPipelineCommand(pipelineCommand.Name, pipelineArgs, pipelineOpts,ref typeTracker);
                branch.AddProcess(pipeline);
                break;
            case FilterCommandNode filterCommand:
                FilterGroup fg;
                switch (filterCommand.Name)
                {
                    //type checking happens inside of the GetCommand function, which is messy but whatever.
                    case "not":
                        fg = GetFilterGroupFromArgument(FilterCombinatorialType.Not, filterCommand.Arguments, branch,ref typeTracker);
                        branch.AddProcess(fg);
                        return;
                    case "any":
                        fg = GetFilterGroupFromArgument(FilterCombinatorialType.Any, filterCommand.Arguments, branch,ref typeTracker);
                        branch.AddProcess(fg);
                        return;
                    case "all":
                        fg = GetFilterGroupFromArgument(FilterCombinatorialType.All, filterCommand.Arguments, branch,ref typeTracker);
                        branch.AddProcess(fg);
                        return;
                    case "none":
                        fg = GetFilterGroupFromArgument(FilterCombinatorialType.None, filterCommand.Arguments, branch,ref typeTracker);
                        branch.AddProcess(fg);
                        return;
                    case "not-all":
                        fg = GetFilterGroupFromArgument(FilterCombinatorialType.NotAll, filterCommand.Arguments, branch,ref typeTracker);
                        branch.AddProcess(fg);
                        return;
                    case "only-one":
                        fg = GetFilterGroupFromArgument(FilterCombinatorialType.OnlyOne, filterCommand.Arguments, branch,ref typeTracker);
                        branch.AddProcess(fg);
                        return;
                    default:
                        break;
                }
                //its not a special filter, just a normal one.
                var filterArgs = WalkArguments(filterCommand.Arguments,branch);
                var filterOpts = WalkOptions(filterCommand.Options,branch);
                var filter = _env.GetFilterCommand(filterCommand.Name, filterArgs, filterOpts, ref typeTracker);
                branch.AddProcess(filter);
                break;
            case SignalCommandNode signalCommand:
                Console.WriteLine($":{signalCommand.Name}.");
                throw new NotImplementedException();
                break;
            case PipeSetLabelNode setLabel:
                Console.WriteLine($"|= Setting Label {setLabel.LabelNode.Name}");
                throw new NotImplementedException();
                break;
           default:
                throw new Exception($"Unhandled node {node}");
                break;
            
        }
    }

    private FilterGroup GetFilterGroupFromArgument(FilterCombinatorialType type, ExpressionNode[] filterCommandArguments, ProcessCollection branch, ref PKTypeTracker tracker)
    {
        if (filterCommandArguments.Length != 1)
        {
            throw new Exception(
                "Combinatorial Filters (not, all, any, etc) must have only one argument, the group: [] of filters.");
            //i guess we can do 'count' eventually. //todo. make a new sublist of the remaining, then WalkExpressions and WalkArguments so it could be used.
        }

        var expr = filterCommandArguments.First();
        if (expr is CommandGroupExpression group)
        {
            //todo: this FilterGroupTemporary thing doesn't need to exist if we can just make something implement AddProcess, then add an extension method to a list of filters.
            var fg =  new FilterGroupTemporaryProcessCollection(branch);
           //i dont think we need to add a brancht to the tracker, because filters are all the same input type?
            foreach (var node in group.CommandNodes)
            {
                Walk(node, fg, ref tracker);
            }
            
            FilterGroup f = new FilterGroup(fg.Filters, type, []);
            return f;
        }
        else
        {
            throw new Exception($"Invalid Argument {expr}. Expected a filter group ([~a])");
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
            case CommandGroupExpression commandGroupExpression:
                throw new Exception("this is a special case that should be handled by the filter node.");
                
            default:
                throw new Exception($"Unhandled node {expressionNode}");
        }
    }

    private void WalkCommand(CommandNode commandNode)
    {
        switch (commandNode)
        {
            
        }
    }
}