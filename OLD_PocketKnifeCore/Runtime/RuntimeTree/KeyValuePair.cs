namespace PocketKnifeCore;

public class KeyValuePair
{
    public PKItem Value { get; protected set; }

    public string Key { get; protected set; }

    public KeyValuePair(string key, PKItem value)
    {
        Key = key;
        Value = value;
    }
    
    public virtual T1 GetValue<T1>()
    {
        if (Value is T1 val)
        {
            return val;
        }
        else
        {
            throw new InvalidOperationException("Cannot get value of type " + typeof(T1).FullName);
        }
    }
}

// public class KeyValuePair<T> : KeyValuePair where T : PKItem
// {
//     public KeyValuePair(string key, PKItem value) : base(key, value)
//     {
//     }
// }
