namespace PocketknifeCore;

public class PKFrame
{
	public string? Name => _name;
	private string? _name;
	
	public Type Type = PKType.None;
	public Stream Stream;
	
	public PKFrame(string? name = null)
	{
		_name = name;
	}
}