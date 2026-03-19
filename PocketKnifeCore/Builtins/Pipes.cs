namespace PocketKnifeCore;

public class BuiltinPipes
{
	public static Dictionary<string, Func<PKItem[], Dictionary<string, PKItem>, Func<PKItem, PKItem>>?> PipelineProviders =
		new Dictionary<string, Func<PKItem[], Dictionary<string, PKItem>, Func<PKItem, PKItem>>?>()
		{
			{
				"filename", (a, o) => {
					//todo: make my own asserts
					BuiltinHelpers.CheckArgumentCount(a, 0);
					string validType = typeof(FileInfo).ToString().ToLowerInvariant();
					
					return new(item =>
					{
						if (item.Type == validType)
						{
							if (item is PKFileInfo pkfi)
							{
								return new PKString(pkfi.Value.Name);
							}
						}

						throw new Exception($"Cannot call '|filename' on {item.Type} item.");
					}); 
				}
			},
			{
				"load", (a, o) =>
				{
					//todo: make my own asserts
					BuiltinHelpers.CheckArgumentCount(a, 1);
					var loadType = a[0].ToString();

					if (loadType == "text")
					{
						return new(item =>
						{
							if (item is PKFileInfo pkfi)
							{
								if (!pkfi.Value.Exists)
								{
									throw new Exception($"File {pkfi.Value} does not exist. Can't |load");
								}
								var stream = pkfi.Value.OpenText();
								var content = stream.ReadToEnd();
								stream.Close();
								return new PKString(content);
							}
							throw new Exception($"Cannot call '|load' on {item.Type} item.");
						});
					}else if (loadType == "csv")
					{
						throw new NotImplementedException("csv loading (table types) not yet implemented.");
					}else if (loadType == "xlsx")
					{
						throw new NotImplementedException("Spreadsheets not yet supported");
					}else if (loadType == "json")
					{
						throw new NotImplementedException("json not yet supported");
					}

					throw new Exception($"bad argument. Unknown type of data to |load {loadType}");
				}
			}
		};
}