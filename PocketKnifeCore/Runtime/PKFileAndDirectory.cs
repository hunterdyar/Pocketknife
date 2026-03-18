namespace PocketKnifeCore;

public class PKFileInfo : PKItem<FileInfo> 
{
    public PKFileInfo(FileInfo fileInfo) : base(fileInfo)
    {
    }
}

public class PKDirectoryInfo : PKItem<DirectoryInfo>, IPKInputProvider
{
    public IEnumerable<PKItem> Enumerate()
    {
        foreach (var fileInfo in Value.EnumerateFiles())
        {
            yield return new PKFileInfo(fileInfo);
        }
    }
}