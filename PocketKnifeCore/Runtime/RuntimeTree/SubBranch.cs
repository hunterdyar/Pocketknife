using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class SubBranch : RuntimeProcess, IProcessCollection
{
	public List<RuntimeProcess> Commands = new List<RuntimeProcess>();

	public string Label => _label;
	private string _label;
	public SubBranch(string label = "") : base()
	{
		_label = label;
	}
	public void AddProcess(RuntimeProcess rp)
	{
		Commands.Add(rp);
	}

	public void SetProvider(IPKInputProvider input)
	{
		throw new NotImplementedException("wrong type of IProcessCollection");
	}

	public override void Execute(Context context)
	{
		var c = context.PushDuplicate();
		context.SetNamedBranch(_label,c);
		foreach (var process in Commands)
		{
			process.Execute(c);
		}
	}
}
