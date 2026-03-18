using System.Diagnostics;
using PocketKnifeCore.Parser;

namespace PKTests;

public class ParserTest1
{
   [Test]
    public void GenericTest()
    {
        string source = """
                        >dir "./input"
                        .
                        ~ext csv
                        |copy-to "./out1/" //returns the new path
                        ^
                        .
                        ~ext xlsx
                        	.
                        	|filename no-ext
                        	|append ".csv" //string function
                        	|< @filename //
                        	^
                        |load xlsx
                        |save csv "./out1/" @filename
                        ^
                        """;
        
        var p = new Parser();
	    p.Parse(source);
        Debug.WriteLine("Parsed...");
    }
}