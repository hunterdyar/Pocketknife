namespace PocketknifeCore.Compiler;

using PocketknifeCore;

public class CompileContext
{
	public Type StackTop => Stack.Peek();
	public Stack<Type> Stack;

	public Dictionary<string, List<Type>> Variables = new Dictionary<string, List<Type>>()
	{
		{ "Index", new List<Type>(1) { typeof(int) } }
	};
	//todo: replace the stack with a frame that has a lazy dictionary, so we can correctly resolve ^@var reachout types.
	
	public CompileContext()
	{
		Stack = new Stack<Type>();
		Stack.Push(PKType.None);
	}

	public void PushType(Type pkType)
	{
		Stack.Push(pkType);
	}

	public void PopType()
	{
		Stack.Pop();
	}

	public void TransformType(Type newType)
	{
		Stack.Pop();
		Stack.Push(newType);
	}

	public void Pack()
	{
		var top = Stack.Pop();
		if (top.IsStream())
		{
			throw new Exception("cannot pack a list. yes. this should work but just isn't implemented yet.");
		}
		Stack.Push(top.Lift());
	}

	public void Unpack()
	{
		var top = Stack.Pop();
		if (!top.IsStream())
		{
			throw new Exception("cannot unpack");
		}

		Stack.Push(top.Lower());
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

	public void AssignNewVariable(string name, Type type)
	{
		if(Variables.ContainsKey(name))
		{
			Variables[name].Add(type);
		}
		else
		{
			Variables[name] = new List<Type>(3) { type };
		}
	}

	public Type GetVariableType(string name, int reachOut)
	{
		if(reachOut == 0)
		{
			return Variables[name][^1];
		}
		else
		{
			return Variables[name][^(1+reachOut)];
		}
	}
	
	public void PushFrame()
	{
		var top = Stack.Peek();
		Stack.Push(top);
	}

	public void ChangeVariableType(string name, int reachOut, Type type)
	{
		Variables[name][^(1 + reachOut)] = type;
	}
}