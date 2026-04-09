using System.Diagnostics;

namespace PocketKnifeCore;

public class BuiltinFilters
{
	public static Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, bool>>?> FilterProviders =
		new Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, bool>>?>();

	public static void CheckPKInputType(PKItem item, Type type)
	{
		Debug.Assert(item.Type.ToLowerInvariant() == type.ToString().ToLowerInvariant());
	}
}