namespace PocketknifeCore;

public class PKNode
{
	
}
public class PKNodeGroup : PKNode
{
	public List<PKNode> Nodes => _nodes;
	private List<PKNode> _nodes;

	public PKNodeGroup(List<PKNode> nodes)
	{
		_nodes = nodes;
	}

	public override string ToString()
	{
		return $"PKNodeGroup({string.Join(", ", _nodes.Select(n => n.ToString()))})";
	}
}