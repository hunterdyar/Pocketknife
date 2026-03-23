using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

//root class for step that receives items (e.g. pipeline, filter, etc)
public abstract class RuntimeProcess
{
	protected RuntimeExpression[] _arguments;
	
	protected RuntimeProcess(RuntimeExpression[] arguments)
	{
		_arguments = arguments;
	}

	protected PKItem[] EvaluateArguments(Context context)
	{
		return _arguments.Select(x => x.GetValue(context)).ToArray();
	}
	public abstract void Execute(Context context);
}