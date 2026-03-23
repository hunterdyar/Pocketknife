using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public abstract class RuntimeExpression
{
	public abstract PKItem GetValue(Context? calledContext);
}