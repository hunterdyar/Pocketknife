using System.ComponentModel;

namespace PocketKnifeCore.Engine;

public class Interpreter
{
	public void Execute(PocketKnifeScript script, string input)
	{
		var context = new Context(new PKString(input));
		script.Execute(context);
	}

	private void ExecuteRoot(PKInputToOutputBranch ioBranch)
	{
		ioBranch.Execute(new Context(null));//sets the arguments.
		
		if (ioBranch.InputProvider.TraversalOrder == TraversalOrder.ItemByItem)
		{
			foreach (var item in ioBranch.InputProvider.Enumerate())
			{
				Context c = new Context(item);
				foreach (var process in ioBranch.RootBranches)
				{
					if (c.KeepProcessing)
					{
						process.Execute(c);
					}
				}
			}
		}
		else if(ioBranch.InputProvider.TraversalOrder == TraversalOrder.CommandByCommand)
		{
			List<Context> contexts = new List<Context>();
			foreach (var item in ioBranch.InputProvider.Enumerate())
			{
				contexts.Add(new Context(item));
			}

			foreach (var process in ioBranch.RootBranches)
			{
				foreach (var c in contexts.Where(c=>c.KeepProcessing))
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
}