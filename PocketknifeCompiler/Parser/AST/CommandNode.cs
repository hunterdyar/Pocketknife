using System.Diagnostics;

namespace PocketKnife.Compiler;

//[sigil]identity arg0 arg1 ar2 (opt=options)
//|transform, filter, etc, etc.
public class Options
{
    public bool Any => _kvPairs != null && _kvPairs.Count > 0;//() would parse as empty, but existing, list.
    public List<KeyValuePairNode>? KVPairs => _kvPairs;
    private List<KeyValuePairNode>? _kvPairs;

    public Options(List<KeyValuePairNode>? kvPairs)
    {
        _kvPairs = kvPairs;
    }

    override public string ToString()
    {
        if(_kvPairs == null)
        {
            return "";
        }
        
        return "("+string.Join(", ", _kvPairs)+")";
    }
}
public class CommandNode : RootNode
{
    public virtual string sigil => "";
    public string Name => commandName; 
    private string commandName;
    public ExpressionNode[] Arguments => _arguments;
    private ExpressionNode[] _arguments;

    public Options? Options => _options;
    private Options? _options;
    
    public CommandNode(string name, List<ExpressionNode> args, Options? opts)
    {
        commandName = name;
        _arguments = args.ToArray();
        _options = opts;
    }
    public override string ToString()
    {
        return $"{sigil}{commandName} {string.Join(" ", _arguments)} {(_options != null ? _options : "")}";
    }
}

//x command arg1 arg2... (key=val key2=val2...)

public class InputProviderNode : CommandNode
{
    override public string sigil => ">";
    //>dir input
    public InputProviderNode(string name, List<ExpressionNode> args, Options? opts) : base(name, args, opts)
    {
    }
}

public class InputLiteralProviderNode : InputProviderNode
{
    public InputLiteralProviderNode(List<ExpressionNode> args, Options? opts) : base("", args, opts)
    {
    }

    public override string ToString()
    {
        return ">" + string.Join(" ", Arguments);
    }
}

public class PipelineCommandNode : CommandNode
{
    override public string sigil => "|";
    public PipelineCommandNode(string name, List<ExpressionNode> args, Options? opts) : base(name, args, opts)
    {
    }
}

public class FilterCommandNode : CommandNode
{
    override public string sigil => "~";
    //~do thing
    public FilterCommandNode(string name, List<ExpressionNode> args, Options? opts) : base(name, args, opts)
    {
    }
}

public class AbortCommandNode : CommandNode
{
    public override string sigil => "!";

    public AbortCommandNode(string name, List<ExpressionNode> args, Options? opts) : base(name, args, opts)
    {
    }
}

public class SignalCommandNode : CommandNode
{
    override public string sigil => ":";
    //:start-row
    public SignalCommandNode(string name, List<ExpressionNode> args, Options? opts) : base(name, args, opts)
    {
    }
}

public class PipeInCommandNode : CommandNode
{
    override public string sigil => "|>";
    public List<RootNode> Commands;
    //|>transform data into stream<data>
    public PipeInCommandNode(List<RootNode> commands, string name, List<ExpressionNode> args, Options? opts) : base(name, args, opts)
    {
        Commands = commands;
    }
}