namespace PocketKnifeCore;

public abstract class PKItem
{
    public abstract string Type { get; }
}

public class PKItem<T> : PKItem
{
    public override string Type => typeof(T).ToString().ToLowerInvariant();
    public T Value;

    public PKItem(T value = default)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

//PKItem has a type, and it has a value, like FileInfo or string