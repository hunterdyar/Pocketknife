using PocketknifeCore.Attributes;

namespace PocketknifeCore;

public class StringMethods
{
	[Pipeline(Name = "append")]
	public static string Append(string first, string second)
	{
		return first + second;
	}

	[Pipeline(Name = "prepend")]
	public static string Prepend(string first, string second)
	{
		return second + first;
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

	[Pipeline(Name = "length")]
	public static int Length(string str)
	{
		return str.Length;
	}
	

	[Filter(Name = "has-whitespace")]
	public static bool HasWhitespace(string str)
	{
		return str.Any(char.IsWhiteSpace);
	}

	[Filter(Name = "is-whitespace")]
	public static bool IsWhitespace(string str)
	{
		return str.All(char.IsWhiteSpace);
	}
	
	[Filter(Name = "is-empty")]
	public static bool IsEmpty(string str)
	{
		return str.Length == 0;
	}
	
	[Filter(Name = "contains")]
	public static bool Contains(string str, string substring)
	{
		return str.Contains(substring);
	}
	
	[PipeGenerator("split")]
	public static List<string> Split(string str, string separator)
	{
		return str.Split(separator).ToList();
	}
	
	[PipeGenerator("chars")]
	public static List<string> Chars(string str)
	{
		return str.ToCharArray().Select(c => c.ToString()).ToList();
	}
}