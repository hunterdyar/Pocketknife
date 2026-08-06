using PocketknifeCore.Compiler;

namespace PocketknifeCore;

public class PKNode
{
	private static readonly List<PKNode> EmptyChildren = [];
	public SourceSlice Span;

	public PKNode(SourceSlice span)
	{
		Span = span;
	}

	public virtual List<PKNode> GetChildren()
	{
		return EmptyChildren;
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

	public override List<PKNode> GetChildren()
	{
		return _nodes;
	}
}