namespace PocketKnifeCore;

public class PKFileInfo : PKItem<FileInfo> 
{
    public PKFileInfo(FileInfo fileInfo) : base(fileInfo)
    {
    }
}

public enum TraversalOrder
{
    ItemByItem,
    CommandByCommand
}
public class PKDirectoryInfo : PKItem<DirectoryInfo>, IPKInputProvider
{
    public TraversalOrder TraversalOrder { get; private set; }
    public PKDirectoryInfo(PKString path, TraversalOrder order)
    {
        TraversalOrder = order;
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