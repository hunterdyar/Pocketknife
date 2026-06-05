namespace PocketknifeCore;

public class PKNode
{
	
}
public class PKNodeGroup : PKNode
{
	private List<PKNode> Nodes;

	public PKNodeGroup(List<PKNode> nodes)
	{
		Nodes = nodes;
	}

	override public string ToString()
	{
		return $"PKNodeGroup({string.Join(", ", Nodes.Select(n => n.ToString()))})";
	}
}