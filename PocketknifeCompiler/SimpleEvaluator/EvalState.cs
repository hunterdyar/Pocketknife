namespace PocketknifeCore.SimpleEvaluator;

public struct EvalState
{
	public bool IsErr = false;
	public bool IsDone = false;
	public EvalState()
	{
	}
	
	public static EvalState Good()
	{
		return new EvalState();
	}

	public static EvalState Bad()
	{
		return new EvalState()
		{
			IsErr = true,
			IsDone = true,
		};
	}

	public static EvalState Done()
	{
		return new EvalState()
		{
			IsDone = true,
			IsErr = false
		};
	}
}