using PocketknifeCore.Attributes;

namespace PocketknifeCore;

public class ListMethods
{
	// [Pipeline(Name = "count")]
	// public static int Count<T>(List<T> list)
	// {
	// 	return list.Count;
	// }

	//all these overloads?
	[Pipeline(Name = "count")]
	public static int Count(List<int> list)
	{
		return list.Count;
	}
	
	[Pipeline(Name = "count")]
	public static int Count(List<string> list)
	{
		return list.Count;
	}
}