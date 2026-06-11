using PocketKnife.Compiler;
using PocketknifeCore;
using PocketknifeCore.Compiler;
using PocketknifeCore.SimpleEvaluator;

namespace PocketknifeCompiler.Tests;

public class FilterTests
{
	[TestCase("""
	          >"hello, world"
	          |>split ","
	          ~contains "e"
	          :print
	          """, "hello")]
	[TestCase("""
	          >12
	          |>factors
	          ~is-even
	          :print
	          """, "2","4","6","12")]
	public void PipelineGetSimple(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}
}
