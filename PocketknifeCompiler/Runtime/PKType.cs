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
	public static readonly PKType None = new(PKKind.None);
	public static readonly PKType String = new(PKKind.String);
	public static readonly PKType Int = new(PKKind.Int);
	public static readonly PKType Long = new(PKKind.Long);
	public static readonly PKType Bool = new(PKKind.Bool);
	public static readonly PKType Double = new(PKKind.Double);
	public static readonly PKType File = new(PKKind.File);
	public static readonly PKType Table = new(PKKind.Table);
	public static readonly PKType Any = new(PKKind.Any);

	public readonly PKKind Kind;
	public readonly bool IsStream => LiftLevel>0;//isEnumerable
	public readonly byte LiftLevel = 0;
	public PKType(PKKind kind, byte liftLevel = 0)
	{
		Kind = kind;
		LiftLevel = liftLevel;
	}

	public static bool IsNone(PKType type) => type.Kind == PKKind.None;
	public bool IsNone() => Kind == PKKind.None;
	
	public PKType Lifted() => new PKType(Kind, (byte)(LiftLevel+1));
	public PKType Lowered() => new PKType(Kind, LiftLevel > 0 ? (byte)(LiftLevel-1) : (byte)0);

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