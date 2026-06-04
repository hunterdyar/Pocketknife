using System.Text;

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

    override public string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var node in RootNodes)
        {
            sb.AppendLine(node.ToString());
        }
        return sb.ToString();
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
    public BranchType Type;
    public InputBranchNode(InputProviderNode input, BranchType type, List<RootNode> commands)
    {
        this.Type = type;
        Input = input;
        CommandSet = new CommandSetNode(commands);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(Input.ToString());
        foreach (var command in CommandSet.Commands)
        {
            sb.AppendLine(command.ToString());
        }

        switch (Type)
        {
            case BranchType.SideEffect:
                sb.AppendLine("^");
                break;
            case BranchType.ListAppend:
                sb.AppendLine("&");
                break;
            case BranchType.Replace:
                sb.AppendLine("<");
                break;
        }
        return sb.ToString();
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
    public BranchNode(LabelNode? label, BranchType type,List<RootNode> commands)
    {
        this.Type = type;
        if (label == null)
        {
            Label = "";
            HasLabel = false;
        }
        else
        {
            //if name is empty, it's still not a label then. like a naked @ maybe? that should err i guess.
            Label = label.Name;
            HasLabel = !string.IsNullOrEmpty(label.Name);
        }
        Commands = new CommandSetNode(commands);
    }

    override public string ToString()
    {
        StringBuilder sb = new StringBuilder();
        if (HasLabel)
        {
            sb.AppendLine(".@"+Label);
        }
        else
        {
            sb.AppendLine(".");
        }
        foreach (var command in Commands.Commands)
        {
            sb.AppendLine(command.ToString());
        }

        switch (Type)
        {
            case BranchType.SideEffect:
                sb.AppendLine("^");
                break;
            case BranchType.ListAppend:
                sb.AppendLine("&");
                break;
            case BranchType.Replace:
                sb.AppendLine("<");
                break;
        }
        return sb.ToString();
    }
}

public class PackListNode : RootNode
{
    public override string ToString()
    {
        return "<>";
    }
}

public class UnpackListNode : RootNode
{
    override public string ToString()
    {
        return "><";
    }
}
