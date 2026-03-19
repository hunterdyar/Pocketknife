namespace PocketKnifeCore;

public interface IPKInputProvider
{
    public TraversalOrder TraversalOrder { get; }
    public IEnumerable<PKItem> Enumerate();
}