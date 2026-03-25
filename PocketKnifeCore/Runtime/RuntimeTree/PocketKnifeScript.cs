using System.ComponentModel;
using System.Text;
using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

//The compiled Program lives in here.
public class PocketKnifeScript : ProcessCollection
{
	//todo: don't limit to this as the only option. e.g. generic; but the use case is 'multiple things in a script'.

	public PocketKnifeScript(RuntimeExpression[] arguments, ProcessCollection? parent) : base(arguments, parent)
	{
	}

	public PocketKnifeScript(params RuntimeExpression[] args) : base(args, null)
	{
		
	}

	public override void Execute(Context context)
	{
		//todo: we gotta get input in here, so i think making the script as arguments makes sense.
		EvaluateArguments(context);
		foreach (var process in Commands)
		{
			process.Execute(context);
		}
	}
}

public class PKInputToOutputBranch : ProcessCollection
{
	public IPKInputProvider InputProvider => _inputProvider;
	private IPKInputProvider _inputProvider;
	public List<RuntimeProcess> RootBranches;
	
	public PKInputToOutputBranch(RuntimeExpression[] arguments, ProcessCollection? parent) : base(arguments, parent)
	{
	}
	
	public override void Execute(Context context)
	{
		_inputProvider.SetArguments(false, context, EvaluateArguments(context));

		if (_inputProvider.TraversalOrder == TraversalOrder.ItemByItem)
		{
			foreach (var item in _inputProvider.Enumerate())
			{
				Context c = new Context(item);
				foreach (var process in RootBranches)
				{
					if (c.KeepProcessing)
					{
						process.Execute(c);
					}
				}
			}
		}
		else if (_inputProvider.TraversalOrder == TraversalOrder.CommandByCommand)
		{
			List<Context> contexts = new List<Context>();
			foreach (var item in _inputProvider.Enumerate())
			{
				contexts.Add(new Context(item));
			}

			foreach (var process in RootBranches)
			{
				foreach (var c in contexts.Where(c => c.KeepProcessing))
				{
					process.Execute(c);
				}
			}
		}
		else
		{
			throw new InvalidEnumArgumentException();
		}
	}

	public void SetProvider(IPKInputProvider inputProvider)
	{
		_inputProvider = inputProvider;
	}
	

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine(_inputProvider.ToString());
		foreach (var runtimeProcess in RootBranches)
		{
			sb.AppendLine(runtimeProcess.ToString());
		}

		return sb.ToString();
	}
}
//todo: this code is begging me to change the input provider to an abstract class, and implement 'pipeline' and 'non-pipeline' subclasses.
//then, a single processCollection can handle it without all this duplicated code.
//but, conceptually, this is sound enough for now. It's halfway there with the input provider 'setArguments' now being more of a 'prepare'.
public class PKPipeInputToOutputBranch : ProcessCollection
{
	public PKPipeInputToOutputBranch(RuntimeExpression[] arguments, ProcessCollection? parent) : base(arguments, parent)
	{
		//
	}

	public IPKInputProvider InputProvider => _inputProvider;
	private IPKInputProvider _inputProvider;
	public List<RuntimeProcess> RootBranches;

	public override void Execute(Context context)
	{
		_inputProvider.SetArguments(true, context, EvaluateArguments(context));
		
		if (_inputProvider.TraversalOrder == TraversalOrder.ItemByItem)
		{
			foreach (var item in _inputProvider.Enumerate())
			{
				Context c = new Context(item);
				foreach (var process in RootBranches)
				{
					if (c.KeepProcessing)
					{
						process.Execute(c);
					}
				}
			}
		}
		else if (_inputProvider.TraversalOrder == TraversalOrder.CommandByCommand)
		{
			List<Context> contexts = new List<Context>();
			foreach (var item in _inputProvider.Enumerate())
			{
				contexts.Add(new Context(item));
			}

			foreach (var process in RootBranches)
			{
				foreach (var c in contexts.Where(c => c.KeepProcessing))
				{
					process.Execute(c);
				}
			}
		}
		else
		{
			throw new InvalidEnumArgumentException();
		}
	}

	public void SetProvider(IPKInputProvider inputProvider)
	{
		_inputProvider = inputProvider;
	}


	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine(_inputProvider.ToString());
		foreach (var runtimeProcess in RootBranches)
		{
			sb.AppendLine(runtimeProcess.ToString());
		}

		return sb.ToString();
	}
}