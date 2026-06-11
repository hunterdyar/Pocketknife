using PocketknifeCore.Attributes;

namespace PocketknifeCore;

public static class IntMethods
{
	[Generator(Name = "range")]
	public static List<int> Range(int start, int end)
	{
		//surely there's a better way to do this?
		var l = new List<int>(end - start);
		for (int i = start; i < end; i++)
		{
			l.Add(i);
		}
		return l;
	}

	[Generator(Name = "range-step")]
	public static List<int> Range(int start, int end, int step)
	{
		if (step > 0)
		{
			if (end < start)
			{
				throw new Exception("End must be greater than start");
			}
		}else if (step < 0)
		{
			if (start > end)
			{
				throw new Exception("Start must be greater than end for negative step.");
			}
		}
		//surely... surely there's a better way to do this?
		
		var l = new List<int>(int.Abs(end - start));
		for (int i = start; i < end; i+=step)
		{
			l.Add(i);
		}
	
		return l;
	}
	
	[Pipeline(Name = "abs")]
	public static int Abs(int i)
	{
		return i < 0 ? -i : i;
		// return int.Abs(i);
	}

	[Pipeline(Name = "neg")]
	public static int Neg(int i)
	{
		return -i;
	}

	[Casting]
	public static double ToDouble(int i)
	{
		return (double)i;
	}
	
	//there's a way to do math ops on the underlying scalar and have them work for any signed number type, I bet.
}