using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class NativePipes
{
	public static Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, PKItem>>?> PipelineProviders = new Dictionary<string, Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, PKItem>>?>()
		{
			{
				"filename", (o) => {
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
									$"unknown filename argument {args[0].ToString()}. valid arguments: '' (none), 'no-ext'");
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

						if (PluginEnvironment.AllLoaders.TryGetValue(loadType, out var func))
						{
							if(item is PKFileInfo fi)
							{
								var loaded = func.Invoke(fi.Value);
								return loaded;
							}
						}

						throw new Exception($"bad argument. Unknown type of data to |load {loadType}");
					});
				}
			},
			
		};

	public static Dictionary<string, Func<Dictionary<string, PKItem>, Func<Context, PKItem[], PKItem>>?> OnContextPipelineProviders =
			new Dictionary<string, Func<Dictionary<string, PKItem>, Func<Context, PKItem[], PKItem>>?>()
			{
				{
				"save", (o) =>
					{
						//get 'overwrite' and 'use extension'
						return new((c,a) =>
						{
							var saveType = a[0].ToString();
							if (PluginEnvironment.AllSavers.TryGetValue(saveType, out var saver))
							{
								saver.Execute(c, a);
							}

							throw new Exception($"bad argument. Unknown type of data to |save {saveType}");
						});
					}
				}
			};

}