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

	[PipelineOperator("to-upper")]
	public static PKTableRow RowToUpper(PKTableRow row, PKItem[] args)
	{
		row.Value.BeginEdit();
		for (var i = 0; i < row.Value.ItemArray.Length; i++)
		{
			object? item = row.Value.ItemArray[i];
			if (item is string s)
			{
				row.Value.ItemArray[i] = s.ToUpper();
			}
		}
		row.Value.EndEdit();
		row.Value.AcceptChanges();
		return row;
	}

	[PipelineOperator("to-upper")]
	public static PKTableCol ColToUpper(PKTableCol col, PKItem[] args)
	{
		col.Value.ColumnName = col.Value.ColumnName.ToUpper();
		return col;
	}
	
}