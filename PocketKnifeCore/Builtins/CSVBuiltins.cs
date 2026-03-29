using System.Data;

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

	[Saver("csv", ".csv", typeof(PKTable))]
	public static void SaveTableToCSV(FileStream writer, PKTable table)
	{
		PKTable.WriteTableToCSVStream(writer, table);
	}

	[PipeInputOperator("rows", typeof(PKTable))]
	public static IEnumerable<PKTableRow> Rows(PKTable table, PKItem[] args)
	{
		foreach (DataRow row in table.Value.Rows)
		{
			yield return new PKTableRow(row);
		}
	}

	[PipeInputOperator("cols", typeof(PKTable))]
	public static IEnumerable<PKTableCol> Cols(PKTable table, PKItem[] args)
	{
		foreach (DataColumn col in table.Value.Columns)
		{
			yield return new PKTableCol(col);
		}
	}
}