using PocketknifeCore.Compiler;

namespace PocketknifeCore.SimpleEvaluator;

//Steps through evaluation one line at a time.
public class LineEvaluator
{
	public EvalState Current => _current;
	private EvalState _current = EvalState.None();

	public Context? Context => _ctx;
	private Context? _ctx = new Context();

	private IEnumerator<EvalState> _execution;
	
	private PKNode? _root;

	public void Run()
	{
			
	}
	public void RunCurrentToEnd()
	{
		if (_current.IsDone)
		{
			return;
		}else if (_current.IsErr)
		{
			//error!
		}else if (_current.IsStarted)
		{
			while (!_current.IsDone && !_current.IsErr)
			{
				if (_execution.MoveNext())
				{
					_current = _execution.Current;
				}
				else
				{ 
					break;
				}
			}
		}
		else
		{
			if (_root == null)
			{
				throw new InvalidOperationException("No root node set.");
			}
			_ctx = new Context();
			int stepCount = 0;
			foreach (var state in SimpleEvaluator.Evaluate(_root, _current.Depth, _ctx))
			{
				if (state.IsErr)
				{
					return;
				}

				stepCount++;
			}
		}
	}

	public void Step()
	{
		if(_current.IsDone || _current.IsErr)
		{
			return;
		}

		if (!_current.IsStarted)
		{
			_ctx = new Context();
			_execution = SimpleEvaluator.Evaluate(_root, 0, _ctx).GetEnumerator();
		}
		
		var currentDepth = _current.Depth;
		if (_execution.MoveNext())
		{
			_current = _execution.Current;
		}
		else
		{
			//todo: not sure why this is failing in this way.
			_current = EvalState.Bad(_current.Depth);
			//throw new Exception("Execution ended unexpectedly (EvalState and enumerator mismatch).");
		}
	}

	public void StepOut()
	{
		if (_current.IsDone || _current.IsErr)
		{
			return;
		}

		if (!_current.IsStarted)
		{
			_ctx = new Context();
			_execution = SimpleEvaluator.Evaluate(_root, _current.Depth, _ctx).GetEnumerator();
		}

		var currentDepth = _current.Depth;
		if (_execution.MoveNext())
		{
			_current = _execution.Current;
			//keep going until we end up less deep current, then stop.
			while(_execution.MoveNext() && _execution.Current.Depth >= currentDepth)
			{
				_current = _execution.Current;
			}
		}
		else
		{
			throw new Exception("Execution ended unexpectedly (EvalState and enumerator mismatch).");
		}
	}

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
			_ctx = new Context();//would be fun to try to do on-the-fly recompilation.
			_current = EvalState.None();
		}
	}
	
	public void Reset()
	{
		_ctx = new Context();
		_current = EvalState.None();
	}
}