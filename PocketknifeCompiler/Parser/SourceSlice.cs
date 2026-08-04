using PocketknifeCore.Compiler;

namespace PocketknifeCore;

public readonly struct SourceSlice
{
    public readonly int StartLoc;
    public readonly int Length;
    private readonly Lexer _lexer;
    public readonly int Line;
    public readonly int Col;

    public SourceSlice(int startLoc, int length, int line, int col, Lexer lexer)
    {
        StartLoc = startLoc;
        Length = length;
        _lexer = lexer;
        Line = line;
        Col = col;
    }

    public string GetString(string source)
    {
        return source.Substring(StartLoc, Length);
    }
    
    public string PrettyLineCol(bool multiLine = true)
    {
        return _lexer.PrettyLineCol(StartLoc, multiLine);
    }

    override public string ToString()
    {
        return PrettyLineCol(false)+": "+ GetString(_lexer.Source);
    }

    public static SourceSlice Span(SourceSlice start, SourceSlice end)
    {
        if (start.StartLoc > end.StartLoc)
        {
            return Span(end, start);
        }

        if (Equals(start, end))
        {
            return start;
        }
        return new SourceSlice(start.StartLoc, (end.StartLoc+end.Length) - start.StartLoc, start.Line, start.Col, start._lexer);
    }
    
    
}