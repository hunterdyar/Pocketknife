using PocketKnifeCore.Engine;

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
    CommandByCommand,
    Inherit,
}
public class PKDirectoryInfo : PKItem<DirectoryInfo>, IPKInputProvider
{
    public TraversalOrder TraversalOrder { get; private set; }
    public void SetArguments(bool asPipeline, Context context, PKItem[] args)
    {
        if (asPipeline)
        {
            throw new Exception("not implemented");
        }
        
        BuiltinHelpers.CheckArgumentCount(args, 1);
        var path = BuiltinHelpers.GetArgument<PKString>(args[0], "directory path");
        if(path.TryGetString(out var dir))
        {
            Value = new DirectoryInfo(dir);
        }
        else
        {
            throw new InvalidCastException($"Cannot get string cast from {args[0]}");
        }
    }

    public PKDirectoryInfo()
    {
        TraversalOrder = TraversalOrder.Inherit;
    }

    public PKDirectoryInfo(DirectoryInfo info)
    {
        Value = info;
        TraversalOrder = TraversalOrder.Inherit;
    }

    public PKDirectoryInfo(DirectoryInfo info, TraversalOrder order)
    {
        Value = info;
        TraversalOrder = order;
    }

    
    public PKDirectoryInfo(TraversalOrder order)
    {
        TraversalOrder = order;
    }

    public Type ProvidedType => typeof(PKFileInfo);

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