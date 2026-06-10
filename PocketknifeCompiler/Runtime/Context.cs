using System.Collections;

namespace PocketknifeCore;

// Layered, breadth-first runtime.
// _timeline is the history of layers; the current layer is the last one.
// _scopes records open scopes ('.', '.@x', '|>' or '>'-expansion at index >= 1).
public class Context
{
	private readonly List<PKLayer> _timeline = new();
	private readonly Stack<ScopeInfo> _scopes = new();

	// Set per iteration in *OnEach so ops (and EvaluateArguments) can ask "which item am I on?"
	public PKItem? CurrentItem { get; private set; }

	public Context()
	{
		// Synthetic root layer at index 0.
		// This is the implicit accumulator for top-level `&` branches and the anchor that all scope StartLayerIndexes can refer back to. The first real input layer is added at index 1.
		_timeline.Add(new PKLayer(typeof(void)));
	}

	private PKLayer Top => _timeline[^1];
	private bool IsAtRoot => _timeline.Count == 1; // only the synthetic root exists.
	
	public void PushStreamWithGenerator(Type inputType, object[] ia, GenInvoker generator)
	{
		if (IsAtRoot)
		{
			// first input: seed layer 1, push a root expansion scope rooted at the (synthetic) layer 0 so PopFrame always has matching state.
			var seedValues = generator.Invoke(ia, this);
			var layer = new PKLayer(inputType);
			foreach (var v in seedValues) layer.Items.Add(new PKItem(v));
			_scopes.Push(new ScopeInfo
			{
				StartLayerIndex = 0,
				IsExpansionScope = true,
			});
			_timeline.Add(layer);
			return;
		}

		// nested expansion: each item in current layer fans out to its own list of children.
		var parent = Top;
		var expanded = new PKLayer(inputType);
		// scope start = index of the layer BEFORE expansion (i.e., the current top).
		var scope = new ScopeInfo
		{
			StartLayerIndex = _timeline.Count - 1,
			IsExpansionScope = true,
			Name = null,
		};
		foreach (var p in parent.Items)
		{
			CurrentItem = p;
			var children = generator.Invoke(ia, this);
			int idx = 0;
			int count = children.Count;
			foreach (var v in children)
			{
				var child = new PKItem(v, p);
				child.Index = idx;
				//todo: test if this is more performant or not.
				//store the evergreen variables as values and check later, so we keep the dictionary lazy.
				// child.Bind("Index", idx);
				// child.Bind("Count", count);
				expanded.Items.Add(child);
				idx++;
			}
		}
		CurrentItem = null;
		_timeline.Add(expanded);
		_scopes.Push(scope);
	}

	//per-item transitions
	public void OperateOnEach(object[] arguments, OpInvoker invoker)
	{
		var prev = Top;
		var next = new PKLayer(prev.Type);
		bool hasVars = ArgsNeedRuntimeEval(arguments);
		object[] resolved = hasVars ? new object[arguments.Length] : arguments;
		foreach (var p in prev.Items)
		{
			CurrentItem = p;
			var args = hasVars ? ResolveArgs(arguments, resolved, p) : arguments;
			var result = invoker(p.Value!, args, this);
			next.Items.Add(new PKItem(result, p));
		}
		CurrentItem = null;
		_timeline.Add(next);
	}

	public void SignalOnEach(object[] arguments, OpInvoker sInvoker)
	{
		var prev = Top;
		bool hasVars = ArgsNeedRuntimeEval(arguments);
		object[] resolved = hasVars ? new object[arguments.Length] : arguments;
		// Signals do not advance a layer; they are pure side-effects.
		foreach (var p in prev.Items)
		{
			CurrentItem = p;
			var args = hasVars ? ResolveArgs(arguments, resolved, p) : arguments;
			sInvoker(p.Value!, args, this);
		}
		CurrentItem = null;
	}

