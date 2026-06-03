namespace PocketKnife.Compiler;

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

public class CommandSetNode
{
    public List<RootNode> Commands;
    public List<string> BranchNames = new List<string>();

    public CommandSetNode(List<RootNode> commands)
    {
        Commands = commands;
    }
}
public class InputBranchNode : RootNode
{
    public InputProviderNode Input;
    public CommandSetNode CommandSet;

    public InputBranchNode(InputProviderNode input, List<RootNode> commands)
    {
        Input = input;
        CommandSet = new CommandSetNode(commands);
    }
}

public enum BranchType
{
    Unknown,
    SideEffect,
    ListAppend,
    Replace,
}
public class BranchNode : RootNode
{
    public BranchType Type;
    public CommandSetNode Commands;
    public bool HasLabel = false;
    public string Label = "";
    public BranchNode(string label, BranchType type,List<RootNode> commands)
    {
        this.Type = type;
        Label = label;
        HasLabel = !string.IsNullOrEmpty(label);
        Commands = new CommandSetNode(commands);
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
