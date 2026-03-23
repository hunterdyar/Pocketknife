using System.Diagnostics;

namespace PocketKnifeCore;

public class BuiltinFilters
{
	public static Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, bool>>?> FilterProviders =
		new Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, bool>>?>()
		{
			{
				"ext", (o) =>
				{
					
					string validType = typeof(FileInfo).ToString().ToLowerInvariant();
					//todo: (case-sensitive) is a thing.
					return new((args, item) =>
					{
						BuiltinHelpers.CheckArgumentCount(args, 1);
						//todo: make my own asserts
						var extension = BuiltinHelpers.GetArgument<PKString>(args[0], "file extension");
						if (!extension.Value.StartsWith('.'))
						{
							extension.Value = "." + extension.Value;
						}
						extension.Value = extension.Value.ToLowerInvariant();
						
						if (item.Type == validType)
						{
							if (item is PKFileInfo pkfi)
							{
								return pkfi.Value.Extension.ToLowerInvariant() == extension.Value;
							}
						}
						return false;
					});
				}
			}
		};

	public static void CheckPKInputType(PKItem item, Type type)
	{
		Debug.Assert(item.Type.ToLowerInvariant() == type.ToString().ToLowerInvariant());
	}
}