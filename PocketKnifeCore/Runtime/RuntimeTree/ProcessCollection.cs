using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public abstract class ProcessCollection : RuntimeProcess
{
	public List<RuntimeProcess> Commands = new List<RuntimeProcess>();
	private List<string> _namedBranches = new List<string>();
	public ProcessCollection? Parent = null;

	protected ProcessCollection(RuntimeExpression[] arguments, ProcessCollection? parent) : base(arguments)
	{
		Parent = parent;
	}

	public void AddProcess(RuntimeProcess rp)
	{
		Commands.Add(rp);
	}

	public void RegisterNamedBranch(string label, SubBranch sb)
	{
		_namedBranches.Add(label);// to be dictionary?
	}

	public bool IsValidLabel(string label)
	{
		if (_namedBranches.Contains(label))
		{
			return true;
		}
		else
		{
			if (Parent != null)
			{
				return Parent.IsValidLabel(label);
			}
			else
			{
				return false;
			}
		}
	}
}