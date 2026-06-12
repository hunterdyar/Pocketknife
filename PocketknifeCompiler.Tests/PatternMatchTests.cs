using PocketKnife.Compiler;
using PocketknifeCore;
using PocketknifeCore.Compiler;
using PocketknifeCore.SimpleEvaluator;

namespace PocketknifeCompiler.Tests;

public class PatternMatchTests
{
	[TestCase("""
	          >1 2 3 4
	          ?
	          + ~is-even
	            |prepend "e"
	          + ~is-odd
	            |prepend "o"
	          ^
	          :print
	          """, "o1","e2", "o3", "e4")]
	public void BasicPipelineMatch(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}
	
	[TestCase("""
	          >range 1 5
	          <>
	          |count
	          ?
	          + ~positive
	            |to-string
	            |prepend "count = "
	            |append " (positive)"
	          + ~~
	            |to-string
	            |prepend "count = "
	            |append " (zero-or-negative)"
	          ^
	          :print
	          """, "count = 4 (positive)")]
	public void PipelineMatch(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}

	[TestCase("""
	          >range 0 50
	          <>
	          |count
	          ?
	          + ~positive
	            ~lt 100
	            |to-string
	            |prepend "in range: "
	          + ~~
	            |to-string
	            |prepend "out of range: "
	          ^
	          :print
	          """, "in range: 50")]
	[TestCase("""
	          >range -5 5
	          ?
	          + ~positive
	            ~drop
	          + ~negative
	            |neg
	          ^
	          :print
	          """, "5", "4", "3", "2", "1", "0")]

	[TestCase("""
	          >range -5 5
	          ?
	          + ~positive
	            ~drop
	          + ~~
	            ~is-even
	          ^
	          :print
	          """, "-4", "-2", "0")]
	public void PipelineMatchAnd(string source, params string[] expected)
	{
		Helpers.RunAndAssert(source, expected);
	}
}
