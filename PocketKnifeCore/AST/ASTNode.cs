using System.Linq.Expressions;
using PocketKnifeCore.Parser;

namespace PocketKnifeCore;

public abstract class ASTNode
{
    public SourceSlice Start;
}

public class PKScript : ASTNode
{
    public List<RootNode> RootNodes;

    public PKScript(List<RootNode> nodes)
    {
        RootNodes = nodes;
    }
}

public class RootNode : ASTNode
{
    
}

public class InputBranch : RootNode
{
    public InputProvider Input;
    public List<RootNode> Commands;

    public InputBranch(InputProvider input, List<RootNode> commands)
    {
        Input = input;
        Commands = commands;
    }
}
public class Branch : RootNode
{
    public List<RootNode> Commands;
    public Branch(List<RootNode> commands)
    {
        Commands = commands;
    }
}

public class PipeOut : RootNode
{
    public bool HasExplicitCommand => explicitCommandName != null;
    public string? ExplicitCommand => explicitCommandName;
    private string? explicitCommandName;

    public List<PropertyValuePair> Options => _opts;
    private List<PropertyValuePair> _opts;
    public PipeOut(string name, List<PropertyValuePair>? opts)
    {
        explicitCommandName = name;
        _opts = opts;
    }

    public PipeOut(List<PropertyValuePair>? opts)
    {
        explicitCommandName = null;
        _opts = opts;
    }
    public PipeOut(string name = null)
    {
        explicitCommandName = name;
        _opts = null;
    }
}

public class SignalOut : RootNode
{
    public bool HasExplicitCommand => explicitCommandName != null;
    private string? explicitCommandName;

    public SignalOut(string name = null)
    {
        explicitCommandName = name;
    }
}

public class PipeSetLabel : RootNode
{
    public Label Label => _label;
    private Label _label;

    public PipeSetLabel(Label label)
    {
        _label = label;
    }
}