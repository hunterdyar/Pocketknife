using System.Reflection;

namespace PocketknifeCore;

public class OperatorDescription
{
	public PKKind InType = PKKind.None;
	public PKKind OutType = PKKind.None;
	public required MethodInfo Method;
	//isList, isGenerator, etc.
	public OperatorDescription()
	{
	}
}