namespace PocketKnifeCore;

[AttributeUsage(AttributeTargets.Method)]
public class PipeInputOperator : Attribute
{
	public string Name => _name;
	private string _name;

	public Type? OnlyValidOn => _onlyValidOn;
	private Type? _onlyValidOn;
	
	public PipeInputOperator(string name, Type? onlyValidOn = null)
	{
		_name = name;
		_onlyValidOn = onlyValidOn;
	}
}