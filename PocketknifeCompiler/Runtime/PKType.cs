namespace PocketknifeCore;

public static class PKType
{
	public static readonly Type None = typeof(void);
	public static readonly Type Any = typeof(object);

	public static bool IsStream(this Type type) => type.GetLiftLevel() > 0;
	
	public static int GetLiftLevel(this Type type)
	{
		int levels = 0;
		Type? current = type;
		while (current != null && current.IsGenericType && current.GetGenericTypeDefinition() == typeof(List<>))
		{
			levels++;
			current = current.GetGenericArguments()[0];
		}
		return levels;
	}

	public static Type Lift(this Type type, int levels = 1)
	{
		Type res = type;
		for (int i = 0; i < levels; i++)
		{
			res = typeof(List<>).MakeGenericType(res);
		}
		return res;
	}

	public static Type Lower(this Type type)
	{
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
		{
			return type.GetGenericArguments()[0];
		}
		return None;
	}

	public static bool IsNone(Type type) => type == typeof(void);
	public static Type GetPKType(Type type) => type;
}

public static class PKTypeHelper
{
	public static Type GetPKType(this object obj)
	{
		return obj.GetType();
	}
}
