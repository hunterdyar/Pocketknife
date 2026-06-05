namespace PocketknifeCore;

public class PKInputProvider : PKNode
{
	private OpInvoker _invoker;
	private string _name;
	public PKInputProvider(string opName, OpInvoker invoker)
	{
		_name = opName;
		_invoker = invoker;
	}
}