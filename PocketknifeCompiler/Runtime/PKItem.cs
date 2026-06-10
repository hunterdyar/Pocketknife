namespace PocketknifeCore;

// One value cell flowing through the layered runtime.
// Bindings are lazily allocated.
public sealed class PKItem
{
	public object? Value;
	public PKItem? Progenitor; // an Item in the previous layer. Let's us walk up and resolve variables.
	//named branches, @Index/@Count, etc. 
	public Dictionary<string, object>? Bindings;

	public PKItem(object? value, PKItem? progenitor = null)
	{
		Value = value;
		Progenitor = progenitor;
	}

	public void Bind(string name, object value)
	{
		Bindings ??= new Dictionary<string, object>();
		Bindings[name] = value;
	}
}
