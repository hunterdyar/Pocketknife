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
				yield return EvalState.Good(depth, input.Span);
				break;
			case PKPipeInputProvider input:
				var pia = EvaluateArguments(input.Arguments, ctx);
				ctx.PushStreamWithPipeGenerator(input.Type, pia, input.PipeGenerator);
				yield return EvalState.Good(depth, input.Span);
				break;
			case PKFilterOperatorNode fopr:
				var fa = EvaluateArguments(fopr.Arguments, ctx);
				ctx.FilterOnEach(fa, fopr.Invoker);
				yield return EvalState.Good(depth, fopr.Span);
				break;
			case PKSignalOperatorNode sopr:
				var soprArguments = EvaluateArguments(sopr.Arguments, ctx);
				ctx.SignalOnEach(soprArguments, sopr.Invoker);
				yield return EvalState.Good(depth, sopr.Span);
				break;
			case PKInlineOperatorNode iopr:
				var ioprArguments = EvaluateArguments(iopr.Arguments, ctx);
				ctx.OperateOnEach(ioprArguments, iopr.Invoker);
				yield return EvalState.Good(depth, iopr.Span);
				break;
			case PKPack pack:
				ctx.Pack();
				yield return EvalState.Good(depth, pack.Span);
				break;
			case PKUnpack unpack:
				ctx.Unpack();
				yield return EvalState.Good(depth, unpack.Span);
				break;
			case PKNamedBranch namedBranch:
				ctx.NewNamedFrame(namedBranch.Label);
				//yield return EvalState.Good(depth, namedBranch.Span);
				foreach (var state in Evaluate(namedBranch.Body, depth + 1, ctx)) yield return state;
				ctx.PopFrame(namedBranch.Type);
				yield return EvalState.Good(depth, namedBranch.Span);
				break;
			case PKBranch branch:
				ctx.NewFrame();
				//yield return EvalState.Good(depth, branch.Span);
				foreach (var state in Evaluate(branch.Body, depth + 1, ctx)) yield return state;
				ctx.PopFrame(branch.Type);
				yield return EvalState.Good(depth, branch.Span);
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
				yield return EvalState.Good(depth, patternMatch.Span);
				break;
			// default:
			// 	throw new NotImplementedException($"{node.GetType()} not yet compilable");
		}
	}

	private static object[] EvaluateArguments(Arguments args, Context ctx)
	{
		return args.EvaluatedArgs;
	}
}