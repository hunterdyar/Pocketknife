using PocketknifeCore.Compiler;

namespace PocketknifeCore;

public class PKNode
{
	public SourceSlice Span;

	public PKNode(SourceSlice span)
	{
		Span = span;
	}
}
public class PKNodeGroup : PKNode
{
	public List<PKNode> Nodes => _nodes;
	private List<PKNode> _nodes;

	public PKNodeGroup(List<PKNode> nodes, SourceSlice span) : base(span)
	{
		_nodes = nodes;
	}

	public override string ToString()
	{
		return $"PKNodeGroup({string.Join(", ", _nodes.Select(n => n.ToString()))})";
	}
}