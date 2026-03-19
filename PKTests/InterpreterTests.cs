using System.Diagnostics;
using PocketKnifeCore.Engine;
using PocketKnifeCore.Parser;

namespace PKTests;


public class InterpreterTests
{
	[Test]
	public void GenericTest()
	{
		string source = """
		                >dir "../../../testdata/input" (order=command)
		                .
		                ~ext csv
		                |copy-to "./out1/" //returns the new path
		                ^
		                .
		                ~ext xlsx
		                	.
		                	|filename no-ext
		                	|append ".csv" //string function
		                	|=@filename
		                	^
		                |load xlsx
		                |save csv "./out1/" @filename
		                ^
		                """;

		var p = new Parser();
		p.Parse(source);
		var interpreter = new Interpreter();
		interpreter.RunScript(p.Program);
		Debug.WriteLine("done");
	}
}