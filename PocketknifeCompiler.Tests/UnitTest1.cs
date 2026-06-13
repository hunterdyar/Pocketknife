using System.Diagnostics;
using PocketKnife.Compiler;
using PocketknifeCore;
using PocketknifeCore.Compiler;
using PocketknifeCore.SimpleEvaluator;

namespace PocketknifeCompiler.Tests;

public class Tests
{
	[SetUp]
	public void Setup()
	{
	}

	[TestCase("""
	          >"Hello"
	          .@x
	            |to-upper
	          ^
	          .@y
	            .@x
	              |to-lower
	            ^
	            |prepend @x
	            |prepend @^x
	          <
	          :print
	          """)]
	[TestCase("""
	          >range 1 5
	          <>
	          |distinct
	          |count
	          |to-string
	          |prepend "unique elements: "
	          :print
	          """)]
	[TestCase("""
	          //comment at beginning of file
	          >range 1 500
	          <>
	          |count
	          ?
	          + ~lt 10
	            |to-string
	            |prepend "bucket = small "
	          + ~lt 100
	            |to-string
	            |prepend "bucket = medium "
	          + ~~
	            |to-string
	            |prepend "bucket = large "
	          <
	          :print
	          //comment at end of file
	          """)]
	[TestCase("""
	          >[]
	          .@sizes
	            >[]
	            .@group
	              >"a"
	              .@x1
	              |to-upper
	              &
	              >"b"
	              .@x2
	              |to-upper
	              &
	            ^
	            >@group
	            |count
	            .@sz
	              |add 0
	            &
	          ^
	          >@sizes
	          |sum
	          |to-string
	          :print
	          """)]
	[TestCase("""
	          >range -3 3
	          <>
	          |sort-by [
	            |abs
	          ]
	          ><
	          |to-string
	          :print
	          """)]
	[TestCase("""
	          // 1. Natural string sort (no bracket) — list<string>, ordinal ascending.
	          >"banana,apple,cherry"
	          |split ","
	          <>
	          |sort
	          ><
	          :print
	          
	          // 2. Sort strings by a computed int key (length) — the key's kind (int) differs
	          //    from the element's kind (string). Stable on equal length (banana/cherry).
	          >"banana,fig,apple,cherry"
	          |split ","
	          <>
	          |sort [
	            |length
	          ]
	          ><
	          :print
	          
	          // 3. |sort-desc over strings — natural order, descending.
	          >"banana,apple,cherry"
	          |split ","
	          <>
	          |sort-desc
	          ><
	          :print
	          
	          // 4. Polymorphic |max / |min over strings — return the ordinal extreme.
	          >"banana,apple,cherry"
	          |split ","
	          <>
	          |max
	          |prepend "max: "
	          :print
	          
	          >"banana,apple,cherry"
	          |split ","
	          <>
	          |min
	          |prepend "min: "
	          :print
	          """)]
	public void ParseTest(string source)
	{
		var p = new Parser();
		p.Parse(source);
		var got = p.Program.ToString();
		Helpers.EachLineEqualIgnoringIndents(got, source);
	}

	// Scaffolding entry point: parse a sample program, then hand the AST to the
	// Compiler with the default op catalog. Not asserting structure yet — this
	// is just a stable place to step through Compile() while it's being written.
	[TestCase("""
	          >"Hello"
	          |to-upper
	          :print
	          """)]
	[TestCase("""
	          >"Hello"
	          |to-upper
	          |to-lower
	          :print
	          """)]
	public void CompileTest(string source)
	{
		var p = new Parser();
		p.Parse(source);

		var catalog = OpCatalog.GetDefaultOpCatalog();
		var compiler = new Compiler(catalog);

		var compiled = compiler.StartCompile(p.Program);

		Assert.That(compiled, Is.Not.Null);
		TestContext.WriteLine(compiled.ToString());
	}


	// is just a stable place to step through Compile() while it's being written.
	[TestCase("""
	          >"Hello"
	          |to-upper
	          :print
	          """, "HELLO")]
	[TestCase("""
	          >"Hello" "Hi" "ahOYYYY!" "yo"
	          |to-upper
	          |to-lower
	          :print
	          """, "hello","hi","ahoyyyy!","yo")]
	[TestCase("""
	          >"Hello" "Hi" "ahOYYYY!" "yo"
	          |to-upper
	          <>
	          |count
	          :print
	          """,$"4")]
	[TestCase("""
	          >"Hello" "Hi" "ahOYYYY!" "yo"
	          |to-lower
	          .
	          <>
	          |count
	          :print
	          ^
	          :print
	          ""","4","hello","hi","ahoyyyy!","yo")]

	[TestCase("""
	          >"Hello" "Hi" "ahOYYYY!" "yo"
	          |to-lower
	          <>
	          .
	          |count
	          :print
	          ^
	          ><
	          :print
	          """, "4", "hello", "hi", "ahoyyyy!", "yo")]
	[TestCase("""
	          >0 1 2 3 4
	          .
	          .
	          .
	          :print
	          ^
	          ^
	          ^
	          """, "0", "1", "2", "3", "4")]
	[TestCase("""
	          >0 1 2 3 4
	          .
	          .
	          .
	          :print
	          
	          """, "0", "1", "2", "3", "4")]
	[TestCase("""
	          >1
	          |add 4
	          &
	          
	          
	          >1
	          |add 4
	          &
	          
	          >range 0 4
	          |mul 2
	          &
	          
	          :print
	          """, "5","5","0","2","4","6")]

	[TestCase("""
	          >range 0 5
	          >range 5 10
	          <
	          :print
	          """, "5", "6", "7", "8", "9")]
	
	[TestCase("""
	          >"a" "b"
	          >1 2
	          :print
	          ^
	          """, "1", "2", "1", "2")]

	[TestCase("""
	          >range 0 5
	          .
	          >range 5 10
	          <
	          ^ 
	          :print
	          """, "0", "1", "2", "3", "4")]
	
	[TestCase("""
	          >range 10 15
	          :echo @Index
	          """, "0", "1", "2", "3", "4")]
	public void SimpleEvalTest(string source, params string[] expectedOutput)
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
		//i just can't be bothered to deal with environment.newlines.
		var expectedOutputString = string.Join(Environment.NewLine, expectedOutput);
		Helpers.EachLineEqualIgnoringIndents(sw.ToString(), expectedOutputString);
	}

	[TestCase("""
	          >range 0 6
	          |abs
	          :print
	          """, "0", "1", "2", "3", "4", "5")]
	[TestCase("""
	          >range -5 6
	          |abs
	          :print
	          """, "5","4","3","2","1","0", "1", "2", "3", "4", "5")]
	[TestCase("""
	          >1 10 100
	          |mul 2
	          |div 5
	          |ceil
	          :print
	          """, "1","4","40")]
	public void ParamsSimpleEvalTest(string source, params string[] expectedOutput)
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
	}

}