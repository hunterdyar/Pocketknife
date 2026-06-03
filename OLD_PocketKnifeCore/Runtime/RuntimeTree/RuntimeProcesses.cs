using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class FilterProcess : RuntimeProcess
{
	public Func<PKItem[], PKItem, bool> Filter => _filter;
	protected Func<PKItem[], PKItem, bool> _filter;
	public FilterProcess(RuntimeExpression[] arguments, Func<PKItem[], PKItem, bool> filter) : base(arguments)
	{
		_filter = filter;
	}

	protected FilterProcess(RuntimeExpression[] arguments) : base(arguments)
	{
		//_filter still needs to get set by the child constructor!
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

public class OnContextPipelineProcess : RuntimeProcess
{
	private Func<Context, PKItem[], PKItem> _process;

	public OnContextPipelineProcess(RuntimeExpression[] arguments, Func<Context, PKItem[], PKItem> process) : base(arguments)
	{
		_process = process;
	}

	public override void Execute(Context context)
	{
		context.Item = _process.Invoke(context, EvaluateArguments(context));
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
