namespace PocketknifeCore.SimpleEvaluator;

public static class SimpleEvaluator
{
	public static void Evaluate(PKNode node, Context ctx = null)
	{
		switch (node)
		{
			case PKNodeGroup group:
				foreach (var n in group.Nodes)
				{
					Evaluate(n, ctx);
				}
				break;
			case PKInputBranch branch:
				//push the input stream onto the stack.
				Evaluate(branch.Input, ctx);
				
				//take that and operate on it
				Evaluate(branch.Body, ctx);
				
				break;
			case PKInputProvider input:
				var value = input.Generator.Invoke(new ReadOnlySpan<PKValue>(), ctx);
				
				ctx.PushStream(input.Kind,value);
				break;
			case InlineOperatorNode iopr:
				ctx.OperateOnEach(iopr.Invoker);
				break;
		}
	}
}