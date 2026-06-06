using PocketknifeCore.Attributes;

namespace PocketknifeCore;

public static class Standard
{
	[Signal(Name = "print")]
	public static void Print(string s) => Console.WriteLine(s);
	
	[Signal(Name = "print")]
	public static void Print(PKValue v) => Console.WriteLine(v.ToString());
	
	[Signal(Name = "print")]
	public static void Print(int i) => Console.WriteLine(i.ToString());
	
	[Signal(Name = "print")]
	public static void Print(List<PKValue> l) => Console.WriteLine(string.Join(", ", l.Select(v => v.ToString())));
}