	public void FilterOnEach(object[] arguments, OpInvoker foprInvoker)
	{
		var prev = Top;
		var next = new PKLayer(prev.Type);
		bool hasVars = ArgsNeedRuntimeEval(arguments);
		object[] resolved = hasVars ? new object[arguments.Length] : arguments;
		foreach (var p in prev.Items)
		{
			CurrentItem = p;
			var args = hasVars ? ResolveArgs(arguments, resolved, p) : arguments;
			if ((bool)foprInvoker(p.Value!, args, this))
			{
				next.Items.Add(new PKItem(p.Value, p));
			}
		}
		CurrentItem = null;
		_timeline.Add(next);
	}

	//the only current "runtime eval" is a VarRef.
	//todo: cache this information during compilation and save the for loop. it's cheap tho.
	private static bool ArgsNeedRuntimeEval(object[] args)
	{
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] is VarRef) return true;
		}

		return false;
	}

	private object[] ResolveArgs(object[] src, object[] dst, PKItem item)
	{
		for (int i = 0; i < src.Length; i++)
		{
			dst[i] = src[i] is VarRef vr ? ResolveVariable(item, vr.Name, vr.ReachOut) : src[i];
		}
		return dst;
	}

	public object ResolveVariable(PKItem item, string name, int reachOut)
	{
		PKItem? cur = item;
		//first, skip the number of @^^^^name reach outs.
		for (int i = 0; i < reachOut && cur != null; i++)
		{
			cur = cur.Progenitor;
		}
		//then, walk up the bindings to get the value.
		while (cur != null)
		{
			if (cur.Bindings != null && cur.TryGetValue(name, out var v))
			{
				return v;
			}
			cur = cur.Progenitor;
		}
		throw new Exception($"variable {name} not found");
	}

	
	public void Pack()
	{
		var top = _timeline[^1];
		var packedList = new List<object>(top.Items.Count);
		foreach (var it in top.Items)
		{
			packedList.Add(it.Value!);
		}
		var packed = new PKLayer(top.Type.Lift());
		
		// Progenitor chain: link the single packed item to one representative item
		// in the prior layer so variable lookup still works through Pack/Unpack.
		var progen = top.Items.Count > 0 ? top.Items[0] : null;
		packed.Items.Add(new PKItem(packedList, progen));
		_timeline.Add(packed);
	}

	public void Unpack()
	{
		var top = _timeline[^1];
		if (!top.Type.IsStream())
		{
			throw new Exception("cannot unpack a non-stream type");
		}
		var unpacked = new PKLayer(top.Type.Lower());
		foreach (var it in top.Items)
		{
			var value = it.Value;
			if (value is IEnumerable enumerable && value is not string)
			{
				foreach (var v in enumerable)
					unpacked.Items.Add(new PKItem(v!, it.Progenitor));
			}
			else
			{
				unpacked.Items.Add(new PKItem(value, it.Progenitor));
			}
		}
		_timeline.Add(unpacked);
	}
	
	public void NewFrame()
	{
		_scopes.Push(new ScopeInfo
		{
			StartLayerIndex = _timeline.Count - 1,
			IsExpansionScope = false,
			Name = null,
		});
	}

	public void NewNamedFrame(string? name = null)
	{
		_scopes.Push(new ScopeInfo
		{
			StartLayerIndex = _timeline.Count - 1,
			IsExpansionScope = false,
			Name = name,
		});
	}

	public void PopFrame(BranchType frameType, PopType popType)
	{
		// scope was opened. Merge accordingly.
		if (_scopes.Count > 0)
		{
			var scope = _scopes.Pop();
			var startIdx = scope.StartLayerIndex;
			var startLayer = _timeline[startIdx];
			var currentLayer = _timeline[^1];

			switch (frameType)
			{
				case BranchType.SideEffect:
				{
					// SideEffect discards the value changes from the body, but a named binding captured inside is still propagated onto the corresponding start-layer items.
					if (scope.Name != null)
					{
						var startSet = new HashSet<PKItem>(startLayer.Items);
						foreach (var it in currentLayer.Items)
						{
							if (it.Value == null) continue;
							// Walk progenitor chain back to a start-layer item ancestor.
							var anc = it;
							while (anc != null && !startSet.Contains(anc))
							{
								anc = anc.Progenitor;
							}

							if (anc != null)
							{
								anc.Bind(scope.Name, it.Value);
							}
						}
					}
					// discard everything after startIdx.
					_timeline.RemoveRange(startIdx + 1, _timeline.Count - startIdx - 1);
					break;
				}
				case BranchType.Replace:
				{
					// keep current layer's items, but rebase them as 1-1 results of startLayer's items.
					var merged = new PKLayer(currentLayer.Type);
					if (currentLayer.Items.Count > 0 && startLayer.Items.Count == currentLayer.Items.Count && !scope.IsExpansionScope)
					{
						// keep the start-layer item itself in the progenitor chain so any outer binding sitting on it (potentially shadowed by this scope's own binding) is reachable as a true ancestor via `@^name`.
						// For unnamed scopes there's nothing to shadow, so we preserve the historical behavior of rebasing to `p.Progenitor` to keep the chain flat.
						bool keepStartItemAsProgenitor = scope.Name != null;
						for (int i = 0; i < startLayer.Items.Count; i++)
						{
							var p = startLayer.Items[i];
							var v = currentLayer.Items[i].Value;
							var item = new PKItem(v, keepStartItemAsProgenitor ? p : p.Progenitor);
							if (scope.Name != null) item.Bind(scope.Name, v!);
							merged.Items.Add(item);
						}
					}
					else
					{
						//collapse current values into a flat layer parented to startLayer's parents.
						foreach (var it in currentLayer.Items)
						{
							var item = new PKItem(it.Value, it.Progenitor);
							merged.Items.Add(item);
						}
					}
					_timeline.RemoveRange(startIdx + 1, _timeline.Count - startIdx - 1);
					_timeline[startIdx] = merged;
					break;
				}
				case BranchType.ListAppend:
				{
					// append current items into the start layer (flat).
					var merged = new PKLayer(currentLayer.Type);
					foreach (var it in startLayer.Items) merged.Items.Add(it);
					foreach (var it in currentLayer.Items)
					{
						merged.Items.Add(new PKItem(it.Value, it.Progenitor));
					}
					_timeline.RemoveRange(startIdx + 1, _timeline.Count - startIdx - 1);
					_timeline[startIdx] = merged;
					break;
				}
			}
			return;
		}

		// no scope open — root-level pop on the very first input branch.
		var top = _timeline[^1];
		switch (frameType)
		{
			case BranchType.SideEffect:
				_timeline.RemoveAt(_timeline.Count - 1);
				break;
			case BranchType.ListAppend:
				if (_timeline.Count >= 2)
				{
					// merge into previous accumulator if present.
					_timeline.RemoveAt(_timeline.Count - 1);
					var acc = _timeline[^1];
					foreach (var it in top.Items)
					{
						acc.Items.Add(new PKItem(it.Value, it.Progenitor));
					}
				}
				else
				{
					throw new NotImplementedException();
				}
				break;
			case BranchType.Replace:
				if (_timeline.Count >= 2)
				{
					_timeline.RemoveAt(_timeline.Count - 2);
				}
				else
				{
					//uh, i think we actually just let this layer exist, on top of "nothing" is the same as replace?
					throw new NotImplementedException();
				}
				break;
		}
	}

	private void BindIfNamed(ScopeInfo scope, PKLayer target, bool collectSelf)
	{
		if (scope.Name == null) return;
		// SideEffect with name: bind start values onto items (rare but supported).
		foreach (var it in target.Items)
		{
			if (it.Value != null) it.Bind(scope.Name, it.Value);
		}
	}
}

public enum PopType
{
	ClonedBranch,
	SubBranch,
}
