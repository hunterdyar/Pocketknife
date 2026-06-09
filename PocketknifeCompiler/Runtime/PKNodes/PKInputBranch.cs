namespace PocketknifeCore;

public class PKInputBranch : PKNode
{
	public PKInputProvider Input => _input;
	private readonly PKInputProvider _input;
	public PKNodeGroup Body => _body;
	private readonly PKNodeGroup _body;
	public BranchType BranchType => _branchType;
	private BranchType _branchType;
	
	public PKInputBranch(PKInputProvider input, PKNodeGroup body, BranchType branchType)
	{
		_input = input;
		_body = body;
		_branchType = branchType;
	}

	override public string ToString()
	{
		return $"PKInputBranch({_input.ToString()}, {_body.ToString()})";
	}
}