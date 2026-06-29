using PocketknifeCore.Compiler;

namespace PocketknifeCore;

public readonly struct SourceSlice(int startLoc, int length, Lexer lexer)
{
    public readonly int StartLoc = startLoc;
    public readonly int Length = length;
    private readonly Lexer _lexer = lexer;
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
        return new SourceSlice(start.StartLoc, (end.StartLoc+end.Length) - start.StartLoc, start._lexer);
    }
    
    
}