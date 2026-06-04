namespace PocketKnife.Compiler;

public readonly struct SourceSlice(int startLoc, int length)
{
    public readonly int StartLoc = startLoc;
    public readonly int Length = length;

    public string GetString(string source)
    {
        return source.Substring(StartLoc, Length);
    }
}