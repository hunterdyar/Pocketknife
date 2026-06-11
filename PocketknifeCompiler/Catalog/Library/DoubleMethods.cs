using PocketknifeCore.Attributes;

namespace PocketknifeCore;

public static class DoubleMethods
{
	[Pipeline(Name = "abs")]
	public static double Abs(double i)
	{
		return i < 0 ? -i : i;
		// return int.Abs(i);
	}
	[Pipeline(Name = "neg")]
	public static double Neg(double i)
	{
		return -i;
	}
	[Pipeline(Name = "round")]
	public static double Round(double i)
	{
		return Math.Round(i);
	}
	[Pipeline(Name = "floor")]
	public static double Floor(double i)
	{
		return Math.Floor(i);
	}
	[Pipeline(Name = "floor-to-int")]
	public static int FloorToInt(double i)
	{
		return (int)Math.Floor(i);
	}
	[Pipeline(Name = "ceil")]
	public static double Ceil(double i)
	{
		return Math.Ceiling(i);
	}
	[Pipeline(Name = "ceil-to-int")]
	public static int CeilToInt(double i)
	{
		return (int)Math.Ceiling(i);
	}

	[Pipeline(Name = "add")]
	public static double Add(double i, double b)
	{
		return i+b;
	}
	
	[Pipeline(Name = "sub")]
	public static double Subtract(double i, double b)
	{
		return i-b;
	}

	[Pipeline(Name = "mul")]
	public static double Mul(double i, double b)
	{
		return i * b;
	}

	[Pipeline(Name = "div")]
	public static double Div(double i, double b)
	{
		return i / b;
	}

	[Pipeline(Name = "sin")]
	public static double Sin(double x)
	{
		return Math.Sin(x);
	}

	[Pipeline(Name = "cos")]
	public static double Cos(double x)
	{
		return Math.Cos(x);
	}

	[Pipeline(Name = "clamp")]
	public static double Clamp(double x, double a, double b)
	{
		return Math.Clamp(x, a,b);
	}

	[Pipeline(Name = "clamp01")]
	public static double Clamp01(double x)
	{
		return Math.Clamp(x, 0, 1);
	}
	
	[Pipeline(Name = "divided-by")]
	public static double DividedBy(double i, double b)
	{
		return b/i;
	}

	[Pipeline(Name = "mod")]
	public static double Mod(double i, double b)
	{
		return i % b;
	}
	
	[Pipeline(Name = "pow")]
	public static double Pow(double i, double b)
	{
		return Math.Pow(i, b);
	}

	[Casting(false)]
	public static double ToInt(double i)
	{
		return (int)i;
	}
	
}
