namespace PocketknifeCore;

public class PKNode
{
	
}
public class PKGroupNode : PKNode
{
	private List<PKNode> Nodes;

	public PKGroupNode(List<PKNode> nodes)
	{
		Nodes = nodes;
	}
}