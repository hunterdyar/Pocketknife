namespace PocketknifeCore;

public class PKFrame
{
	public string? Name => _name;
	private string? _name;
	
	public Type Type = PKType.None;
	public List<object> Values = new();

	public PKFrame(string? name = null)
	{
		_name = name;
	}
}