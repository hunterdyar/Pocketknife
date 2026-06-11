using PocketKnife.Compiler;
using PocketknifeCore;
using PocketknifeCore.Compiler;
using PocketknifeCore.SimpleEvaluator;

namespace PocketknifeCompiler.Tests;

public class PipelineGeneratorTests
{
	[TestCase("""
	          >"hello"
	          |>chars
	          |append " "
	          :print
	          """, "h ","e  ","l  ","l  ","o  ")]
	[TestCase("""
	          >"hello, world"
	          |>split ","
	          <>
	          |count
	          :print
	          """, "2")]
	public void PipelineGetSimple(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}
}
