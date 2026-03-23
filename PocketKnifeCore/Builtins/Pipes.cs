namespace PocketKnifeCore;

public class BuiltinPipes
{
	public static Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, PKItem>>?> PipelineProviders = new Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, PKItem>>?>()
		{
			{
				"filename", (o) => {
					//todo: make my own asserts
					
					string validType = typeof(FileInfo).ToString().ToLowerInvariant();
					
					return new((args, item) =>
					{
						bool ext = true;
						if (args.Length == 1)
						{
							var setting = args[0].ToString();
							if (setting == "no-ext")
							{
								ext = false;
							}
							else
							{
								throw new Exception(
									$"unknown filename argument {args[0].ToString()}. valid arguments: 'no-ext'");
							}
						}
						else
						{
							BuiltinHelpers.CheckArgumentCount(args, 0);
						}

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
				"load", (o) =>
				{
					return new((a, item) =>
					{
						//todo: make my own asserts
						BuiltinHelpers.CheckArgumentCount(a, 1);
						var loadType = a[0].ToString();

						if (loadType == "text")
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

							throw new Exception($"Cannot call '|load text' on {item.Type} item.");
						}
						else if (loadType == "csv")
						{
							if (item is PKFileInfo pkfi)
							{
								if (!pkfi.Value.Exists)
								{
									throw new Exception($"File {pkfi.Value} does not exist. Can't |load");
								}

								var stream = pkfi.Value.OpenText();
								var table = PKTable.ReadCSVStreamToTable(stream);
								stream.Close();
								return table;
							}

							throw new Exception($"Cannot call '|load csv' on {item.Type} item.");
						}
						else if (loadType == "xlsx")
						{
							throw new NotImplementedException("Spreadsheets not yet supported");
						}
						else if (loadType == "json")
						{
							throw new NotImplementedException("json not yet supported");
						}

						throw new Exception($"bad argument. Unknown type of data to |load {loadType}");
					});
				}
			},
			{
				"append", ((opts) =>
				{
					return new((args,item) =>
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