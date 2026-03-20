namespace PocketKnifeCore;

public class BuiltinPipes
{
	public static Dictionary<string, Func<PKItem[], Dictionary<string, PKItem>, Func<PKItem, PKItem>>?> PipelineProviders =
		new Dictionary<string, Func<PKItem[], Dictionary<string, PKItem>, Func<PKItem, PKItem>>?>()
		{
			{
				"filename", (a, o) => {
					//todo: make my own asserts
					bool ext = true;
					if (a.Length == 1)
					{
						var setting = a[0].ToString();
						if (setting == "no-ext")
						{
							ext = false;
						}
						else
						{
							throw new Exception(
								$"unknown filename argument {a[0].ToString()}. valid arguments: 'no-ext'");
						}
					}
					else
					{
						BuiltinHelpers.CheckArgumentCount(a, 0);
					}

					string validType = typeof(FileInfo).ToString().ToLowerInvariant();
					
					return new(item =>
					{
						if (item.Type == validType)
						{
							if (item is PKFileInfo pkfi)
							{
								if (ext)
								{
									return new PKString(pkfi.Value.Name);
								}
								else
								{
									//no extension
									return new PKString(pkfi.Value.Name.Replace(pkfi.Value.Extension, ""));
								}
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
			},
			{
				"append", ((args, opts) =>
				{
					if (args.Length == 0)
					{
						throw new Exception("|append needs at least one argument.");
					}
					
					var appends = new string[args.Length];
					for (int i = 0; i < args.Length; i++)
					{
						appends[i] = args[i].ToString();
					}

					return new(item =>
					{
						if (item.TryGetString(out string s))
						{
							for (int i = 0; i < args.Length; i++)
							{
								s += s;
							}

							return new PKString(s);
						}

						throw new Exception($"Cannot call '|filename' on {item.Type} item.");
					}); 
				})
			}
		};
}