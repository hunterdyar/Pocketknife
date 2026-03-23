using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class Constant :RuntimeExpression
{
	private PKItem _item;

	public Constant(PKItem item)
	{
		this._item = item;
	}

	public override PKItem GetValue(Context calledContext)
	{
		return _item;
	}
}