namespace PocketKnifeCore.Engine;

public class PKTypeTracker
{
	public Type Current => _typeStack.Peek();
	private readonly Stack<Type> _typeStack;

	public PKTypeTracker(Type type)
	{
		_typeStack =new Stack<Type>([type]);
	}

	public void Pipeline(Type input, Type output)
	{
		if (input == null || output == null)
		{
			throw new NullReferenceException();
		}
		if (input == _typeStack.Peek())
		{
			if (input != output)
			{
				_typeStack.Pop();
				_typeStack.Push(output);
			}
		}
		else
		{
			throw new Exception($"Invalid type {input}.");
		}
	}

	public void Filter(Type type)
	{
		if (type != _typeStack.Peek())
		{
			throw new Exception($"cannot apply filter of type {type} to {_typeStack}");
		}
	}

	public void StartBranch(Type input, Type branchType)
	{
		if (input == _typeStack.Peek())
		{
			_typeStack.Push(branchType);
		}
	}

	//new fresh branch on top of any type :p
	public void StartBranch(Type branchType)
	{
		_typeStack.Push(branchType);
	}

	public void StartBranch()
	{
		if (_typeStack.Count == 0)
		{
			throw new Exception();
		}
		_typeStack.Push(_typeStack.Peek());
	}


	public void EndBranch()
	{
		if (_typeStack.Count > 1)
		{
			_typeStack.Pop();
		}
		else
		{
			throw new Exception("ended pipeline stack too many times? no typestack to work with.");
		}
	}

	public void Signal(Type type)
	{
		if (type != _typeStack.Peek())
		{
			throw new Exception("broken signal");
		}
	}

	public bool AnyUpTheStack(Type type)
	{
		return _typeStack.Any(x => x == type);
	}
}