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
		//todo: this won't work for List<List<Kind>>
		if (top.IsStream)
		{
			throw new Exception("cannot pack a list. yes. this should work but just isn't implemented yet.");
		}
		Stack.Push(new PKType(top.Kind, true));
	}

	public void Unpack()
	{
		var top = Stack.Pop();
		if (!top.IsStream)
		{
			throw new Exception("cannot unpack");
		}

		Stack.Push(new PKType(top.Kind, false));
	}
}