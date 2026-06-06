using System.Collections;

namespace PocketknifeCore;

public readonly struct PKValue
{
	//need a "isListOf" property, and it's reference to List<Kind>... (for after <>, etc)
	//but that would mess up all the asstrings and stuff, so maybe a child of PKValue?
	public readonly PKType Type => _type;
	private readonly PKType _type;
	private readonly long _scalar;
	private readonly object? _ref;
	
	public PKValue(PKType type, long scalar) : this()
	{
		_type = type;
		_scalar = scalar;
	}

	public PKValue(PKType type, object? @ref) : this()
	{
		_type = type;
		_ref = @ref;
	}

	public static readonly PKValue None = default;

	public string AsString() => (string)_ref;
	public T RefAs<T>() => (T)_ref ?? throw new InvalidOperationException();
	// public T ScalarAs<T>() => (T)_scalar ?? throw new InvalidOperationException();
	public int AsInt => (int)_scalar;
	public long AsLong => (long)_scalar;
	public double AsDouble => (double)_scalar;
	public bool AsBool => _scalar != 0;

	public static PKValue FromString(string s)
	{
		return new PKValue(new PKType(PKKind.String), s);
	}

	public static PKValue FromInt(int i)
	{
		return new PKValue(new PKType(PKKind.Int), i);
	}
	
	public static PKValue FromLong(long l)
	{
		return new PKValue(new PKType(PKKind.Long), l);
	}
	
	public static PKValue FromBool(bool b)
	{
		return new PKValue(new PKType(PKKind.Bool), b ? 1 : 0);
	}
	public static PKValue FromDouble(double d)
	{
		return new PKValue(new PKType(PKKind.Double), (long)d);
	}

	public static PKType GetPKType(Type type)
	{
		//todo: this will only work for system types
		if (type == typeof(void))
		{
			return PKType.None;
		}

		switch (System.Type.GetTypeCode(type))
		{
			case TypeCode.Empty:
				return PKType.None;
			case TypeCode.String:
				return new PKType(PKKind.String);
			case TypeCode.Int32:
				return new PKType(PKKind.Int);
			case TypeCode.Int64:
				return new PKType(PKKind.Long);
			case TypeCode.Boolean: 
				return new PKType(PKKind.Bool);
			case TypeCode.Double:
				return new PKType(PKKind.Double);
		}
		
		if (type.GenericTypeArguments.Length > 0)
		{
			var oftd = type.GetGenericTypeDefinition();
			var oft = GetPKType(type.GenericTypeArguments[0]);
			if (oft != PKType.None)
			{
				//is this IEnumerable?
				if (oftd == typeof(IEnumerable<>))
				{
					return new PKType(oft.Kind, true);
				}else if (oftd == typeof(List<>))
				{
					return new PKType(oft.Kind, true);
				}
			}
		}
		
		return PKType.None;
	}

	override public string ToString()
	{
		return "pktype("+_ref?.ToString() ?? _scalar.ToString()+")";
	}
}
