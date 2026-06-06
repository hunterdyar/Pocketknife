namespace PocketknifeCore.Compiler;

public class CompileContext
{
	public PKType StackTop => Stack.Peek();
	public Stack<PKType> Stack;

	public CompileContext()
	{
		Stack = new Stack<PKType>();
		Stack.Push(PKType.None);
	}

	public void PushType(PKType pkType)
	{
		Stack.Push(pkType);
	}

	public void PopType()
	{
		Stack.Pop();
	}

	public void Pack()
	{
		var top = Stack.Pop();
		if (top.IsStream)
		{
			throw new Exception("cannot pack a list. yes. this should work but just isn't implemented yet.");
		}
		Stack.Push(top.Lifted());
	}

	public void Unpack()
	{
		var top = Stack.Pop();
		if (!top.IsStream)
		{
			throw new Exception("cannot unpack");
		}

		Stack.Push(top.Lowered());
	}
	
	public void PopFrame(BranchType frameType)
	{
		//nothing currently pops more than a single frame.
		switch (frameType)
		{
			case BranchType.SideEffect:
				//branch result is discarded; restore outer stack unchanged.
				Stack.Pop();
				break;
			case BranchType.ListAppend:
				//outer frame's type is preserved; branch contributes values but not type.
				Stack.Pop();
				break;
			case BranchType.Replace:
				//branch's resulting top type replaces outer top: drop the cloned outer underneath.
				var branchTop = Stack.Pop();
				Stack.Pop();
				Stack.Push(branchTop);
				break;
		}
	}

	public void PushFrame()
	{
		var top = Stack.Peek();
		Stack.Push(top);
	}
}