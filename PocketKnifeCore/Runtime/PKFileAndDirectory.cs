namespace PocketKnifeCore;

public class PKFileInfo : PKItem<FileInfo> 
{
    public PKFileInfo(FileInfo fileInfo) : base(fileInfo)
    {
    }
}

public class PKDirectoryInfo : PKItem<DirectoryInfo>, IPKInputProvider
{
    public PKDirectoryInfo(PKString path)
    {
        Value = new DirectoryInfo(path.Value);
    }

    public IEnumerable<PKItem> Enumerate()
    {
        if (!Value.Exists)
        {
            throw new Exception($"Cannot find path {Value.FullName}");
        }
        foreach (var fileInfo in Value.EnumerateFiles())
        {
            yield return new PKFileInfo(fileInfo);
        }
    }
}