using System.Diagnostics;
using PocketKnife.Compiler;

namespace PocketknifeCompiler.Tests;

public class Tests
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void Test1()
	{
		Assert.Pass();
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
	              >echo "a"
	              .@x1
	                |to-upper
	              &
	              >echo "b"
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
		EachLineEqualIgnoringIndents(got, source);
	}

	private void EachLineEqualIgnoringIndents(string got, string source)
	{
		var gots = got.Trim().Split(Environment.NewLine).Where(x=>x.Trim()!="").ToArray();
		var sources = source.Trim().Split(Environment.NewLine).Select(x=>x.Trim()).Where(x => x != "" && !x.StartsWith("//")).ToArray();
		for (int i = 0; i < sources.Length; i++){
			if (sources[i].Trim().StartsWith("//"))
			{
				continue;
			}
			Assert.That(gots[i].Trim(), Is.EqualTo(sources[i].Trim()));
		}
	}
}