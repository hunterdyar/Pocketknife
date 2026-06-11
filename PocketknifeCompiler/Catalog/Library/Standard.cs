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

	[Filter(Name = "is")]
	public static bool IsValue(object value, object compare)
	{
		return value.Equals(compare);
	}
}