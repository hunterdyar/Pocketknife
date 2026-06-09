namespace PocketknifeCore;

public class PKInputProvider : PKNode
{
	public Type Type => _type;
	private Type _type;
	public GenInvoker Generator => _generator;
	private GenInvoker _generator;
	public string Name => _name;
	private string _name;
	
	public Arguments Arguments => _arguments;
	private Arguments _arguments;
	public PKInputProvider(Type type, string opName, GenInvoker generator, Arguments arguments)
	{
		_type = type;
		_name = opName;
		_generator = generator;
		_arguments = arguments;
	}

	public override string ToString()
	{
		return $"PKInputProvider({_name})";
	}
}