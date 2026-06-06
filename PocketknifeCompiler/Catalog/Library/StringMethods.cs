using PocketknifeCore.Attributes;

namespace PocketknifeCore;

public class StringMethods
{
	[Pipeline(Name = "append")]
	public static string Append(string first, string second)
	{
		return first + second;
	}
	[Pipeline(Name = "to-upper")]
	public static string ToUpper(string str)
	{
		return str.ToUpper();
	}

	[Pipeline(Name = "to-lower")]
	public static string ToLower(string str)
	{
		return str.ToLower();
	}

	[Pipeline(Name = "trim")]
	public static string Trim(string str)
	{
		return str.Trim();
	}
}