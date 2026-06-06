using System.Reflection;

namespace PocketknifeCore;

public class OperatorDescription
{
	public PKType InType = PKType.None;
	public PKType OutType = PKType.None;
	public required MethodInfo Method;
	//isList, isGenerator, etc.
	public OperatorDescription()
	{
	}
}