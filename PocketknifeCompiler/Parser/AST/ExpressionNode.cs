using System.Globalization;
using System.Text;
using PocketknifeCore;

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

public class NumberNode : LiteralExpressionNode
{

    public NumberNode(string source) : base(PKValue.FromDouble(Convert.ToDouble(source)))
    {
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

public class StringLiteralNode : LiteralExpressionNode
{
    public StringLiteralNode(string source) : base(PKValue.FromString(source))
    {
    }

    public override string ToString()
    {
        return '"'+ Value.ToString() +'"';
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
public abstract class LiteralExpressionNode : ExpressionNode
{
    public PKValue Value => _value;
    private PKValue _value;

    protected LiteralExpressionNode(PKValue value)
    {
        _value = value;
    }

   
}
public class EmptyListLiteralExpression : LiteralExpressionNode
{
    //FromList<PKValue>
    public EmptyListLiteralExpression() : base(PKValue.FromBool(false))
    {
    }

    public override string ToString() => "[]";
}