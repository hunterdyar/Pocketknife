using System.Collections;
using System.Diagnostics;

namespace PocketknifeCore;

// Layered, breadth-first runtime.
// _timeline is the history of layers; the current layer is the last one.
// _scopes records open scopes ('.', '.@x', '|>' or '>'-expansion at index >= 1).
public class Context
{
	public int TimelineLength => _timeline.Count;
	public int MaxTimelineLength;
	public int MaxScopeDepth;
	
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
			for (var i = 0; i < seedValues.Count; i++)
			{
				layer.Items.Add(new PKItem(seedValues[i], null, i));
			}

			_scopes.Push(new ScopeInfo
			{
				StartLayerIndex = 0,//root
				IsExpansionScope = true,
			});
			_timeline.Add(layer);
			MaxTimelineLength = Math.Max(MaxTimelineLength, _timeline.Count);
			MaxScopeDepth = Math.Max(MaxScopeDepth, _scopes.Count);
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
		//iterate over the loop, applying to only matching items. If we aren't in a branch (armID == null), then we won't bother doing the check for every item.
		
		foreach (var p in parent.Items)
		{
			//inactive parents. don't invoke...
			//but we still need to add them to the timeline so that the progenitor chain is correct.
			//...IsActive continues to recognize it as belonging to the inactive arm on subsequent layers.
			if (!IsActive(p))
			{
				var leaf = new PKItem(p.Value, p);
				leaf.Index = 0;
				expanded.Items.Add(leaf);
				continue;
			}
			CurrentItem = p;
			var children = generator.Invoke(ia, this);
			int idx = 0;
			foreach (var v in children)
			{
				var child = new PKItem(v, p, idx);
				//todo: test if this is more performant or not.
				expanded.Items.Add(child);
				idx++;
			}
		}

