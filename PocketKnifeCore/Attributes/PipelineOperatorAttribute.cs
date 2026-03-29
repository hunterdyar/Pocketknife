namespace PocketKnifeCore;

[AttributeUsage(AttributeTargets.Method)]
public class PipelineOperator : Attribute
{
	public string Name => _name;
	private string _name;
	
	public PipelineOperator(string name)
	{
		_name = name;
	}
}