namespace PocketknifeCore;

public class Arguments
{
	public PKValue[] EvaluatedArgs;
	// public OpInvoker? Unevaluated;
	
	public Arguments(PKValue[] evaluatedArgs)
	{
		EvaluatedArgs = evaluatedArgs;
	}

	public static Arguments Empty = new Arguments(Array.Empty<PKValue>());
}