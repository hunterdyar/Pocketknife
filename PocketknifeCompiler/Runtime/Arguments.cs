namespace PocketknifeCore;

public class Arguments
{
	public object[] EvaluatedArgs;
	// public OpInvoker? Unevaluated;
	
	public Arguments(object[] evaluatedArgs)
	{
		EvaluatedArgs = evaluatedArgs;
	}

	public static Arguments Empty = new Arguments(Array.Empty<Object>());
}