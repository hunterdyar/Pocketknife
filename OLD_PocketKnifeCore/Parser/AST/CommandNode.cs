namespace PocketKnifeCore;

public class CommandNode : RootNode
{
    public string Name => commandName; 
    private string commandName;
    public ExpressionNode[] Arguments => _arguments;
    private ExpressionNode[] _arguments;
    
    public List<KeyValuePairNode>? Options => _options;
    private List<KeyValuePairNode>? _options;
    
    public CommandNode(string name, List<ExpressionNode> args, List<KeyValuePairNode>? opts)
    {
        commandName = name;
        _arguments = args.ToArray();
        _options = opts;
    }
}

//x command arg1 arg2... (key=val key2=val2...)

public class InputProviderNode : CommandNode
{
    //>dir input
    public InputProviderNode(string name, List<ExpressionNode> args, List<KeyValuePairNode>? opts) : base(name, args, opts)
    {
    }
}

public class PipelineCommandNode : CommandNode
{
    public PipelineCommandNode(string name, List<ExpressionNode> args, List<KeyValuePairNode>? opts) : base(name, args, opts)
    {
    }
}

public class FilterCommandNode : CommandNode
{
    //~do thing
    public FilterCommandNode(string name, List<ExpressionNode> args, List<KeyValuePairNode>? opts) : base(name, args, opts)
    {
    }
}

public class SignalCommandNode : CommandNode
{
    //:start-row
    public SignalCommandNode(string name, List<ExpressionNode> args, List<KeyValuePairNode>? opts) : base(name, args, opts)
    {
    }
}

public class PipeInCommandNode : CommandNode
{
    public List<RootNode> Commands;
    //:start-row
    public PipeInCommandNode(List<RootNode> commands, string name, List<ExpressionNode> args, List<KeyValuePairNode>? opts) : base(name, args,
        opts)
    {
        Commands = commands;
    }
}