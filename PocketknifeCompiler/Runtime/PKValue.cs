namespace PocketknifeCore;

public readonly struct PKValue
{
	public readonly PKKind Kind => _kind;
	private readonly PKKind _kind;
	private readonly long _scalar;
	private readonly object? _ref;

	public PKValue(PKKind kind, long scalar) : this()
	{
		_kind = kind;
		_scalar = scalar;
	}

	public PKValue(PKKind kind, object? @ref) : this()
	{
		_kind = kind;
		_ref = @ref;
	}

	public static readonly PKValue None = default;
	public bool IsNone => _kind == PKKind.None;

	public string AsString() => (string)_ref;
	public T RefAs<T>() => (T)_ref ?? throw new InvalidOperationException();
	// public T ScalarAs<T>() => (T)_scalar ?? throw new InvalidOperationException();
	public int AsInt => (int)_scalar;
	public long AsLong => (long)_scalar;
	public double AsDouble => (double)_scalar;
	public bool AsBool => _scalar != 0;

	public static PKValue FromString(string s)
	{
		return new PKValue(kind: PKKind.String, @ref: s);
	}

	public static PKValue FromInt(int i)
	{
		return new PKValue(kind: PKKind.Int, scalar: i);
	}
	
	public static PKValue FromLong(long l)
	{
		return new PKValue(kind: PKKind.Long, scalar: l);
	}
	
	public static PKValue FromBool(bool b)
	{
		return new PKValue(kind: PKKind.Bool, scalar: b ? 1 : 0);
	}
	public static PKValue FromDouble(double d)
	{
		return new PKValue(kind: PKKind.Double, scalar: (long)d);
	}
}
