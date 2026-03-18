namespace PocketKnifeCore.Parser;

public class SourceSlice
{
    public int StartLoc;
    public int Length;

    public string GetString(string source)
    {
        return source.Substring(StartLoc, Length);
    }
}