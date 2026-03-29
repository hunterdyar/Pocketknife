using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class NativePipes
{
	// Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, PKItem>>?
	public delegate Func<PKItem[], PKItem, PKItem> NativePipe(Dictionary<string, PKItem> options, out Type retType);
	
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
								return c.Item;
							}
							else
							{
								throw new Exception($"bad argument. Unknown type of data to |save {saveType}");
							}
						});
					}
				}
			};

}