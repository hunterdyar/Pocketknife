using System.Text;
using PocketknifeCore;

namespace PocketKnife.Compiler;

public abstract class ASTNode
{
    public SourceSlice Start;
    public virtual bool IsBoundary => false;

    public static string BranchTypeToString(BranchType type)
    {
        switch (type)
        {
            case BranchType.SideEffect: return "^";
            case BranchType.ListAppend: return "&";
            case BranchType.Replace: return "<";
            default: return "";
        }
    }
}

public class ScriptNode : ASTNode
{
    public List<RootNode> RootNodes;

    public ScriptNode(List<RootNode> nodes)
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

public class CommandSetNode : ASTNode
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
    public BranchType BranchType;
    public InputBranchNode(InputProviderNode input, BranchType type, List<RootNode> commands)
    {
        this.BranchType = type;
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

        switch (BranchType)
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
        
        sb.AppendLine(ASTNode.BranchTypeToString(Type));
        return sb.ToString();
    }
}

public class PackListNode : RootNode
{
    public override bool IsBoundary => false;

    public override string ToString()
    {
        return "<>";
    }
}

public class UnpackListNode : RootNode
{
    public override bool IsBoundary => false;
    override public string ToString()
    {
        return "><";
    }
}

public abstract class PatternMatch : RootNode
{
    public List<PatternBranchArm> Arms;
    public BranchType CloseType;
    public PatternMatch(List<PatternBranchArm> arms, BranchType closeType)
    {
        Arms = arms;
        CloseType = closeType;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("?");
        foreach (var arm in Arms)
        {
            sb.AppendLine(arm.ToString());
        }
        sb.AppendLine(ASTNode.BranchTypeToString(CloseType));
        return sb.ToString();
    }
}

public class NakedPatternMatch : PatternMatch
{
    public NakedPatternMatch(List<PatternBranchArm> arms, BranchType closeType) : base(arms, closeType)
    {
    }
}

public class PatternExpressionMatch : PatternMatch
{
    public ExpressionNode Expression;

    public PatternExpressionMatch(ExpressionNode expr, List<PatternBranchArm> arms, BranchType closeType) : base(arms, closeType)
    {
        Expression = expr;
    }
}

public class PatternBranchArm : RootNode
{
    public List<RootNode> Commands;
    public BranchType CloseType;
    public bool IsDefault = false;
    public FilterCommandNode? FilterToMatch;
    public PatternBranchArm(FilterCommandNode? filterCommandNode, List<RootNode>? commands, BranchType closeType)
    {
        Commands = commands;
        CloseType = closeType;
        FilterToMatch = filterCommandNode;
        IsDefault = filterCommandNode == null;
    }

    override public string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("+ ");
        sb.AppendLine(FilterToMatch?.ToString() ?? "~~");
        foreach (var command in Commands)
        {
            sb.AppendLine(command.ToString());
        }
        if(CloseType != BranchType.Unknown)
        {
            sb.AppendLine(ASTNode.BranchTypeToString(CloseType));
        }
        return sb.ToString();
        
    }
}