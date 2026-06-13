using PocketKnife.Compiler;
using PocketknifeCore;
using PocketknifeCore.Compiler;
using PocketknifeCore.SimpleEvaluator;

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

	public static void RunAndAssert(string source, string[] expectedOutput)
	{
		using var sw = new StringWriter();
		Console.SetOut(sw);

		var p = new Parser();
		p.Parse(source);

		var catalog = OpCatalog.GetDefaultOpCatalog();
		var compiler = new Compiler(catalog);

		var compiled = compiler.StartCompile(p.Program);

		var context = new Context();
		SimpleEvaluator.EvaluateAll(compiled, context);

		var expectedOutputString = string.Join(Environment.NewLine, expectedOutput);
		Helpers.EachLineEqualIgnoringIndents(sw.ToString(), expectedOutputString);
		TestContext.Out.WriteLine($"Final Timeline Length: {context.TimelineLength}");
		TestContext.Out.WriteLine("Max Timeline Length: " + context.MaxTimelineLength);
		TestContext.Out.WriteLine("Max Scope Depth: " + context.MaxScopeDepth);
	}
}