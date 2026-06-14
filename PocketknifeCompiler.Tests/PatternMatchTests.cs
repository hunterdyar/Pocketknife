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

	//even branch produces string, ~~ branch produces int. after ^ it should be an "any" type, which print should be able to handle.
	[TestCase("""
	          >range -2 3
	          ?
	          + ~is-even
	            |to-string
	            |prepend "even="
	          + ~~
	            |add 0
	          ^
	          :print
	          """, "even=-2", "-1", "even=0", "1", "even=2")]
	public void PipelineMatchHeterogeneousArmTypes(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}
	//todo: nested patternmatches do not parse clearly. you currently need to close the last branch and the ? with ^'s to disambiguate.
	[TestCase("""
	          >range -3 4
	          ?
	          + ~positive
	            ?
	            + ~is-even
	              |to-string
	              |prepend "pos-even="
	              ^
	            + ~~
	              |to-string
	              |prepend "pos-odd="
	              ^//all branches convert to string, so this should be string.
	            ^
	          + ~~
	            |to-string
	            |prepend "nonpos="
	            ^
	          ^
	          //all branches convert to string, so this should be string; not int. 
	          :print
	          """, "nonpos=-3", "nonpos=-2", "nonpos=-1", "nonpos=0", "pos-odd=1", "pos-even=2", "pos-odd=3")]
	[TestCase("""
	          >range 1 5
	          ?
	          + ~is-even //2 4
	            |mul 3
	            ?
	            + ~gt 10
	              |add 1
	              ^
	            + ~~
	              |add 2
	              ^
	            ^
	          + ~~ //1 3
	            |add 1
	          ^
	          :print
	          """, "2", "8", "4", "13")]//stays int throughout this test.
	public void NestedPipelineMatch(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}


	[TestCase("""
	          >range 1 5
	          .@x
	          <
	          ?
	          + ~is-even
	            .@x
	              |neg
	            <
	            |to-string
	            |append " outer="
	            |append @^x
	          + ~~
	            |to-string
	            |append " x="
	            |append @x
	          ^
	          :print
	          """, "1 x=1", "-2 outer=2", "3 x=3", "-4 outer=4")]
	public void PipelineMatchReachOutNamedBinding(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}
	
	[TestCase("""
	          >range 10 14
	          ?
	          + ~is-even
	            |to-string
	            |append "-even-"
	            |append @Index
	          + ~~
	            |to-string
	            |append "-odd-"
	            |append @Index
	          ^
	          :print
	          """, "10-even-0", "11-odd-1", "12-even-2", "13-odd-3")]
	[TestCase("""
	          >range 10 14
	          ?
	          + ~is-even
	            |to-string
	            |append "-even-"
	            |append @Index
	            
	          + ~~
	            |to-string
	            |append "-odd-"
	            |append @Index
	            
	          ^
	          :print
	          """, "10-even-0", "11-odd-1", "12-even-2", "13-odd-3")]
	public void PipelineMatchIndexInsideArms(string source, params string[] expectedOutput)
	{
		Helpers.RunAndAssert(source, expectedOutput);
	}
}
