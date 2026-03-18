namespace PocketKnifeCore;

public class Command : RootNodes
{
    private string commandName;
    private Expression[] Arguments;
    private List<PropertyValuePair>? Options;
    
    public Command(string name, List<Expression> args, List<PropertyValuePair>? opts)
    {
        commandName = name;
        Arguments = args.ToArray();
        Options = opts;
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