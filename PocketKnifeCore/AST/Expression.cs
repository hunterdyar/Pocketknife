namespace PocketKnifeCore;

public class Expression : ASTNode
{
    
}

public class Identifier(string source) : Expression
{
    public string Name = source;
}

public class Number : Expression
{
    public double Value;

    public Number(string source)
    {
        Value = Convert.ToDouble(source);
    }
}
public class StringLiteral : Expression
{
    public string Value;
    public StringLiteral(string source)
    {
        Value = source;
    }
}

public class Label : Expression
{
    public string Name;
    public Label(string source)
    {
        Name = source;
    }
}

//a=b
public class PropertyValuePair : Expression
{
    public string Key;
    public Expression Value;

    public PropertyValuePair(string key, Expression expression)
    {
        Key = key;
        Value = expression;
    }
}