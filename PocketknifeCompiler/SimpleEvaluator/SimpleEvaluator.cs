using System.Diagnostics;

namespace PocketknifeCore.SimpleEvaluator;

public static class SimpleEvaluator
{
	public static void EvaluateAll(PKNode node, Context ctx = null)
	{
		int stepCount = 0;
		foreach (var state in Evaluate(node,0, ctx))
		{
			if (state.IsErr)
			{
				return;
			}
			stepCount++;
		}
	}
	public static IEnumerable<EvalState> Evaluate(PKNode node, int depth, Context ctx = null)
	{
		switch (node)
		{
			case PKNodeGroup group:
				foreach (var n in group.Nodes)
				{
					foreach (var state in Evaluate(n,depth+1, ctx))
					{
						yield return state;
					}
				}
				break;
			case PKInputBranch branch:
				//push the input stream onto the stack.
				foreach (var evalState in Evaluate(branch.Input, depth, ctx)) yield return evalState;
				//take that and operate on it
				foreach (var evalState in Evaluate(branch.Body, depth + 1, ctx)) yield return evalState;
				
				//pop the (input) stream off of the branch.
				ctx.PopFrame(branch.BranchType);
				break;
			case PKGenInputProvider input:
				var ia = EvaluateArguments(input.Arguments, ctx);
				ctx.PushStreamWithGenerator(input.Type, ia, input.Generator);
				yield return EvalState.Running(depth, input.Span);
				break;
			case PKPipeInputProvider input:
				var pia = EvaluateArguments(input.Arguments, ctx);
				ctx.PushStreamWithPipeGenerator(input.Type, pia, input.PipeGenerator);
				yield return EvalState.Running(depth, input.Span);
				break;
			case PKFilterOperatorNode fopr:
				var fa = EvaluateArguments(fopr.Arguments, ctx);
				ctx.FilterOnEach(fa, fopr.Invoker);
				yield return EvalState.Running(depth, fopr.Span);
				break;
			case PKSignalOperatorNode sopr:
				var soprArguments = EvaluateArguments(sopr.Arguments, ctx);
				ctx.SignalOnEach(soprArguments, sopr.Invoker);
				yield return EvalState.Running(depth, sopr.Span);
				break;
			case PKInlineOperatorNode iopr:
				var ioprArguments = EvaluateArguments(iopr.Arguments, ctx);
				ctx.OperateOnEach(ioprArguments, iopr.Invoker);
				yield return EvalState.Running(depth, iopr.Span);
				break;
			case PKPack pack:
				ctx.Pack();
				yield return EvalState.Running(depth, pack.Span);
				break;
			case PKUnpack unpack:
				ctx.Unpack();
				yield return EvalState.Running(depth, unpack.Span);
				break;
			case PKNamedBranch namedBranch:
				ctx.NewNamedFrame(namedBranch.Label);
				//yield return EvalState.Good(depth, namedBranch.Span);
				foreach (var state in Evaluate(namedBranch.Body, depth + 1, ctx)) yield return state;
				ctx.PopFrame(namedBranch.Type);
				yield return EvalState.Running(depth, namedBranch.Span);
				break;
			case PKBranch branch:
				ctx.NewFrame();
				//yield return EvalState.Good(depth, branch.Span);
				foreach (var state in Evaluate(branch.Body, depth + 1, ctx)) yield return state;
				ctx.PopFrame(branch.Type);
				yield return EvalState.Running(depth, branch.Span);
				break;
			case PKPatternMatch patternMatch:
				//todo: optimize for allocations
				var filters = patternMatch.Branches.Select(x => x.Filter);
				var args = patternMatch.Branches.Select(x => x.Arguments);
				//todo: alternate needs to be in the list of iterators, not separate.
				ctx.BeginPatternMatch(filters.ToArray(), args.Select(x=>x.EvaluatedArgs).ToArray(), patternMatch.Alternate != null);
				for (var i = 0; i < patternMatch.Branches.Count; i++)
				{
					var branch = patternMatch.Branches[i];
					ctx.EnterArm(i);
					foreach (var state in Evaluate(branch.Body,depth+1, ctx)) yield return state;
					ctx.ExitArm(branch.CloseType);
				}
				if (patternMatch.Alternate != null)
				{
					ctx.EnterArm(patternMatch.Branches.Count);
					foreach (var state in Evaluate(patternMatch.Alternate.Body,depth+1, ctx)) yield return state;
					ctx.ExitArm(patternMatch.Alternate.CloseType);
				}
				ctx.EndPatternMatch();
				yield return EvalState.Running(depth, patternMatch.Span);
				break;
			// default:
			// 	throw new NotImplementedException($"{node.GetType()} not yet compilable");
		}
	}

	
	private static object[] EvaluateArguments(Arguments args, Context ctx)
	{
		return args.EvaluatedArgs;
	}

	public static EvalCursor CreateCursor(PKNode root)
	{
		//is there a faster way to do this? do we even need a queue?
		var remaining = new Queue<PKNode>();
		foreach (var childNode in root.GetChildren())
		{
			remaining.Enqueue(childNode);
		}
		
		var cursor = new EvalCursor()
		{
			Frames = new Stack<(Queue<PKNode> Remaining, int Depth)>(){}
		};
		cursor.Frames.Push((remaining, 0));
		
		return cursor;
	}

