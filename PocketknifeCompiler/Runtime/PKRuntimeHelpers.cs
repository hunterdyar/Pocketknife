using System.Collections;

namespace PocketknifeCore;

public static class PKRuntimeHelpers
{
	//Converts an object (expected to be a List of objects) to a List of T.
	public static List<T> ConvertList<T>(object? input)
	{
		if (input == null) return new List<T>();
		if (input is List<T> already) return already;
		
		if (input is IEnumerable enumerable)
		{
			var list = new List<T>();
			foreach (var item in enumerable)
			{
				if (item is T correctlyTyped)
				{
					list.Add(correctlyTyped);
				}
				else
				{
					list.Add((T)Convert.ChangeType(item, typeof(T))!);
				}
			}
			return list;
		}
		
		throw new InvalidOperationException($"Cannot convert {input.GetType()} to List<{typeof(T)}>");
	}

	//Normalizes any collection to a List of objects for the Pocketknife stack.
	public static List<object>? NormalizeList(object? input)
	{
		if (input == null)
		{
			return null;
		}
		
		if (input is List<object> already)
		{
			return already;
		}
		
		if (input is IEnumerable enumerable)
		{
			var list = new List<object>();
			foreach (var item in enumerable)
			{
				list.Add(NormalizeValue(item));
			}
			return list;
		}
		
		return new List<object> { NormalizeValue(input) };
	}

	public static object NormalizeValue(object? input)
	{
		if (input == null) return null!;
		if (input is string or int or bool or double or long) return input;
		
		if (input is IEnumerable enumerable && !(input is string))
		{
			return NormalizeList(input)!;
		}
		
		return input;
	}
}
