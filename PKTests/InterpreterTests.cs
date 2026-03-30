using System.Diagnostics;
using PocketKnifeCore.Engine;
using PocketKnifeCore.Parser;

namespace PKTests;

public class InterpreterTests
{
	[Test]
	public void GenericCSVTestOne()
	{
		string source = """
		                >dir "../../../testdata/input"
		                .
		                ~ext csv
		                .fn
		                |filename no-ext
		                |append ".csv"
		                ^
		                |load csv
		                |save csv "./out1/" @fn

		                //pipe-input turns one input into many, until an output (<).

		                |>cols //foreach row, we'll loop over each column
		                |to-upper
		                < //the changes to the columns, applied.

		                """;

		var p = new Parser();
		p.Parse(source);
		var compiler = new Compiler();
		compiler.CompileScript(p.Program);
		var i = new Interpreter();
		i.Execute(compiler.Script, "");
	}

	[Test]
	public void ExtractionExampleTest()
	{
		string source = """
		                >dir "../../../testdata/input"
		                ~ext zip
		                |extract
		                <
		                """;

		var p = new Parser();
		p.Parse(source);
		var compiler = new Compiler();
		compiler.CompileScript(p.Program);
		var i = new Interpreter();
		i.Execute(compiler.Script, "");

		var expectedOutput = new DirectoryInfo(Directory.GetCurrentDirectory() + "/../../../testdata/input/testzip/");
		Assert.That(expectedOutput.Exists);
		if (expectedOutput.Exists)
		{
			expectedOutput.Delete(true);
		}
	}
}
