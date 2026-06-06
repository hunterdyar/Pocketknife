namespace PocketknifeCore;

public class PKInputProvider : PKNode
{
	public PKType Type => _type;
	private PKType _type;
	public GenInvoker Generator => _generator;
	private GenInvoker _generator;
	public string Name => _name;
	private string _name;
	public PKInputProvider(PKType type, string opName, GenInvoker generator)
	{
		_type = type;
		_name = opName;
		_generator = generator;
	}

	public override string ToString()
	{
		return $"PKInputProvider({_name})";
	}
}