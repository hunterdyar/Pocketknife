using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class FilterProcess : RuntimeProcess
{
	private Func<PKItem[], PKItem, bool> _filter;
	public FilterProcess(RuntimeExpression[] arguments, Func<PKItem[], PKItem, bool> filter) : base(arguments)
	{
		_filter = filter;
	}

	public override void Execute(Context context)
	{
		var arguments = EvaluateArguments(context);
		context.KeepProcessing = _filter.Invoke(arguments, context.Item);
	}
}

public class PipelineProcess : RuntimeProcess
{
	private Func<PKItem[], PKItem, PKItem> _process;

	public PipelineProcess(RuntimeExpression[] arguments, Func<PKItem[], PKItem, PKItem> process) : base(arguments)
	{
		_process = process;
	}

	public override void Execute(Context context)
	{
		context.Item = _process.Invoke(EvaluateArguments(context), context.Item);
	}
}

public class SignalProcess : RuntimeProcess
{
	public SignalProcess(RuntimeExpression[] arguments) : base(arguments)
	{
	}

	public override void Execute(Context context)
	{
		var args = EvaluateArguments(context);
		throw new NotImplementedException();
	}
}
