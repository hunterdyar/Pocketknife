namespace PocketKnifeCore;

public class ExpressionNode : ASTNode
{
    
}

public class IdentifierNode(string source) : ExpressionNode
{
    public string Name = source;
}

public class NumberNode : ExpressionNode
{
    public double Value;

    public NumberNode(string source)
    {
        Value = Convert.ToDouble(source);
    }
}
public class StringLiteralNode : ExpressionNode
{
    public string Value;
    public StringLiteralNode(string source)
    {
        Value = source;
    }
}

public class LabelNode : ExpressionNode
{
    public string Name;
    public LabelNode(string source)
    {
        Name = source;
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
}