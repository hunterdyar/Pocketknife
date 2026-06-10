namespace PocketknifeCore;

// A snapshot of the runtime "stream" at one step in the timeline.
public sealed class PKLayer
{
	public Type Type;
	public List<PKItem> Items;

	public PKLayer(Type type)
	{
		Type = type;
		Items = new List<PKItem>();
	}

	public PKLayer(Type type, List<PKItem> items)
	{
		Type = type;
		Items = items;
	}

	// Produce an empty next layer with the same type. Caller fills Items.
	public static PKLayer NextFrom(PKLayer previous)
	{
		return new PKLayer(previous.Type);
	}
}
