namespace PocketknifeCore;

public class InlineOperatorNode : PKNode
{
	public OpInvoker Invoker => _invoker;
	private OpInvoker _invoker;

	public string Name => _name;
	private string _name;

	public InlineOperatorNode(string name, OpInvoker invoker)
	{
		_name = name;
		_invoker = invoker;
	}
}
