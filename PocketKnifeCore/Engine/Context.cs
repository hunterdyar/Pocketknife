namespace PocketKnifeCore.Engine;

public class Context
{
	private Environment _environment;
	public List<string> TypeHistory = new List<string>();
	public PKItem Item;
	public bool KeepProcessing = true;
	public Context? Parent;

	private Context()
	{
	}

	public Context(Environment environment, PKItem item)
	{
		_environment = environment;
		Item = item;
	}

	public Context PushDuplicate()
	{
		var newTop = new Context()
		{
			Item = this.Item,
			KeepProcessing = this.KeepProcessing,
			_environment = this._environment,
			Parent = this
		};
		return newTop;
	}

	public override string ToString()
	{
		return "Ctx-top: "+this.Item.ToString();
	}
}