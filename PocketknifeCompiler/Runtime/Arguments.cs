namespace PocketknifeCore;

public class Arguments
{
	public object[] EvaluatedArgs;
	// True if any element of EvaluatedArgs is a VarRef and must be
	// resolved per-item at runtime. Set by the compiler.
	public bool HasVarRefs;
	// public OpInvoker? Unevaluated;
	
	public Arguments(object[] evaluatedArgs)
	{
		EvaluatedArgs = evaluatedArgs;
		HasVarRefs = false;
		for (int i = 0; i < evaluatedArgs.Length; i++)
		{
			if (evaluatedArgs[i] is VarRef)
			{
				HasVarRefs = true;
				break;
			}
		}
	}

	public static Arguments Empty = new Arguments(Array.Empty<Object>());
}

// represents an unresolved `@name` / `@^^name` argument.
public readonly struct VarRef
{
	public readonly Type Type;
	public readonly string Name;
	public readonly int ReachOut;
	public readonly CastingDescription? Cast;
	public VarRef(string name, int reachOut, Type type, CastingDescription? cast = null)
	{
		Name = name;
		ReachOut = reachOut;
		Type = type;
		Cast = cast;
	}
	public override string ToString() => "@" + new string('^', ReachOut) + Name;
	
}