namespace PocketknifeCompiler.Tests;

public static class Helpers
{
	public static void EachLineEqualIgnoringIndents(string got, string source)
	{
		var gots = got.Trim().Split(Environment.NewLine).Where(x => x.Trim() != "").ToArray();
		var sources = source.Trim().Split(Environment.NewLine).Select(x => x.Trim()).Where(x => x != "" && !x.StartsWith("//")).ToArray();
		for (int i = 0; i < sources.Length; i++)
		{
			if (sources[i].Trim().StartsWith("//"))
			{
				continue;
			}

			Assert.That(gots[i].Trim(), Is.EqualTo(sources[i].Trim()));
		}
	}
}