namespace PocketknifeCore;

public class PKFrame
{
	public string? Name => _name;
	private string? _name;
	
	public PKType Type;
	public List<object> Values;

	public PKFrame(string? name = null)
	{
		_name = name;
	}
}