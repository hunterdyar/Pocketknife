namespace PocketKnifeCore;

public class Command : RootNode
{
    public string Name => commandName; 
    private string commandName;
    public Expression[] Arguments => _arguments;
    private Expression[] _arguments;
    
    public List<PropertyValuePair>? Options => _options;
    private List<PropertyValuePair>? _options;
    
    public Command(string name, List<Expression> args, List<PropertyValuePair>? opts)
    {
        commandName = name;
        _arguments = args.ToArray();
        _options = opts;
    }
}

//x command arg1 arg2... (key=val key2=val2...)

public class InputProvider : Command
{
    //>dir input
    public InputProvider(string name, List<Expression> args, List<PropertyValuePair>? opts) : base(name, args, opts)
    {
    }
}

public class PipelineCommand : Command
{
  
    //
    public PipelineCommand(string name, List<Expression> args, List<PropertyValuePair>? opts) : base(name, args, opts)
    {
    }
}



public class FilterCommand : Command
{
    //~do thing
    public FilterCommand(string name, List<Expression> args, List<PropertyValuePair>? opts) : base(name, args, opts)
    {
    }
}

public class SignalCommand : Command
{
    //:start-row
    public SignalCommand(string name, List<Expression> args, List<PropertyValuePair>? opts) : base(name, args, opts)
    {
    }
}