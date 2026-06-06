using PocketknifeCore.Attributes;

namespace PocketknifeCore;

public static class Standard
{
	[Signal(Name = "print")]
	public static void Print(string s) => Console.WriteLine(s);
	
	[Signal(Name = "print")]
	public static void Print(PKValue v) => Print(v.ToString());
	
	[Signal(Name = "print")]
	public static void Print(int i) => Print(i.ToString());
	
	[Signal(Name = "print")]
	public static void Print(List<PKValue> l) => Print(string.Join(", ", l.Select(v => v.ToString())));
}