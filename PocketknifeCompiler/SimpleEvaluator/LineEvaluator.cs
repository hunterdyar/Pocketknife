using PocketknifeCore.Compiler;

namespace PocketknifeCore.SimpleEvaluator;

//Steps through evaluation one line at a time.
public class LineEvaluator
{
	private EvalCursor? _cursor; // replaces _execution
	private readonly Stack<ContextSnapshot> _undoStack = new();

	public bool CanStep => !_current.IsDone && !_current.IsErr;
	public bool CanStepBack => _undoStack.Count > 0;
	
	public EvalState Current => _current;
	private EvalState _current = EvalState.NotStarted();

	public Context? Context => _ctx;

	private Context? _ctx = new Context();
	
	private PKNode? _root;

	public void Run()
	{
			
	}
	public void RunCurrentToEnd()
	{
		Step();
		//todo: re-implement
		while (!_current.IsDone && !_current.IsErr)
		{
			Step();
		}
	}

	public void Step()
	{
		if (_current.IsDone || _current.IsErr) return;

		if (!_current.IsStarted)
		{
			_ctx = new Context();
			_cursor = SimpleEvaluator.CreateCursor(_root); // build initial cursor
			_undoStack.Clear();
		}

		// Snapshot BEFORE the step (includes cursor clone)
		_undoStack.Push(TakeSnapshot(_current));
		_current = SimpleEvaluator.StepOnce(_cursor, _ctx); // advance one node
	}

	public void StepBack()
	{
		if (!CanStepBack) return;

		var snap = _undoStack.Pop();
		_ctx.RestoreFrom(snap);
		_cursor = snap.Cursor; // restore cursor — no replay needed
		_current = snap.StateBefore;
	}

	private ContextSnapshot TakeSnapshot(EvalState stateBefore) => new()
	{
		StateBefore = stateBefore,
		Timeline = _ctx.Timeline.Select(Context.CloneLayer).ToList(),
		Scopes = new Stack<ScopeInfo>(_ctx.Scopes.Reverse()),
		Cursor = _cursor!.Clone(),
	};
	
	public void SetRoot(PKNode rootNode)
	{
		if (rootNode == null)
		{
			throw new ArgumentNullException(nameof(rootNode));
		}
		
		if (_root != rootNode)
		{
			//reset when recompiled I think
			_root = rootNode;
			_undoStack.Clear();
			_ctx = new Context();//would be fun to try to do on-the-fly recompilation.
			_current = EvalState.NotStarted();
		}
	}
	
	public void Reset()
	{
		_ctx = new Context();
		_current = EvalState.NotStarted();
		_undoStack.Clear();
		_cursor = null;
	}
}