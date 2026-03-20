using System.Data;
using System.Globalization;
using CsvHelper;

namespace PocketKnifeCore;

public class PKTable : PKItem<DataTable>
{
	private PKTable(DataTable dt) : base(dt)
	{
	}


	public static PKTable ParseCSVStringToTable(string raw)
	{
		//https://joshclose.github.io/CsvHelper/
		using (var csv = new CsvReader(new StringReader(raw), CultureInfo.InvariantCulture))
		{
			using (var dr = new CsvDataReader(csv))
			{
				DataTable dt = new DataTable();
				dt.Load(dr);
				return new PKTable(dt);
			}
		}
	}

	public static PKTable ReadCSVStreamToTable(StreamReader stream)
	{
		using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
		{
			using (var dr = new CsvDataReader(csv))
			{
				DataTable dt = new DataTable();
				dt.Load(dr);
				return new PKTable(dt);
			}
		}
	}
}

public class PKTableRow : PKItem<DataRow>
{
	
}
