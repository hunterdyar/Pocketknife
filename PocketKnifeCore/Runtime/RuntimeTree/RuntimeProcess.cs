using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

//root class for step that receives items (e.g. pipeline, filter, etc)
public abstract class RuntimeProcess
{
	public abstract void Execute(Context context);
}