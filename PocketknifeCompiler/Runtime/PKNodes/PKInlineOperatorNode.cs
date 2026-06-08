namespace PocketknifeCore;

public class PKInlineOperatorNode : PKNode
{
	public OpInvoker Invoker => _invoker;
	private OpInvoker _invoker;

	public string Name => _name;
	private string _name;

	public Arguments Arguments => _arguments;
	private Arguments _arguments;

	public PKInlineOperatorNode(string name, OpInvoker invoker, Arguments arguments)
	{
		_name = name;
		_invoker = invoker;
		_arguments = arguments;
	}
}

public class PKFilterOperatorNode : PKInlineOperatorNode
{
	public PKFilterOperatorNode(string name, OpInvoker invoker, Arguments arguments) : base(name, invoker, arguments)
	{
	}
}