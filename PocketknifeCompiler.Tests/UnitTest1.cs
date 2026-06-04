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
		var sources = source.Trim().Split(Environment.NewLine).Where(x => x.Trim() != "").ToArray();
		Assert.That(gots.Length, Is.EqualTo(sources.Length));
		for (int i = 0; i < sources.Length; i++)
		{
			Assert.That(gots[i].Trim(), Is.EqualTo(sources[i].Trim()));
		}
	}
}