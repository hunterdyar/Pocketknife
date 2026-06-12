namespace PocketknifeCore;

public class PKPatternMatch : PKNode
{
	public List<PKPatternFilterMatchBranch> Branches;
	public PKPatternBranch? Alternate;
	
	//the optional branch endings must match the 'last' ending.
	
	public BranchType BranchType;
	//we have different types of pattern matches?
	
	public PKPatternMatch(List<PKPatternFilterMatchBranch> branches, PKPatternBranch? alternate, BranchType branchType)
	{
		Branches = branches;
		Alternate = alternate;
		BranchType = branchType;
	}
}

public class PKPatternBranch : PKNode
{
	public PKNodeGroup Body;
	public BranchType CloseType;
	public PKPatternBranch(PKNodeGroup body, BranchType closeType)
	{
		Body = body;
		CloseType = closeType;
	}
}

public class PKPatternFilterMatchBranch : PKPatternBranch
{
	public OpInvoker Filter;
	public Arguments Arguments;
	public PKPatternFilterMatchBranch(OpInvoker filter, Arguments arguments, PKNodeGroup body, BranchType closeType) : base(body, closeType)
	{
		Filter = filter;
		Arguments = arguments;
	}
}

//PKPatternExpressionMatchBranch