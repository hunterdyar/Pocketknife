using System.Diagnostics;
using PocketKnifeCore.Engine;
using PocketKnifeCore.Parser;

namespace PKTests;


public class CompilerTests
{
	private PluginEnvironment _environment;
	[SetUp]
	public void Setup()
	{
		_environment = new PluginEnvironment();
	}
	
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
		                //|load xlsx
		                |load csv
		                |save csv "./out1/" @fn
		                
		                //pipe-input turns one input into many, until an output (<).
		                
		                |>cols //foreach row, we'll loop over each column
		                |to-upper
		                < //the changes to the columns, applied.
		                
		                """;

		var p = new Parser();
		p.Parse(source);
		var compiler = new Compiler(p, _environment);
	}

	[Test]
	public void ParseBlenderExampleTest()
	{
		string source = """
		                >dir ./files
		                |load blender
		                |render-frame 20 (cycles-device=CUDA, threads=12)
		                """;

		var p = new Parser();
		p.Parse(source);
		var compiler = new Compiler(p, _environment);
	}


	[Test]
	public void ParseExtractionExampleTest()
	{
		string source = """
		                >dir "./"
		                ~ext zip
		                |extract
		                <
		                """;

		var p = new Parser();
		p.Parse(source);
		var compiler = new Compiler(p, _environment);
	}

	
}