namespace PocketknifeCore;

public class PKInputBranch : PKNode
{
	private readonly PKInputProvider _input;
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