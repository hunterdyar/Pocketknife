using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class SubBranch : ProcessCollection
{

	public string Label => _label;
	private string _label;
	public SubBranch(string label = "") : base()
	{
		_label = label;
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
