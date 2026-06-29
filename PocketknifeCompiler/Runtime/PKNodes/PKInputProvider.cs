namespace PocketknifeCore;

public class PKInputProvider : PKNode
{
	public Type Type => _type;
	private Type _type;

	public string Name => _name;
	protected string _name;
	
	public Arguments Arguments => _arguments;
	private Arguments _arguments;
	public PKInputProvider(Type type, string opName, Arguments arguments, SourceSlice span) : base(span)
	{
		_type = type;
		_name = opName;
		_arguments = arguments;
	}

	public override string ToString()
	{
		return $"PKInputProvider({_name})";
	}
}

public class PKGenInputProvider : PKInputProvider
{
	public GenInvoker Generator => _generator;
	private GenInvoker _generator;

	public PKGenInputProvider(Type type, string opName, Arguments arguments, GenInvoker generator, SourceSlice span) : base(type, opName, arguments, span)
	{
		_generator = generator;
	}

	public override string ToString()
	{
		return $"PKGenInputProvider({_name})";
	}
}