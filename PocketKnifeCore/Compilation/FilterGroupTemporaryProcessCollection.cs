namespace PocketKnifeCore.Engine;

//todo: we can get rid of this hack, change ProcessCollection to an interface or something, but it's fine for now.
public class FilterGroupTemporaryProcessCollection : ProcessCollection
{
	public List<FilterProcess> Filters = new List<FilterProcess>();
	public FilterGroupTemporaryProcessCollection(ProcessCollection? parent) : base([], parent)
	{
		
	}

	public override void Execute(Context context)
	{
		throw new InvalidOperationException();
	}

	public override void AddProcess(RuntimeProcess rp)
	{
		if (rp is FilterProcess fp)
		{
			Filters.Add(fp);
		}
		else
		{
			throw new Exception($"Error at {rp}. Expected command of type Filter (~)");
			base.AddProcess(rp);
		}
	}
}