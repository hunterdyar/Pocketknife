namespace PocketknifeCore;

public class PKInputBranch : PKNode
{
	private readonly PKInputProvider _input;
	private readonly List<PKNode> _body;

	public PKInputBranch(PKInputProvider input, List<PKNode> body)
	{
		_input = input;
		_body = body;
	}
}