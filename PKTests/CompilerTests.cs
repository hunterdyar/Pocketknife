using System.Diagnostics;
using PocketKnifeCore.Engine;
using PocketKnifeCore.Parser;

namespace PKTests;


public class CompilerTests
{
	[Test]
	public void GenericTest()
	{
		string source = """
		                >dir "../../../testdata/input"
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
		var interpreter = new Compiler();
		interpreter.CompileScript(p.Program);
		Debug.WriteLine("done");
	}
}