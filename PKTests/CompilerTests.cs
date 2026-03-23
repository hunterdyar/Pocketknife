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
		                	.fn
		                	|filename no-ext
		                	|append ".csv" //string function
		                	^
		                |load xlsx
		                |save csv "./out1/" @fn
		                
		                //pipe-input turns one input into many, until an output (<) or a pipeout (|<) which becomes the new pipeline.
		                .
		                >table //rows
		                |>cols //foreach row, we'll loop over each column
		                |toUpper
		                |< //the changes to the columns, applied.
		                |<
		                ^
		                """;

		var p = new Parser();
		p.Parse(source);
		var interpreter = new Compiler();
		interpreter.CompileScript(p.Program);
		Debug.WriteLine("done");
	}

	[Test]
	public void ParseBlenderExampleTest()
	{
		string source = """
		                >dir ./files
		                |load blender
		                |render-frame 20 (cycles-device=CUDA threads=12)
		                """;

		var p = new Parser();
		p.Parse(source);
		var interpreter = new Compiler();
		interpreter.CompileScript(p.Program);
		Debug.WriteLine("done");
	}

	
}