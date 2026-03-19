namespace PocketKnifeCore;

public class Command : RootNode
{
    public string Name => commandName; 
    private string commandName;
    public ExpressionNode[] Arguments => _arguments;
    private ExpressionNode[] _arguments;
    
    public List<KeyValuePairNode>? Options => _options;
    private List<KeyValuePairNode>? _options;
    
    public Command(string name, List<ExpressionNode> args, List<KeyValuePairNode>? opts)
    {
        commandName = name;
        _arguments = args.ToArray();
        _options = opts;
    }
}

//x command arg1 arg2... (key=val key2=val2...)

public class InputProviderNode : Command
{
    //>dir input
    public InputProviderNode(string name, List<ExpressionNode> args, List<KeyValuePairNode>? opts) : base(name, args, opts)
    {
    }
}

public class PipelineCommandNode : Command
{
  
    //
    public PipelineCommandNode(string name, List<ExpressionNode> args, List<KeyValuePairNode>? opts) : base(name, args, opts)
    {
    }
}

public class FilterCommandNode : Command
{
    //~do thing
    public FilterCommandNode(string name, List<ExpressionNode> args, List<KeyValuePairNode>? opts) : base(name, args, opts)
    {
    }
}

public class SignalCommandNode : Command
{
    //:start-row
    public SignalCommandNode(string name, List<ExpressionNode> args, List<KeyValuePairNode>? opts) : base(name, args, opts)
    {
    }
}