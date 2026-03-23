namespace PocketKnifeCore;

public interface IPKInputProvider
{
    public TraversalOrder TraversalOrder { get; }
    public void SetArguments(PKItem[] args);
    public IEnumerable<PKItem> Enumerate();
    //tostring?
}