namespace PocketKnifeCore;

public static class CSVBuiltins
{
	[Loader("csv")]
	public static PKTable LoadTextToString(FileInfo file)
	{
		var stream = file.OpenText();
		var table = PKTable.ReadCSVStreamToTable(stream);
		stream.Close();
		return table;
	}
}