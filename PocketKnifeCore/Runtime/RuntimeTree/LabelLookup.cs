using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class LabelLookup : RuntimeExpression
{
	public string Name;

	public LabelLookup(string name)
	{
		Name = name;
	}

	public override PKItem GetValue(Context? calledContext)
	{
		if (calledContext == null)
		{
			throw new Exception("Invalid context to use LabelLookup. Is this expecting a constant?");
		}
		if(calledContext.TryGetNamedBranch(Name, out var item))
		{
			return item;
		}

		throw new Exception($"Cannot get label item of name @{Name}");
	}
}