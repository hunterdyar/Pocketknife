using PocketknifeCore.Attributes;

namespace PocketknifeCore;

public static class Standard
{
	[Signal(Name = "print")]
	public static void Print(string s) => Console.WriteLine(s);
	
	[Signal(Name = "print")]
	public static void Print(object o) => Console.WriteLine(o.ToString());
	
	[Signal(Name = "print")]
	public static void Print(int i) => Console.WriteLine(i.ToString());

	[Signal(Name = "echo")]
	public static void Echo(object i, object o) => Console.WriteLine(o.ToString());
	
	[Filter(Name = "is")]
	public static bool IsValue(object value, object compare)
	{
		return value.Equals(compare);
	}

	[Filter(Name = "drop")]
	public static bool Drop(object _)
	{
		return false;	
	}

	//basically a NOP filter.
	[Filter(Name = "keep")]
	public static bool Keep(object _)
	{
		return true;
	}
}