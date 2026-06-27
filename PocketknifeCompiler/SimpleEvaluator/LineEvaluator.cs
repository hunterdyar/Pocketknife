using PocketKnife.Compiler;

namespace PocketknifeCore.SimpleEvaluator;

//Steps through evaluation one line at a time.
public class LineEvaluator()
{
	private EvalState _current = EvalState.None();

	public Context? Context => _ctx;
	private Context? _ctx;

	private IEnumerable<EvalState> _execution;
	
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
			using var execEnum = _execution.GetEnumerator();
			while (!_current.IsDone && !_current.IsErr)
			{
				if (execEnum.MoveNext())
				{
					_current = execEnum.Current;
				}
				else
				{
					throw new Exception("Execution ended unexpectedly (EvalState and enumerator mismatch).");
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
			foreach (var state in SimpleEvaluator.Evaluate(_root, _ctx))
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
			_execution = SimpleEvaluator.Evaluate(_root, _ctx);
		}
		
		using var execEnum = _execution.GetEnumerator();
		if (execEnum.MoveNext())
		{
			_current = execEnum.Current;
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
			//reset when recompiled i think
			_root = rootNode;
			_ctx = new Context();//would be fun to try to do on-the-fly recompilation.
			_current = EvalState.None();
		}
	}
	
	public void Reset()
	{
		_ctx = null;
		_current = EvalState.None();
	}
}