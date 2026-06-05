namespace PocketknifeCore.Compiler;

public class CompileContext
{
	public PKKind StackTop => Stack.Peek();
	public Stack<PKKind> Stack;

	public CompileContext()
	{
		Stack = new Stack<PKKind>();
		Stack.Push(PKKind.None);
	}

	public void PushType(PKKind pkKind)
	{
		Stack.Push(pkKind);
	}

	public void PopType()
	{
		Stack.Pop();
	}
}