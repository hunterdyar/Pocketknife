namespace PocketknifeCore.SimpleEvaluator;

public struct EvalState
{
	public bool IsErr = false;
	public bool IsDone = false;
	public bool IsStarted = false;
	public int Depth;
	public EvalState(int depth)
	{
		Depth = depth;
	}
	public static EvalState None(int depth = 0)
	{
		return new EvalState()
		{
			IsErr = false,
			IsDone = false,
			IsStarted = false,
			Depth = depth
		};
	}

	public static EvalState Good(int depth)
	{
		return new EvalState()
		{
			IsStarted = true,
			IsErr = false,
			IsDone = false,
			Depth = depth
		};
	}

	public static EvalState Bad(int depth)
	{
		return new EvalState()
		{
			IsErr = true,
			IsDone = true,
			IsStarted = true,
			Depth = depth
		};
	}

	public static EvalState Done(int depth)
	{
		return new EvalState()
		{
			IsDone = true,
			IsErr = false,
			IsStarted = true,
			Depth = depth
		};
	}
}