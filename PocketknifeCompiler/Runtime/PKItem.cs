namespace PocketknifeCore;

// One value cell flowing through the layered runtime.
// Bindings are lazily allocated.
public sealed class PKItem
{
	public object? Value;
	public PKItem? Progenitor; // an Item in the previous layer. Let's us walk up and resolve variables. "progenitor" is a better name than "daddy"
	//named branches, @Index/@Count, etc. 
	public Dictionary<string, object>? Bindings;
	public int Index;
	public int? ArmID = null;
	
	public PKItem(object? value, PKItem? progenitor = null, int index = 0)
	{
		Value = value;
		Progenitor = progenitor;
		Index = index;
	}

	public void Bind(string name, object value)
	{
		Bindings ??= new Dictionary<string, object>();
		Bindings[name] = value;
	}
	
	public bool TryGetValue(string name, out object value)
	{
		//I think this string comparison check is cheaper than loading a dictionary for every single variable, even when they don't have bindings?
		//we lose the lazy allocation of the dictionary if it contains Index... idk what's more performant.
		if (name == "Index")
		{
			value = Index;
			 return true;
		}

		if (Bindings == null)
		{
			value = default;
			return false;
		}
		
		value = default;
		if (Bindings != null && Bindings.TryGetValue(name, out value))
		{
			return true;
		}
		return false;
	}
}
