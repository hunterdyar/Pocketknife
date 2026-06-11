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
	
	[PipeGenerator("count-to")]
	public static List<int> CountTo(int i)
	{
		if (i < 0)
		{
			return new List<int>();
		}
		
		var l = new List<int>(i);
		for (int j = 0; j < i; j++)
		{
			l.Add(j);
		}
		return l;
	}

	[PipeGenerator("factors")]
	public static List<int> Factors(int num)
	{
		List<int> factors = new List<int>();

		// Skip two if the number is odd
		int incrementer = num % 2 == 0 ? 1 : 2;

		for (int i = 1; i <= Math.Sqrt(num); i += incrementer)
		{
			if (num % i == 0)
			{
				factors.Add(i);
				if (i != num / i)
				{
					factors.Add(num / i);
				}
			}
		}

		// Sort the list of factors
		factors.Sort();

		return factors;
	}
	//there's a way to do math ops on the underlying scalar and have them work for any signed number type, I bet.
}