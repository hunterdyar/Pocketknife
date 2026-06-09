using System.Diagnostics;

namespace PocketknifeCore.SimpleEvaluator;

public static class SimpleEvaluator
{
	public static void EvaluateAll(PKNode node, Context ctx = null)
	{
		int stepCount = 0;
		foreach (var state in Evaluate(node, ctx))
		{
			if (state.IsErr)
			{
				return;
			}
			stepCount++;
		}
	}
	public static IEnumerable<EvalState> Evaluate(PKNode node, Context ctx = null)
	{
		switch (node)
		{
			case PKNodeGroup group:
				foreach (var n in group.Nodes)
				{
					foreach (var state in Evaluate(n, ctx))
					{
						yield return state;
					}
				}
				break;
			case PKInputBranch branch:
				//push the input stream onto the stack.
				foreach (var evalState in Evaluate(branch.Input, ctx)) yield return evalState;
				//take that and operate on it
				foreach (var evalState in Evaluate(branch.Body, ctx)) yield return evalState;
				break;
			case PKInputProvider input:
				var ia = EvaluateArguments(input.Arguments, ctx);
				var value = input.Generator.Invoke(ia, ctx);
				ctx.PushStream(input.Type, value);
				yield return EvalState.Good();
				break;
			case PKFilterOperatorNode fopr:
				var fa = EvaluateArguments(fopr.Arguments, ctx);
				ctx.FilterOnEach(fa, fopr.Invoker);
				yield return EvalState.Good();
				break;
			case PKInlineOperatorNode iopr:
				var ioprArguments = EvaluateArguments(iopr.Arguments, ctx);
				ctx.OperateOnEach(ioprArguments, iopr.Invoker);
				yield return EvalState.Good();
				break;
			case PKPack:
				ctx.Pack();
				yield return EvalState.Good();
				break;
			case PKUnpack:
				ctx.Unpack();
				yield return EvalState.Good();
				break;
			case PKNamedBranch namedBranch:
				ctx.NewNamedFrame(namedBranch.Label);
				yield return EvalState.Good();
				foreach (var state in Evaluate(namedBranch.Body, ctx)) yield return state;
				ctx.PopFrame(namedBranch.Type);
				yield return EvalState.Good();
				break;
			case PKBranch branch:
				ctx.NewFrame();
				yield return EvalState.Good();
				foreach (var state in Evaluate(branch.Body, ctx)) yield return state;
				ctx.PopFrame(branch.Type);
				yield return EvalState.Good();
				break;
			// default:
			// 	throw new NotImplementedException($"{node.GetType()} not yet compilable");
		}
	}

	private static object[] EvaluateArguments(Arguments args, Context ctx)
	{
		//todo: check for variables, etc.

		return args.EvaluatedArgs;
	}
}

public struct EvalState
{
	public bool IsErr = false;

	public EvalState()
	{
	}

	public static EvalState Good()
	{
		return new EvalState();
	}

	public static EvalState Bad()
	{
		return new EvalState()
		{
			IsErr = true
		};
	}
}