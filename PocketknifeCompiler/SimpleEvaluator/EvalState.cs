namespace PocketknifeCore.SimpleEvaluator;

public enum EvalPhase
{
	NotStarted,
	Running,
	Complete,
	Error
}

public readonly struct EvalState
{
	public EvalPhase Phase { get; init; }
	public int Depth { get; init; }
	public SourceSlice Evaluated { get; init; }

	// Convenience properties kept for minimal call-site churn
	public bool IsStarted => Phase != EvalPhase.NotStarted;
	public bool IsDone => Phase == EvalPhase.Complete;
	public bool IsErr => Phase == EvalPhase.Error;

	public static EvalState NotStarted(int depth = 0) => new() { Phase = EvalPhase.NotStarted, Depth = depth };
	public static EvalState Running(int depth, SourceSlice evaluated) => new() { Phase = EvalPhase.Running, Depth = depth, Evaluated = evaluated };
	public static EvalState Complete(int depth, SourceSlice evaluated) => new() { Phase = EvalPhase.Complete, Depth = depth, Evaluated = evaluated };
	public static EvalState Error(int depth, SourceSlice evaluated) => new() { Phase = EvalPhase.Error, Depth = depth, Evaluated = evaluated };
}