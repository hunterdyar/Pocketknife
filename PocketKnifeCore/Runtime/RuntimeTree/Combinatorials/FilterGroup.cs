using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public enum FilterCombinatorialType
{
	All,
	None,
	Any,
	NotAll,
	OnlyOne,
	Not
}

public class FilterGroup : FilterProcess
{
	private FilterCombinatorialType Type;
	private List<FilterProcess> _processes;
	public FilterGroup(List<FilterProcess> processes,FilterCombinatorialType type, RuntimeExpression[] arguments) : base(arguments)
	{
		//we pass null into the base 
		_filter = Process;
		Type = type;
		_processes = processes;
	}

	protected virtual bool Process(PKItem[] args, PKItem item)
	{
		switch (Type)
		{
			case FilterCombinatorialType.All:
				return _processes.All(x => x.Filter.Invoke(args, item));
			case FilterCombinatorialType.Any:
				return _processes.Any(x => x.Filter.Invoke(args, item));
			case FilterCombinatorialType.None:
				return !_processes.Any(x => x.Filter.Invoke(args, item));
			case FilterCombinatorialType.NotAll:
				return !_processes.All(x => x.Filter.Invoke(args, item));
			case FilterCombinatorialType.OnlyOne:
				return _processes.Count(x => x.Filter.Invoke(args, item)) == 1;
			case FilterCombinatorialType.Not:
				if (_processes.Count != 1)
				{
					throw new Exception(
						"Error. Multiple processes applied to 'not'. this is a runtime error that should have been caught by the compiler, so its... probably a parsing error?");
				}

				return !_processes.First().Filter.Invoke(args, item);
			default:
				throw new Exception("Invalid Process TYpe for filter group");
		}
	}

	public override void Execute(Context context)
	{
		var arguments = EvaluateArguments(context);
		context.KeepProcessing = _filter.Invoke(arguments, context.Item);
	}
}