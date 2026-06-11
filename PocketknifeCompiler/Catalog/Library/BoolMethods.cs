using PocketknifeCore.Attributes;

namespace PocketknifeCore;

public class BoolMethods
{
	[Filter(Name = "true")]
	public static bool IsTrue(bool b)
	{
		return b;
	}

	[Filter(Name = "false")]
	public static bool IsFalse(bool b)
	{
		return !b;
	}
}