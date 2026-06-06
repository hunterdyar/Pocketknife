using PocketKnife.Compiler;

namespace PocketknifeCore;

public class PKBranch : PKNode
{
	public PKNodeGroup Body => _body;
	private PKNodeGroup _body;
	
	public BranchType Type => _type;
	private BranchType _type;
	public PKBranch(PKNodeGroup body, BranchType type)
	{
		_body = body;
		_type = type;
	}
}

public class PKNamedBranch : PKBranch
{
	public string Label { get; }
	public PKNamedBranch(string label, PKNodeGroup body, BranchType type) : base(body, type)
	{
		Label = label;
	}
}