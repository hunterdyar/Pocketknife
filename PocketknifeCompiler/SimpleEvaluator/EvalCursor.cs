namespace PocketknifeCore.SimpleEvaluator;

//A position in the AST walk
public class EvalCursor
{
	public Stack<(Queue<PKNode> Remaining, int Depth)> Frames = new();
	public bool IsDone => Frames.Count == 0 || Frames.All(f => f.Remaining.Count == 0);

	public EvalCursor Clone()
	{
		var copy = new EvalCursor();
		foreach (var (q,d) in Frames.Reverse())
		{
			copy.Frames.Push((new Queue<PKNode>(q), d));
		}
		return copy;
	}
}