	public static EvalState StepOnce(EvalCursor cursor, Context ctx)
	{
	    //pop empty frames until we find work or run out
	    while (cursor.Frames.Count > 0 && cursor.Frames.Peek().Remaining.Count == 0)
	    {
		    cursor.Frames.Pop();
	    }

	    if (cursor.Frames.Count == 0)
	    {
		    return EvalState.Complete(0, default);
	    }

	    var (queue, depth) = cursor.Frames.Peek();
	    var node = queue.Dequeue();

	    try
	    {
	        switch (node)
	        {
		        //branch closer that is a 'real' node, not side-effect only.
	            case PKScopeStateNode closer:
	                return closer.Execute();
	            //enter arm/exit arm. Not a 'real' node.
				case PKScopeCloserNode closer: 
					closer.Execute();
					return StepOnce(cursor, ctx);
	            case PKNodeGroup group:
	                //transparent — push children and immediately recurse
	                cursor.Frames.Push((new Queue<PKNode>(group.Nodes), depth + 1));
	                return StepOnce(cursor, ctx);

	            case PKInputBranch branch:
	                //push: input node, then body, then closer that calls PopFrame
	                var ibQueue = new Queue<PKNode>();
	                ibQueue.Enqueue(branch.Input);
	                foreach (var n in branch.Body.Nodes) ibQueue.Enqueue(n);
	                ibQueue.Enqueue(new PKScopeStateNode(() =>
	                {
	                    ctx.PopFrame(branch.BranchType);
	                    return EvalState.Running(depth, branch.Span);
	                }));
	                cursor.Frames.Push((ibQueue, depth + 1));
	                return StepOnce(cursor, ctx);

	            case PKNamedBranch namedBranch:
	                ctx.NewNamedFrame(namedBranch.Label);
	                var nbQueue = new Queue<PKNode>(namedBranch.Body.Nodes);
	                //closer
	                nbQueue.Enqueue(new PKScopeStateNode(() =>
	                {
	                    ctx.PopFrame(namedBranch.Type);
	                    return EvalState.Running(depth, namedBranch.Span);
	                }));
	                cursor.Frames.Push((nbQueue, depth + 1));
	                return StepOnce(cursor, ctx);

	            case PKBranch branch:
	                ctx.NewFrame();
	                var bQueue = new Queue<PKNode>(branch.Body.Nodes);
	                //closer
	                bQueue.Enqueue(new PKScopeStateNode(() =>
	                {
	                    ctx.PopFrame(branch.Type);
	                    return EvalState.Running(depth, branch.Span);
	                }));
	                cursor.Frames.Push((bQueue, depth + 1));
	                return StepOnce(cursor, ctx);

	            case PKPatternMatch pm:
		            var filters = pm.Branches.Select(x => x.Filter).ToArray();
		            var args = pm.Branches.Select(x => x.Arguments.EvaluatedArgs).ToArray();
		            ctx.BeginPatternMatch(filters, args, pm.Alternate != null);
		            var pmQueue = new Queue<PKNode>();
		            for (var i = 0; i < pm.Branches.Count; i++)
		            {
			            var arm = pm.Branches[i];
			            var armIndex = i;
			            pmQueue.Enqueue(new PKScopeCloserNode(() => ctx.EnterArm(armIndex))); // side-effect only
			            foreach (var n in arm.Body.Nodes) pmQueue.Enqueue(n);
			            pmQueue.Enqueue(new PKScopeCloserNode(() => ctx.ExitArm(arm.CloseType))); // side-effect only
		            }

		            if (pm.Alternate != null)
		            {
			            var alt = pm.Alternate;
			            var altIndex = pm.Branches.Count;
			            pmQueue.Enqueue(new PKScopeCloserNode(() => ctx.EnterArm(altIndex))); // side-effect only
			            foreach (var n in alt.Body.Nodes) pmQueue.Enqueue(n);
			            pmQueue.Enqueue(new PKScopeCloserNode(() => ctx.ExitArm(alt.CloseType))); // side-effect only
		            }
					//closer
		            pmQueue.Enqueue(new PKScopeStateNode(() => 
		            {
			            ctx.EndPatternMatch();
			            return EvalState.Running(depth, pm.Span);
		            }));
		            cursor.Frames.Push((pmQueue, depth + 1));
		            return StepOnce(cursor, ctx);


	            // Leaf nodes
	            case PKGenInputProvider input:
	                ctx.PushStreamWithGenerator(input.Type, input.Arguments.EvaluatedArgs, input.Generator);
	                return EvalState.Running(depth, input.Span);
	            case PKPipeInputProvider input:
	                ctx.PushStreamWithPipeGenerator(input.Type, input.Arguments.EvaluatedArgs, input.PipeGenerator);
	                return EvalState.Running(depth, input.Span);
	            case PKFilterOperatorNode fopr:
	                ctx.FilterOnEach(fopr.Arguments.EvaluatedArgs, fopr.Invoker);
	                return EvalState.Running(depth, fopr.Span);
	            case PKSignalOperatorNode sopr:
	                ctx.SignalOnEach(sopr.Arguments.EvaluatedArgs, sopr.Invoker);
	                return EvalState.Running(depth, sopr.Span);
	            case PKInlineOperatorNode iopr:
	                ctx.OperateOnEach(iopr.Arguments.EvaluatedArgs, iopr.Invoker);
	                return EvalState.Running(depth, iopr.Span);
	            case PKPack pack:
	                ctx.Pack();
	                return EvalState.Running(depth, pack.Span);
	            case PKUnpack unpack:
	                ctx.Unpack();
	                return EvalState.Running(depth, unpack.Span);

	            default:
		            throw new Exception("Unknown node type");
	                return EvalState.Running(depth, node.Span); //unknown node — skip
	        }
	    }
	    catch (Exception)
	    {
	        return EvalState.Error(depth, node.Span);
	    }
	}
}