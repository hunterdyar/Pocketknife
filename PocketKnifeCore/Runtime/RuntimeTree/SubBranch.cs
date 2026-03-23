using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class SubBranch : ProcessCollection
{
	public SubBranch(RuntimeExpression[] arguments, ProcessCollection? parent, string label = "") : base(arguments, parent)
	{
		_label = label;
	}

	public string Label => _label;
	private string _label;


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
