namespace PocketknifeCore;

public class PKInputBranch : PKNode
{
	public PKInputProvider Input => _input;
	private readonly PKInputProvider _input;
	public PKNodeGroup Body => _body;
	private readonly PKNodeGroup _body;

	public PKInputBranch(PKInputProvider input, PKNodeGroup body)
	{
		_input = input;
		_body = body;
	}

	override public string ToString()
	{
		return $"PKInputBranch({_input.ToString()}, {_body.ToString()})";
	}
}