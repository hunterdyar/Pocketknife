using System.Linq.Expressions;
using PocketKnifeCore.Parser;

namespace PocketKnifeCore;

public abstract class ASTNode
{
    public SourceSlice Start;
}

public class PKScriptNode : ASTNode
{
    public List<RootNode> RootNodes;

    public PKScriptNode(List<RootNode> nodes)
    {
        RootNodes = nodes;
    }
}

public class RootNode : ASTNode
{
    
}

public class InputBranchNode : RootNode
{
    public InputProviderNode Input;
    public List<RootNode> Commands;

    public InputBranchNode(InputProviderNode input, List<RootNode> commands)
    {
        Input = input;
        Commands = commands;
    }
}
public class BranchNode : RootNode
{
    public List<RootNode> Commands;
    public BranchNode(List<RootNode> commands)
    {
        Commands = commands;
    }
}

public class PipeOutNode : RootNode
{
    public bool HasExplicitCommand => explicitCommandName != null;
    public string? ExplicitCommand => explicitCommandName;
    private string? explicitCommandName;

    public List<KeyValuePairNode> Options => _opts;
    private List<KeyValuePairNode> _opts;
    public PipeOutNode(string name, List<KeyValuePairNode>? opts)
    {
        explicitCommandName = name;
        _opts = opts;
    }

    public PipeOutNode(List<KeyValuePairNode>? opts)
    {
        explicitCommandName = null;
        _opts = opts;
    }
    public PipeOutNode(string name = null)
    {
        explicitCommandName = name;
        _opts = null;
    }
}

public class SignalOutNode : RootNode
{
    public bool HasExplicitCommand => explicitCommandName != null;
    private string? explicitCommandName;

    public SignalOutNode(string name = null)
    {
        explicitCommandName = name;
    }
}

public class PipeSetLabelNode : RootNode
{
    public LabelNode LabelNode => _labelNode;
    private LabelNode _labelNode;

    public PipeSetLabelNode(LabelNode labelNode)
    {
        _labelNode = labelNode;
    }
}