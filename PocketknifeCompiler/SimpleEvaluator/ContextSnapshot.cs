using PocketknifeCore.SimpleEvaluator;

namespace PocketknifeCore;

public class ContextSnapshot
{
	public EvalState StateBefore;
	public List<PKLayer> Timeline;
	public Stack<ScopeInfo> Scopes;
	public EvalCursor Cursor;
}