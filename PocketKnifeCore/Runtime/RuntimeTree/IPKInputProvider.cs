using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public interface IPKInputProvider
{
    public TraversalOrder TraversalOrder { get; }
    public void SetArguments(bool asPipeline, Context context, PKItem[] args);
    public IEnumerable<PKItem> Enumerate();
    public Type ProvidedType { get; }
    //tostring?
}