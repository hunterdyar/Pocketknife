namespace PocketknifeCore;

public enum PKKind
{
	None,
	Any,
	String,
	Int,
	Long,
	Bool,
	Double,
	File,
	Table,
}

public readonly struct PKType : IEquatable<PKType>
{
	public static readonly PKType None = new(PKKind.None,false);
	public static readonly PKType String = new(PKKind.String, false);
	public static readonly PKType Int = new(PKKind.Int, false);
	public static readonly PKType Long = new(PKKind.Long, false);
	public static readonly PKType Bool = new(PKKind.Bool, false);
	public static readonly PKType Double = new(PKKind.Double, false);
	public static readonly PKType File = new(PKKind.File, false);
	public static readonly PKType Table = new(PKKind.Table, false);
	public static readonly PKType Any = new(PKKind.Any, false);

	public readonly PKKind Kind;
	public readonly bool IsStream;//isEnumerable

	public PKType(PKKind kind, bool isStream = false)
	{
		Kind = kind;
		IsStream = isStream;
	}

	public static bool IsNone(PKType type) => type.Kind == PKKind.None;
	public bool IsNone() => Kind == PKKind.None;
	
	public PKType AsStream() => new PKType(Kind, true);
	public PKType AsSingle() => new PKType(Kind, false);

	override public string ToString() => IsStream ? $"<{Kind.ToString()}>" : Kind.ToString();
	
	#region IDE Generated Equality Members
	
	public bool Equals(PKType other)
	{
		return Kind == other.Kind && IsStream == other.IsStream;
	}

	public override bool Equals(object? obj)
	{
		return obj is PKType other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine((int)Kind, IsStream);
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