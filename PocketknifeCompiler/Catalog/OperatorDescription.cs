using System.Reflection;

namespace PocketknifeCore;

public class OperatorDescription
{
	public readonly OpKind OpKind;
	public Type InType = PKType.None;
	public Type OutType = PKType.None;
	public required MethodInfo Method;
	
	public int ArgCount => FirstArgIsStream() ? Method.GetParameters().Length -1 : Method.GetParameters().Length;

	public bool FirstArgIsStream()
	{
		return OpKind != OpKind.Generator;
	}

	//isList, isGenerator, etc.
	public OperatorDescription(OpKind kind)
	{
		OpKind = kind;
	}
}

public class CastingDescription : IEquatable<CastingDescription>
{
	public bool Implicit => _isImplicit;
	private bool _isImplicit;
	public required Type InType = PKType.None;
	public required Type OutType = PKType.None;
	public required MethodInfo Method;

	public CastingDescription(bool isImplicit)
	{
		_isImplicit = isImplicit;
	}


	#region Generated Equality Members

	public bool Equals(CastingDescription? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;
		return _isImplicit == other._isImplicit && InType == other.InType && OutType == other.OutType;
	}

	public override bool Equals(object? obj)
	{
		if (obj is null) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((CastingDescription)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(_isImplicit, InType, OutType);
	}

	public static bool operator ==(CastingDescription? left, CastingDescription? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(CastingDescription? left, CastingDescription? right)
	{
		return !Equals(left, right);
	}

	#endregion

	public object ApplyNow(object pkValue)
	{
		if (Method is null)
		{
			throw new InvalidOperationException("Method not set for casting description");
		}


		var o = (Method.Invoke(null, [pkValue]) ?? throw new InvalidOperationException());
		return o;
	}
}

public enum OpKind
{
	Pipeline,
	Generator,
	Filter,
	Signal,
	PipeIn,
}