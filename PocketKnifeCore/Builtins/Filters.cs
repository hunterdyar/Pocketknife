namespace PocketKnifeCore;

public class BuiltinFilters
{
	public static Dictionary<string, Func<PKItem[], Dictionary<string, PKItem>?, Func<PKItem, bool>>> FilterProviders =
		new Dictionary<string, Func<PKItem[], Dictionary<string, PKItem>?, Func<PKItem, bool>>>()
		{
			{
				"ext", (a, o) =>
				{
					//todo: make my own asserts
					BuiltinHelpers.CheckArgumentCount(a, 1);
					var extension = BuiltinHelpers.GetArgument<PKString>(a[0], "file extension");
					if (!extension.Value.StartsWith('.'))
					{
						extension.Value = "." + extension.Value;
					}

					extension.Value = extension.Value.ToLowerInvariant();
					string validType = typeof(FileInfo).ToString().ToLowerInvariant();
					//todo: (case-sensitive) is a thing.
					return new(item =>
					{
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
}