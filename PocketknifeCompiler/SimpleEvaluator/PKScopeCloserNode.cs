namespace PocketknifeCore.SimpleEvaluator;

//Used by the non-recursive tree-walker. Is not part of the compiled AST.
public class PKScopeCloserNode : PKNode
{
	public Action Execute;
	public PKScopeCloserNode(Action execute) : base(default) => Execute = execute;
}

public class PKScopeStateNode :PKNode
{
	public Func<EvalState> Execute;
	public PKScopeStateNode(Func<EvalState> execute) : base(default) => Execute = execute;
}