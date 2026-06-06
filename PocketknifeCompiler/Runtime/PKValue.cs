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
	public int AsInt() => (int)_scalar;
	public long AsLong() => _scalar;
	public double AsDouble() => (double)_scalar;
	public bool AsBool() => _scalar != 0;
	public List<PKValue> AsList()
	{
		if (_type.IsStream)
		{
			return _ref as List<PKValue> ?? throw new InvalidOperationException();
		}
		throw new InvalidOperationException();
	}

	public static PKValue FromList(List<PKValue> items, PKType itemType)
	{
		return new PKValue(itemType.Lifted(), items);
	}

	public static PKValue FromString(string s)
	{
		return new PKValue(PKType.String, s);
	}

	public static PKValue FromInt(int i)
	{
		return new PKValue(PKType.Int, i);
	}
	
	public static PKValue FromLong(long l)
	{
		return new PKValue(PKType.Long, l);
	}
	
	public static PKValue FromBool(bool b)
	{
		return new PKValue(PKType.Bool, b ? 1 : 0);
	}
	public static PKValue FromDouble(double d)
	{
		return new PKValue(PKType.Double, (long)d);
	}

	public static PKType GetPKType(Type type)
	{
		//todo: this will only work for system types
		if (type == typeof(void))
		{
			return PKType.None;
		}
		
		if (type == typeof(PKValue))
		{
			return new PKType(PKKind.Any);
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
					return oft.Lifted();
				}else if (oftd == typeof(List<>))
				{
					return oft.Lifted();
				}else if(oftd == typeof(Array))//todo: untested.
				{
					return oft.Lifted();
				}
			}
		}
		
		return PKType.None;
	}

	override public string ToString()
	{
		return _ref?.ToString() ?? _scalar.ToString();
	}

}
