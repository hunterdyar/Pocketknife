using PocketknifeCore.Attributes;

namespace PocketknifeCore;

public static class Standard
{
	[Signal(Name = "print")]
	public static void Print(string s) => Console.WriteLine(s);
}