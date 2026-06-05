namespace PocketknifeCore;

public class PKGeneratorNode : PKNodeGroup
{
	private PKInputProvider _input;
	public PKGeneratorNode(PKInputProvider input, List<PKNode> body) : base(body)
	{
	}

	override public string ToString()
	{
		return $"PKGeneratorNode({_input.ToString()}, +{base.ToString()})";
	}
}