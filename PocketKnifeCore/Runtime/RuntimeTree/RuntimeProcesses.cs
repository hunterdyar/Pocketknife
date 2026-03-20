using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class FilterProcess : RuntimeProcess
{
	private Func<PKItem, bool> _filter;
	public FilterProcess(Func<PKItem, bool> filter)
	{
		_filter = filter;
	}

	public override void Execute(Context context)
	{
		context.KeepProcessing = _filter.Invoke(context.Item);
	}
}

public class PipelineProcess : RuntimeProcess
{
	private Func<PKItem, PKItem> _process;

	public PipelineProcess(Func<PKItem, PKItem> process)
	{
		_process = process;
	}

	public override void Execute(Context context)
	{
		context.Item = _process.Invoke(context.Item);
	}
}

public class SignalProcess : RuntimeProcess
{
	public override void Execute(Context context)
	{
		throw new NotImplementedException();
	}
}
