namespace PocketKnifeCore;

[AttributeUsage(AttributeTargets.Method)]
public class LoaderAttribute : Attribute
{
	public string Name => _name;
	private string _name;

	public LoaderAttribute(string name)
	{
		_name = name;
	}
}

