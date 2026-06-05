namespace PocketknifeCore;

public class PKInputProvider : PKNode
{
	public PKKind Kind => _kind;
	private PKKind _kind;
	public GenInvoker Generator => _generator;
	private GenInvoker _generator;
	public string Name => _name;
	private string _name;
	public PKInputProvider(PKKind kind, string opName, GenInvoker generator)
	{
		_name = opName;
		_generator = generator;
	}

	public override string ToString()
	{
		return $"PKInputProvider({_name})";
	}
}