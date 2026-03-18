namespace PocketKnifeCore;

public interface IPKInputProvider
{
    public IEnumerable<PKItem> Enumerate();
}