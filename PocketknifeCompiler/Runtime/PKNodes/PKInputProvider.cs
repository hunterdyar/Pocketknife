namespace PocketknifeCore;

public class PKInputProvider : PKNode
{
	public PKType Type => _type;
	private PKType _type;
	public GenInvoker Generator => _generator;
	private GenInvoker _generator;
	public string Name => _name;
	private string _name;
	
	public Arguments Arguments => _arguments;
	private Arguments _arguments;
	public PKInputProvider(PKType type, string opName, GenInvoker generator, Arguments arguments)
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