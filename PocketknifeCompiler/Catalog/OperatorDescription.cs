using System.Reflection;

namespace PocketknifeCore;

public class OperatorDescription
{
	public readonly OpKind OpKind;
	public PKType InType = PKType.None;
	public PKType OutType = PKType.None;
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

public enum OpKind
{
	Pipeline,
	Generator,
	Filter,
	Signal,
	PipeIn,
}