		CurrentItem = null;
		_timeline.Add(expanded);
		_scopes.Push(scope);
		MaxTimelineLength = Math.Max(MaxTimelineLength, _timeline.Count);
		MaxScopeDepth = Math.Max(MaxScopeDepth, _scopes.Count);
	}

	public void PushStreamWithPipeGenerator(Type inputType, object[] arguments, PipeGenInvoker generator)
	{
		if (IsAtRoot)
		{
			throw new InvalidOperationException();
		}

		// nested expansion: each item in current layer fans out to its own list of children.
		var parent = Top;
		var expanded = new PKLayer(inputType);
		bool hasVars = ArgsNeedRuntimeEval(arguments);
		object[] resolved = hasVars ? new object[arguments.Length] : arguments;

		// scope start = index of the layer BEFORE expansion (i.e., the current top).
		var scope = new ScopeInfo
		{
			StartLayerIndex = _timeline.Count - 1,
			IsExpansionScope = true,
			Name = null,
		};
		foreach (var p in parent.Items)
		{
			//see note in PushStreamWithGenerator.
			if (!IsActive(p))
			{
				var leaf = new PKItem(p.Value, p, 0);
				expanded.Items.Add(leaf);
				continue;
			}
			
			CurrentItem = p;
			var args = hasVars ? ResolveArgs(arguments, resolved, p) : arguments;
			var children = generator.Invoke(p.Value!, args, this);
			int idx = 0;
			// int count = children.Count;
			foreach (var v in children)
			{
				var child = new PKItem(v, p, idx);
				//todo: see gen todo
				expanded.Items.Add(child);
				idx++;
			}
		}
		
		CurrentItem = null;
		_timeline.Add(expanded);
		_scopes.Push(scope);
		MaxTimelineLength = Math.Max(MaxTimelineLength, _timeline.Count);
		MaxScopeDepth = Math.Max(MaxScopeDepth, _scopes.Count);
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
			if (IsActive(p))
			{
				CurrentItem = p;
				var args = hasVars ? ResolveArgs(arguments, resolved, p) : arguments;
				var result = invoker(p.Value!, args, this);
				next.Items.Add(new PKItem(result, p, p.Index));
			}
			else
			{
				//not my branch, but keep in timeline.
				next.Items.Add(new PKItem(p.Value, p, p.Index));
			}
		}
		CurrentItem = null;
		_timeline.Add(next);
		MaxTimelineLength = Math.Max(MaxTimelineLength, _timeline.Count);
	}

	public void SignalOnEach(object[] arguments, OpInvoker sInvoker)
	{
		var prev = Top;
		bool hasVars = ArgsNeedRuntimeEval(arguments);
		object[] resolved = hasVars ? new object[arguments.Length] : arguments;
		// Signals do not advance a layer; they are pure side-effects.
		foreach (var p in prev.Items)
		{
			if (IsActive(p))
			{
				CurrentItem = p;
				var args = hasVars ? ResolveArgs(arguments, resolved, p) : arguments;
				sInvoker(p.Value!, args, this);
			}
		}
		CurrentItem = null;
	}

	public void FilterOnEach(object[] arguments, OpInvoker foprInvoker)
	{
		var prev = Top;
		var next = new PKLayer(prev.Type);
		bool hasVars = ArgsNeedRuntimeEval(arguments);
		object[] resolved = hasVars ? new object[arguments.Length] : arguments;
		int idx = 0;
		foreach (var p in prev.Items)
		{
			if (!IsActive(p))
			{
				//todo: add test for new index after filtering
				next.Items.Add(new PKItem(p.Value, p, idx));
				idx++; 
				continue;
			}

			CurrentItem = p;
			var args = hasVars ? ResolveArgs(arguments, resolved, p) : arguments;
			if ((bool)foprInvoker(p.Value!, args, this))
			{
				next.Items.Add(new PKItem(p.Value, p, idx));
				idx++;
			}
			
		}
		CurrentItem = null;
		_timeline.Add(next);
		MaxTimelineLength = Math.Max(MaxTimelineLength, _timeline.Count);
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
			dst[i] = src[i] is VarRef vr ? ResolveVariable(item, vr.Name, vr.ReachOut, vr.Cast) : src[i];
		}
		return dst;
	}

	public object ResolveVariable(PKItem item, string name, int reachOut, CastingDescription? cast = null)
	{
		PKItem? cur = item;
		//first, skip the number of @^^^^name reach outs.
		int reachedPast = reachOut;

		//walk up the bindings to get the value.
		object value = null;
		while (cur != null)
		{
			if (cur.TryGetValue(name, out value))
			{
				//valid binding found, we return or skip it.
				if(reachedPast > 0)
				{
					reachedPast--;
					cur = cur.Progenitor;
					continue;
				}
				if (cast != null)
				{
					//todo: replace with compiled invoker.
					value = cast.ApplyNow(value);
				}
				return value;
			}
			cur = cur.Progenitor;
		}

		if (value != null && reachOut > 0)
		{
			throw new Exception($"variable {name} exists, but it was skipped by ^ reach-outs. Value not found.");
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
		MaxTimelineLength = Math.Max(MaxTimelineLength, _timeline.Count);
	}

	public void Unpack()
	{
		var top = _timeline[^1];
		if (!top.Type.IsStream())
		{
			throw new Exception("cannot unpack a non-stream type");
		}
		var unpacked = new PKLayer(top.Type.Lower());
		int idx = 0;
		foreach (var it in top.Items)
		{
			var value = it.Value;
			if (value is IEnumerable enumerable && value is not string)
			{
				foreach (var v in enumerable)
				{
					unpacked.Items.Add(new PKItem(v!, it.Progenitor, idx));
					idx++;
				}
			}
			else
			{
				unpacked.Items.Add(new PKItem(value, it.Progenitor, idx));
				idx++;
			}
		}
		_timeline.Add(unpacked);
		MaxTimelineLength = Math.Max(MaxTimelineLength, _timeline.Count);
	}
	
	private void NewClonedLayer()
	{
		var top = _timeline[^1];
		//todo: not a deep enough clone? idk if that matters for reference objects.
		var cloned = new PKLayer(top.Type)
		{
			Items = new List<PKItem>(top.Items)
		};
		_timeline.Add(cloned);
		MaxTimelineLength = Math.Max(MaxTimelineLength, _timeline.Count);
	}
	
	public void NewFrame()
	{
		_scopes.Push(new ScopeInfo
		{
			StartLayerIndex = _timeline.Count - 1,
			IsExpansionScope = false,
			Name = null,
		});
		MaxScopeDepth = Math.Max(MaxScopeDepth, _scopes.Count);
		NewClonedLayer();
	}

	public void NewNamedFrame(string? name = null)
	{
		_scopes.Push(new ScopeInfo
		{
			StartLayerIndex = _timeline.Count - 1,
			IsExpansionScope = false,
			Name = name,
		});
		MaxScopeDepth = Math.Max(MaxScopeDepth, _scopes.Count);
		NewClonedLayer();
	}

	public void PopFrame(BranchType frameType, bool keepHistory = false)
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
						// keep the start-layer item in the progenitor chain so any outer binding sitting on it (potentially shadowed by this scope's own binding) is reachable as an ancestor via @^name.
						// For unnamed scopes there's nothing to shadow, so we preserve the historical behavior of rebasing to `p.Progenitor` to keep the chain flat.
						// keepHistory=true (pattern-match arm exit) must also preserve the start-layer item in the progenitor chain so IsActive() can walk back to the [umbrella partition] layer where ArmID stamps live.
						bool keepStartItemAsProgenitor = scope.Name != null || keepHistory;
						for (int i = 0; i < startLayer.Items.Count; i++)
						{
							var p = startLayer.Items[i];
							var v = currentLayer.Items[i].Value;
							var item = new PKItem(v, keepStartItemAsProgenitor ? p : p.Progenitor, p.Index);//we can copy index over because the counts are the same. i think?
							Debug.Assert(p.Index == i);
							if (scope.Name != null)
							{
								item.Bind(scope.Name, v!);
							}
							merged.Items.Add(item);
						}
					}
					else
					{
						//collapse current values into a flat layer parented to startLayer's parents.
						int idx = merged.Items.Count;
						foreach (var it in currentLayer.Items)
						{
							var item = new PKItem(it.Value, it.Progenitor, idx++);
							merged.Items.Add(item);
						}
					}

					if (keepHistory)
					{
						_timeline.Add(merged);	
					}
					else
					{
						_timeline.RemoveRange(startIdx + 1, _timeline.Count - startIdx - 1);
						_timeline[startIdx] = merged;
					}
					
					MaxTimelineLength = Math.Max(MaxTimelineLength, _timeline.Count);
					break;
				}
				case BranchType.ListAppend:
				{
					// append current items into the start layer (flat).
					var merged = new PKLayer(currentLayer.Type);
					int idx = 0;
					foreach (var it in startLayer.Items)
					{
						merged.Items.Add(it);
						idx++;
					}
					foreach (var it in currentLayer.Items)
					{
						merged.Items.Add(new PKItem(it.Value, it.Progenitor, idx));
						idx++;
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
					int idx = acc.Items.Count;
					foreach (var it in top.Items)
					{
						acc.Items.Add(new PKItem(it.Value, it.Progenitor, idx));
						idx++;
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

	public void BeginPatternMatch(OpInvoker[] filters, object[][] filterArgs, bool hasAlternate)
	{
		var startIdx = _timeline.Count - 1;
		//NewClonedLayer()-style clone where the cloning helper stamps ArmID on each new PKItem according to the first matching filter (with CurrentItem set during evaluation so bindings resolve).
		var top = _timeline[^1];
		var cloned = new PKLayer(top.Type)
		{
			Items = new List<PKItem>(top.Items)
		};
		int alternate = filters.Length;
		foreach (var item in cloned.Items)
		{
			for (int i = 0; i < filters.Length; i++)
			{
				var f = (bool)filters[i](item.Value, filterArgs[i], this);
				if (f)
				{
					item.ArmID = i;
					break;
				}
			}

			if (hasAlternate && item.ArmID == null)
			{
				item.ArmID = alternate;
			}
			
		}
		//
		_timeline.Add(cloned);
		
		var s = new ScopeInfo()
		{
			StartLayerIndex = startIdx,
			ArmID = null,
			IsArmUmbrella = true,
		};
		_scopes.Push(s);

		MaxTimelineLength = Math.Max(MaxTimelineLength, _timeline.Count);
		MaxScopeDepth = Math.Max(MaxScopeDepth, _scopes.Count);
		
		
	}

	public void EnterArm(int i)
	{
		// Find the umbrella's partition layer — that is the layer where ArmID stamps live, and must be used as this arm's StartLayerIndex so IsActive() can walk back to it via Progenitor to determine arm membership.
		int partitionIdx = -1;
		foreach (var s in _scopes)
		{
			if (s.IsArmUmbrella)
			{
				partitionIdx = s.StartLayerIndex + 1;
				break;
			}
		}
		if (partitionIdx < 0)
		{
			throw new Exception("EnterArm called outside an active pattern match umbrella scope");
		}

		_scopes.Push(new ScopeInfo
		{
			StartLayerIndex = partitionIdx,
			ArmID = i,
			IsExpansionScope = false,
			Name = null,
		});
		MaxScopeDepth = Math.Max(MaxScopeDepth, _scopes.Count);
		// Clone the current top: this preserves accumulated transformations from prior arms (inactive items pass through unchanged), so each arm layers its changes on top of the previous arm's merged result.
		NewClonedLayer();
	}

	public void ExitArm(BranchType closeType)
	{
		PopFrame(closeType, true);
	}

	public void EndPatternMatch()
	{
		if (_scopes.TryPeek(out var scope))
		{
			if (scope.IsArmUmbrella)
			{
				_scopes.Pop();
			}
			else
			{
				throw new Exception("no active pattern match");
			}
		}
		else
		{
			throw new Exception("no active pattern scope");
		}
	}

	private bool IsActive(PKItem item)
	{
		foreach (var scope in _scopes)
		{
			if (scope.IsArmUmbrella)
			{
				//inside ? but not inside specific arm body.
				return true;
			}

			if (scope.ArmID != null)
			{
				//walk it back to a progenitor that lives in this arm scopes start layer.
				var startLayer = _timeline[scope.StartLayerIndex];
				var cur = item;
				while (cur != null)
				{
					//
					if (startLayer.Items.Contains(cur)) //todo: optimize with HashSet<PKItem> per arm scope (cached on ScopeInfo or computed lazily?) for the start-layer membership check.
					{
						return cur.ArmID == scope.ArmID;
					}
					cur = cur.Progenitor;
				}

				return false;
			}
		}

		return true;//no arm context, all active.
	}
}