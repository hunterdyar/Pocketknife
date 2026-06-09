namespace PocketknifeCore;

public readonly struct PKType : IEquatable<PKType>
{
	public readonly bool IsStream => LiftLevel>0;//isEnumerable
	public static readonly PKType None = new PKType(typeof(void));
	public static readonly PKType Any = new PKType(typeof(object));

	public readonly byte LiftLevel = 0;
	public readonly Type Type;
	public PKType(Type type, byte liftLevel = 0)
	{
		Type = type;
		LiftLevel = liftLevel;
	}

	public static bool IsNone(PKType type) => type.Type == typeof(void);
	public bool IsNone() => Type == typeof(void);
	
	public PKType Lift()
	{
		return new PKType(typeof(List<>).MakeGenericType(Type), (byte)(LiftLevel + 1));
	}

	public PKType Lift(int levels)
	{
		PKType res = this;
		for (int i = 0; i < levels; i++)
		{
			res = res.Lift();
		}
		return res;
	}

	public PKType Lower()
	{
		if (LiftLevel == 0) return None;
		if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(List<>))
		{
			return new PKType(Type.GetGenericArguments()[0], (byte)(LiftLevel - 1));
		}
		return None;
	}

	//todo: rewrite this to use generics/reflection?
	public PKType Lifted() => Lift();
	public PKType Lowered() => Lower();

	override public string ToString() => IsStream ? $"<{Type.ToString()}>" : Type.ToString();
	
	public static PKType GetPKType(Type type)
	{
		if (type == typeof(void)) return None;

		byte levels = 0;
		Type current = type;
		while (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(List<>))
		{
			levels++;
			current = current.GetGenericArguments()[0];
		}
		return new PKType(type, levels);
	}
	
	#region IDE Generated Equality Members
	
	public bool Equals(PKType other)
	{
		return Type == other.Type && LiftLevel == other.LiftLevel;
	}

	public override bool Equals(object? obj)
	{
		return obj is PKType other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Type, LiftLevel);
	}

	public static bool operator ==(PKType left, PKType right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PKType left, PKType right)
	{
		return !left.Equals(right);
	}

	#endregion

}

public static class PKTypeHelper
{
	public static PKType GetPKType(this object obj)
	{
		return new PKType(obj.GetType());
	}
}