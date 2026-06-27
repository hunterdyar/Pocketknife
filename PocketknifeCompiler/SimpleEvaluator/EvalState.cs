namespace PocketknifeCore.SimpleEvaluator;

public struct EvalState
{
	public bool IsErr = false;
	public bool IsDone = false;
	public bool IsStarted = false;
	public EvalState()
	{
	}

	public static EvalState None()
	{
		return new EvalState()
		{
			IsErr = false,
			IsDone = false,
			IsStarted = false
		};
	}

	public static EvalState Good()
	{
		return new EvalState()
		{
			IsStarted = true,
			IsErr = false,
			IsDone = false
		};
	}

	public static EvalState Bad()
	{
		return new EvalState()
		{
			IsErr = true,
			IsDone = true,
			IsStarted = true
		};
	}

	public static EvalState Done()
	{
		return new EvalState()
		{
			IsDone = true,
			IsErr = false,
			IsStarted = true
		};
	}
}