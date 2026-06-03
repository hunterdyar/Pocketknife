using System.Globalization;

namespace PocketKnifeCore;

public class DateTimeBuiltins
{
	//filter: before (arg1 otherdatetime)
	//filter: after (arg1 otherdatetime)
	//filter: same-day-of-week, etc.

	[PipelineOperator("to-datetime")]
	public static PKDateTime PipelineOperator(PKString input, PKItem[] args)
	{
		if(DateTime.TryParse(input.Value, CultureInfo.CurrentCulture, out var time))
		{
			return new PKDateTime(time);
		}

		throw new RuntimeException($"Unable to convert '{input}' to a DateTime (using {CultureInfo.CurrentCulture.Name} culture).");
	}

	[PipelineOperator("day-of-year")]
	public static PKNumber DayOfYear(PKDateTime dateTime, PKItem[] args)
	{
		return new PKNumber(dateTime.Value.DayOfYear);
	}

	[PipelineOperator("day")]
	public static PKNumber DayOfMonth(PKDateTime dateTime, PKItem[] args)
	{
		return new PKNumber(dateTime.Value.Day);
	}

	[PipelineOperator("month")]
	public static PKNumber Month(PKDateTime dateTime, PKItem[] args)
	{
		return new PKNumber(dateTime.Value.Month);
	}

	[PipelineOperator("year")]
	public static PKNumber Year(PKDateTime dateTime, PKItem[] args)
	{
		return new PKNumber(dateTime.Value.Year);
	}
	
}