namespace PocketKnifeCore;

[AttributeUsage(AttributeTargets.Method)]
public class SaverAttribute : Attribute
{
	public string Name => _name;
	private string _name;
	public string DefaultExtension => _ext;
	private string _ext;

	public Type? OnlyValidOn => _onlyValidOn;
	private Type? _onlyValidOn;

	public SaverAttribute(string name, string defaultExtension, Type? onlyValidOn)
	{
		_name = name;
		_ext = defaultExtension;
		_onlyValidOn = onlyValidOn;
	}
}