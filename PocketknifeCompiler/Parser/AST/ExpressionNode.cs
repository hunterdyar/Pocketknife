using System.Globalization;
using System.Text;

namespace PocketKnife.Compiler;

public class ExpressionNode : ASTNode
{
    
}
//raw ident
public class IdentifierNode(string source) : ExpressionNode
{
    public string Name = source;
    override public string ToString() => Name;
}

public class NumberNode : ExpressionNode
{
    public double Value;

    public NumberNode(string source)
    {
        Value = Convert.ToDouble(source);
    }

    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}

public class StringLiteralNode : ExpressionNode
{
    public string Value;
    public StringLiteralNode(string source)
    {
        Value = source;
    }

    public override string ToString()
    {
        return '"'+Value+'"';
    }
}

public class LabelNode : ExpressionNode
{
    public string Name;
    public int ReachOut => _reachOut;
    int _reachOut;
    public LabelNode(string source, int reachOut = 0)
    {
        Name = source;
        _reachOut = reachOut;
    }

    override public string ToString()
    {
        if (_reachOut == 0)
        {
            return '@'+Name;
        }
        else
        {
            return "@" + new string('^', _reachOut) + Name;
        }
    }
}

//a=b
public class KeyValuePairNode : ExpressionNode
{
    public string Key;
    public ExpressionNode Value;

    public KeyValuePairNode(string key, ExpressionNode expressionNode)
    {
        Key = key;
        Value = expressionNode;
    }
    override public string ToString() => $"{Key}={Value}";
}

public class CommandGroupExpression : ExpressionNode
{
    public List<CommandNode> CommandNodes;

    public CommandGroupExpression(List<CommandNode> nodes)
    {
        CommandNodes = nodes;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[");
        foreach (var command in CommandNodes)
        {
            sb.AppendLine(command.ToString());
        }
        sb.AppendLine("]");
        return sb.ToString();
    }
}