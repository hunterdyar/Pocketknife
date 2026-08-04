using System.Diagnostics;
using System.Globalization;
using System.Text;
using PocketknifeCore;

namespace PocketknifeCore.Compiler;

public class ExpressionNode : ASTNode
{
    public ExpressionNode(SourceSlice span) : base(span)
    {
    }
}
//raw ident
public class IdentifierNode(string source, SourceSlice span) : ExpressionNode(span)
{
    public string Name = source;
    override public string ToString() => Name;
}

public class NumberNode : LiteralExpressionNode
{

    public NumberNode(object source, SourceSlice span) : base(source, span)
    {
    }

    public override string ToString()
    {
        return Value.ToString();
    }
    public static NumberNode FromString(string source, SourceSlice span)
    {
        // if(source.EndsWith("f"))
        // {
        //     return new NumberNode(PKValue.FromFloat(Convert.ToSingle(source)));
        // }
        if (source.Contains('.'))
        {
            var d = Convert.ToDouble(source, CultureInfo.InvariantCulture);
            return new NumberNode(d, span);
        }
        else
        {
            var i = Convert.ToInt32(source, CultureInfo.InvariantCulture);
            return new NumberNode(i, span);
        }
    }
}

public class StringLiteralNode : LiteralExpressionNode
{
    public StringLiteralNode(string source, SourceSlice span) : base(source, span)
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
    public LabelNode(string source,SourceSlice span, int reachOut = 0) : base(span)
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

    public KeyValuePairNode(string key, ExpressionNode expressionNode, SourceSlice span): base(span)
    {
        Key = key;
        Value = expressionNode;
    }
    override public string ToString() => $"{Key}={Value}";
}

public class CommandGroupExpression : ExpressionNode
{
    public List<CommandNode> CommandNodes;

    public CommandGroupExpression(List<CommandNode> nodes, SourceSlice span): base(span)
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
    public object Value => _value;
    private object _value;

    protected LiteralExpressionNode(object value, SourceSlice span) : base(span)
    {
        _value = value;
    }

   
}
public class EmptyListLiteralExpression : LiteralExpressionNode
{
    //FromList<PKValue>
    
    public EmptyListLiteralExpression(SourceSlice span) : base(new List<object>(), span)
    {
    }

    public override string ToString() => "[]";
}