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
	          |trim
	          |length
	          :print
	          <>
	          |count
	          :print
	          """, "5","5","2")]
	[TestCase("""
	          >12
	          .
	          |>factors
	          &
	          :print
	          
	          """, "12","1", "2", "3","4", "6","12")]
	public void PipelineGetSimple(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}
}
