namespace PocketknifeCore;

public class PKGeneratorNode : PKGroupNode
{
	private PKInputProvider _input;
	public PKGeneratorNode(PKInputProvider input, List<PKNode> body) : base(body)
	{
	}
}