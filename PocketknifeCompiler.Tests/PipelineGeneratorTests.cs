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
	          """, "H ","e  ","l  ","l  ","o  ")]
	public void PipelineGetSimple(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}
	

	
}
