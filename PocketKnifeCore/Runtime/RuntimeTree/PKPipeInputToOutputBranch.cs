using System.ComponentModel;
using System.Text;
using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

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

	public override void Execute(Context context)
	{
		_inputProvider.SetArguments(true, context, EvaluateArguments(context));
		
		if (_inputProvider.TraversalOrder == TraversalOrder.ItemByItem)
		{
			foreach (var item in _inputProvider.Enumerate())
			{
				Context c = new Context(item);
				foreach (var process in Commands)
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

			foreach (var process in Commands)
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
		foreach (var runtimeProcess in Commands)
		{
			sb.AppendLine(runtimeProcess.ToString());
		}

		return sb.ToString();
	}
}