namespace PocketKnife.Compiler;

public readonly struct Token(Lexer lexer, SourceSlice source, TokenType type)
{
    public readonly Lexer Lexer = lexer;
    public readonly SourceSlice Source = source;
    public readonly TokenType Type = type;

    public string GetSource(string source)
    {
        return Source.GetString(source);
    }

    override public string ToString()
    {
        return (Type)+": "+Source.GetString(Lexer.Source);
    }
}