using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class NativePipes
{
	public static Dictionary<string, Func<Dictionary<string, PKItem>,PluginEnvironment, Func<Context, PKItem[], PKItem>>?> OnContextPipelineProviders =
		new Dictionary<string, Func<Dictionary<string, PKItem>,PluginEnvironment,  Func<Context, PKItem[], PKItem>>?>()
		{
			{
			"save", (o, pe) =>
				{
					//get 'overwrite' and 'use extension'
					return new((c,a) =>
					{
						var saveType = a[0].ToString();
						if (pe.AllSavers.TryGetValue(saveType, out var saver))
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