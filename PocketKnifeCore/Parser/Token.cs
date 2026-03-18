namespace PocketKnifeCore.Parser;

public struct Token
{
    public Lexer Lexer;
    public SourceSlice Source;
    public TokenType Type;

    public string GetSource(string source)
    {
        return Source.GetString(source);
    }

    override public string ToString()
    {
        return (Type)+": "+Source.GetString(Lexer.Source);
    }
}