using System.Diagnostics;
using PocketKnife.Compiler;
using PocketknifeCore.Errors;

namespace PocketknifeCore.Compiler;

public class Compiler
{
	private OpCatalog _catalog;

	//we don't have a root "program" node because we're going to support a REPL.
	public Compiler(OpCatalog catalog)
	{
		_catalog = catalog;
	}

	public PKNode StartCompile(ASTNode node)
	{
		CompileContext ctx = new CompileContext();
		return Compile(node, ctx);
	}

	public PKNode Compile(ASTNode node, CompileContext ctx)
	{
		switch (node)
		{
			case ScriptNode scriptNode:
				List<PKNode> nodes = new();
				foreach (var rootNode in scriptNode.RootNodes)
				{
					var n = Compile(rootNode, ctx);
					nodes.Add(n);
				}

				return new PKNodeGroup(nodes);
			case CommandSetNode commandSetNode:
			{
				var commands = commandSetNode.Commands;

				// Fast path: scan once, bail out cheaply if no boundaries.
				List<int>? boundaries = null;
				for (int i = 0; i < commands.Count; i++)
				{
					if (commands[i].IsBoundary)
					{
						boundaries ??= new List<int>(4);
						boundaries.Add(i);
					}
				}

				if (boundaries is null)
				{
					var compiled = new List<PKNode>(commands.Count);
					foreach (var c in commands)
					{
						compiled.Add(Compile(c, ctx));
					}
					return new PKNodeGroup(compiled);
				}

				//split into segments around boundaries.
				var groups = new List<PKNode>(boundaries.Count + 1);
				int start = 0;
				for (int b = 0; b <= boundaries.Count; b++)
				{
					int end = b < boundaries.Count ? boundaries[b] : commands.Count;
					var segment = new List<PKNode>(end - start);
					for (int k = start; k < end; k++)
					{
						segment.Add(Compile(commands[k], ctx));
					}
					groups.Add(new PKNodeGroup(segment));
					start = end;
				}

				return new PKNodeGroup(groups);
			}
			case InputBranchNode inputBranchNode:
				var ic = (PKInputProvider)Compile(inputBranchNode.Input, ctx);
				Debug.Assert(!PKType.IsNone(ctx.StackTop));
				var body = (PKNodeGroup)Compile(inputBranchNode.CommandSet, ctx);
				return new PKInputBranch(ic, body, inputBranchNode.BranchType);
				
			case InputLiteralProviderNode inputLiteralProviderNode:
				//
				if (inputLiteralProviderNode.Arguments.Length == 0)
				{
					throw new CompilerException(inputLiteralProviderNode.Start, "no arguments to literal provider");
				}
				else
				{
					List<object> literals = new List<object>(inputLiteralProviderNode.Arguments.Length);
					Type argKind = PKType.None;
					for (var i = 0; i < inputLiteralProviderNode.Arguments.Length; i++)
					{
						var arg = inputLiteralProviderNode.Arguments[i];
						var resArg = EvaluateExpression(arg, ctx);
						if (i == 0)
						{
							argKind = resArg.GetPKType();
							literals.Add(resArg);
						}
						else
						{
							if (resArg.GetPKType() == argKind)
							{
								literals.Add(resArg);
							}
							else
							{
								throw new CompilerException(inputLiteralProviderNode.Arguments[i].Start, $"all arguments must be of the same type. {argKind} != {resArg.GetPKType()}");
							}
						}
					}
					
					ctx.PushType(argKind);
					return new PKGenInputProvider(argKind, inputLiteralProviderNode.Name, Arguments.Empty, (args, context) => literals);

				}
			case PipeInInputProviderNode pipeInInputProviderNode:
				Debug.Assert(pipeInInputProviderNode.sigil == "|>");
				if (_catalog.TryGetOp(pipeInInputProviderNode.Name, out var piopr))
				{
					// Build (or fetch a cached) OpInvoker for this overload.
					//todo: casting!
					PipeGenInvoker opInvoker = piopr.GetOrBuildPipeGenerator(ctx.StackTop, out var call);
					var args = CompileArguments(call, pipeInInputProviderNode.Arguments, ctx);
					var callType = call.OutType.Lower();
					ctx.PushType(callType);
					return new PKPipeInputProvider(callType, piopr.Name, opInvoker, args);
				}
				else
				{
					throw new CompilerException(pipeInInputProviderNode.Start, $"unknown operator {pipeInInputProviderNode.Name}");
				}
				break;
			case InputProviderNode inputProviderNode:
				Debug.Assert(inputProviderNode.sigil == ">");
				if (_catalog.TryGetOp(inputProviderNode.Name, out var iopr))
				{
					// Build (or fetch a cached) OpInvoker for this overload.
					GenInvoker genInvoker = iopr.GetOrBuildGenerator(out var call);
					var args = CompileArguments(call, inputProviderNode.Arguments, ctx);
					//generator out type should be list<x>, but really, it's list. we are doing the following commands on every one.
					var callType = call.OutType.Lower();
					ctx.PushType(callType);
					return new PKGenInputProvider(callType, iopr.Name, args, genInvoker);
				}
				else
				{
					throw new CompilerException(inputProviderNode.Start, $"unknown operator {inputProviderNode.Name}");
				}
			case BranchNode branchNode:
				if (branchNode.Type == BranchType.Unknown)
				{
					throw new NotImplementedException();
				}
				
				if (string.IsNullOrEmpty(branchNode.Label))
				{
					ctx.PushFrame();
					var subbranch = (PKNodeGroup)Compile(branchNode.Commands, ctx);
					ctx.PopFrame(branchNode.Type);
					return new PKBranch(subbranch, branchNode.Type);
				}
				else
				{
					ctx.PushFrame();
					//read variable from within itself? then we assign here, and change after the subbranch compiles. idk.
					var subbranch = (PKNodeGroup)Compile(branchNode.Commands, ctx);
					ctx.PopFrame(branchNode.Type);
					//ctx.ChangeVariableType(branchNode.Label, 0, ctx.StackTop);//after transformations, we have the actual variable type.
					//wait, 
					ctx.AssignNewVariable(branchNode.Label, ctx.StackTop);

					return new PKNamedBranch(branchNode.Label, subbranch, branchNode.Type);
				}
			case PipelineCommandNode pipelineNode:
				Debug.Assert(pipelineNode.sigil == "|");
				if (_catalog.TryGetOp(pipelineNode.Name, out var popr))
				{
					if (popr.HasOp(ctx.StackTop))
					{
						OpInvoker invoker = popr.GetOrBuildInvoker(ctx.StackTop, out var call);
						var args = CompileArguments(call, pipelineNode.Arguments, ctx);
						ctx.TransformType(call.OutType);
						return new PKInlineOperatorNode(popr.Name, invoker, args);
					}
					
					//check if any of our casts are useful for this operator... they are not sorted, but we presume not a big deal for now!
					foreach (var cast in _catalog.GetImplicitCasts(ctx.StackTop))
					{
						if (popr.HasOp(cast.OutType))
						{
							OpInvoker invoker = popr.GetOrBuildInvoker(ctx.StackTop, cast, out var call);
							var args = CompileArguments(call, pipelineNode.Arguments, ctx);
							ctx.TransformType(cast.OutType);
							return new PKInlineOperatorNode(popr.Name, invoker, args);
						}//else continue
					}
					
					if (popr.HasOp(PKType.Any))
					{
						OpInvoker invoker = popr.GetOrBuildInvoker(ctx.StackTop, out var call);
						var args = CompileArguments(call, pipelineNode.Arguments, ctx);
						ctx.TransformType(call.OutType);
						return new PKInlineOperatorNode(popr.Name, invoker, args);
					}
					
					throw new CompilerException(pipelineNode.Start, $"operator {popr.Name} does not have an overload for incoming type {ctx.StackTop}");
					
				}
				else
				{
					throw new CompilerException(pipelineNode.Start, $"unknown | operator {pipelineNode.Name}");
				}
			case SignalCommandNode pipelineNode:
				Debug.Assert(pipelineNode.sigil == ":");
				if (_catalog.TryGetOp(pipelineNode.Name, out var sopr))
				{
					//:'s can take the incoming type, or they can just ignore the stack completely.
					if (sopr.HasOp(ctx.StackTop))
					{
						OpInvoker invoker = sopr.GetOrBuildInvoker(ctx.StackTop, out var call);
						var args = CompileArguments(call, pipelineNode.Arguments, ctx);
						return new PKInlineOperatorNode(sopr.Name, invoker, args);
					}
					
					//check if any of our casts are useful for this operator... they are not sorted, but we presume not a big deal for now!
					foreach (var cast in _catalog.GetImplicitCasts(ctx.StackTop))
					{
						if (sopr.HasOp(cast.OutType))
						{
							OpInvoker invoker = sopr.GetOrBuildInvoker(ctx.StackTop, cast, out var call);
							var args = CompileArguments(call, pipelineNode.Arguments, ctx);
							return new PKInlineOperatorNode(sopr.Name, invoker, args);
						} //else continue
					}
					
					if (sopr.HasOp(PKType.Any))
					{
						OpInvoker invoker = sopr.GetOrBuildInvoker(PKType.Any, out var call);
						var args = CompileArguments(call, pipelineNode.Arguments, ctx);
						return new PKInlineOperatorNode(sopr.Name, invoker, args);
					}
					else if (sopr.HasOp(PKType.None))
					{
						OpInvoker invoker = sopr.GetOrBuildInvoker(PKType.None, out var call);
						var args = CompileArguments(call, pipelineNode.Arguments, ctx);
						return new PKInlineOperatorNode(sopr.Name, invoker, args);
					}
					else
					{
						throw new CompilerException(pipelineNode.Start, $"operator {sopr.Name} does not have an overload for incoming type {ctx.StackTop}");
					}
				}
				throw new CompilerException(pipelineNode.Start, $"unknown : operator {pipelineNode.Name}");
			case DefaultFilterCommandNode defaultFilterNode:
				Debug.Assert(defaultFilterNode.sigil == "~~");
				return new PKFilterOperatorNode("~~", ((input, args, context) => true), Arguments.Empty);
				break;
			case FilterCommandNode filterNode:
				Debug.Assert(filterNode.sigil == "~");
				if(_catalog.TryGetOp(filterNode.Name, out var fopr))
				{
					if (fopr.HasOp(ctx.StackTop))
					{
						OpInvoker invoker = fopr.GetOrBuildInvoker(ctx.StackTop, out var call);
						var args = CompileArguments(call, filterNode.Arguments, ctx);
						return new PKFilterOperatorNode(fopr.Name, invoker, args);
					}

					foreach (var cast in _catalog.GetImplicitCasts(ctx.StackTop))
					{
						OpInvoker invoker = fopr.GetOrBuildInvoker(ctx.StackTop, cast, out var call);
						var args = CompileArguments(call, filterNode.Arguments, ctx);
						return new PKFilterOperatorNode(fopr.Name, invoker, args);
					}
					
					if (fopr.HasOp(PKType.Any))
					{
						OpInvoker invoker = fopr.GetOrBuildInvoker(ctx.StackTop, out var call);
						var args = CompileArguments(call, filterNode.Arguments, ctx);
						return new PKFilterOperatorNode(fopr.Name, invoker, args);
					}
					else
					{
						throw new CompilerException(filterNode.Start, $"operator {fopr.Name} does not have an overload for incoming type {ctx.StackTop}");
					}
					
				}

				throw new CompilerException(filterNode.Start, $"unknown ~ operator {filterNode.Name}");

			case PackListNode:
				ctx.Pack();
				return new PKPack();
			case UnpackListNode:
				ctx.Unpack();
				return new PKUnpack();
			case NakedPatternMatch nakedPatternMatch:
				var arms = new List<PKPatternFilterMatchBranch>(nakedPatternMatch.Arms.Count);
				PKPatternBranch? defaultArm = null;
				// All arms see the same input type (the ?-input) and may produce different
				// output types. Save the input type so we can restore it before compiling
				// each subsequent arm, and collect arm output types to widen the post-?
				// top type when arms disagree.
				var inputType = ctx.StackTop;
				Type? unifiedOut = null;
				bool first = true;
				foreach (var patternBranchArm in nakedPatternMatch.Arms)
				{
					if (!first)
					{
						// Reset stack top to the ? input before compiling this arm.
						ctx.TransformType(inputType);
					}
					first = false;
					if (patternBranchArm.IsDefault)
					{
						defaultArm = (PKPatternBranch)Compile(patternBranchArm, ctx);
					}
					else
					{
						//naked ? means ~ arms. (unless syntactic sugar stuff.)
						arms.Add((PKPatternFilterMatchBranch)Compile(patternBranchArm, ctx));
					}
					var armOut = ctx.StackTop;
					if (unifiedOut == null)
					{
						unifiedOut = armOut;
					}
					else if (unifiedOut != armOut)
					{
						unifiedOut = PKType.Any;
						//consider throwing an exception, but this would break ~~ and ~drop, which the compiler should not worry about.
						//not sure the best way to deal with it, but any is a good enough solution for now; I don't have (a|b) types.
					}
				}

				// Final stack top is the unified arm output (or the input type if there were no arms).
				ctx.TransformType(unifiedOut ?? inputType);

				if (nakedPatternMatch.CloseType != BranchType.SideEffect)
				{
					throw new CompilerException(nakedPatternMatch.Start, "pattern match (?+) must have ^ closer. (it's not a real branch, it can't &append or <replace because it doesn't branch away to begin with).");
				}
				return new PKPatternMatch(arms, defaultArm, nakedPatternMatch.CloseType);
			// case PatternExpressionMatch patternExpressionMatchNode:
			// 	//compile the branches but we expect expressions, not filters.
			// 	throw new NotImplementedException();
			case PatternBranchArm branchArm:
				// Arms run on parallel copies of the ? input — their type changes must
				// not leak across sibling arms or out of the pattern match.
				var armCloseType = branchArm.CloseType == BranchType.Unknown ? BranchType.Replace : branchArm.CloseType;
				ctx.PushFrame();
				PKFilterOperatorNode? filter = null;
				if (branchArm.FilterToMatch != null && !branchArm.IsDefault)
				{
					filter = (PKFilterOperatorNode)Compile(branchArm.FilterToMatch,ctx);
				}
				var armBody = (PKNodeGroup)Compile(branchArm.Commands,ctx);
				// Bubble the arm's output type up so NakedPatternMatch can collect it.
				ctx.PopFrame(BranchType.Replace);
				
				if (filter == null)
				{
					return new PKPatternBranch(armBody, armCloseType);
				}
				else
				{
					return new PKPatternFilterMatchBranch(filter.Invoker, filter.Arguments, armBody, armCloseType);
				}
				
				break;
			default:
				throw new NotImplementedException($"{node.GetType()} not yet compilable");
		}
	}

	private Arguments CompileArguments(OperatorDescription overload, ExpressionNode[] arguments, CompileContext ctx)
	{
		var a = overload.FirstArgIsStream() ? 1 : 0;
		Debug.Assert(overload.Method.GetParameters().Length == arguments.Length + a);
		if (arguments.Length == 0)
		{
			return Arguments.Empty;
		}
		
		List<object> args = new List<object>(arguments.Length);
		for (var i = 0; i < arguments.Length; i++)
		{
			var arg = arguments[i];
			var paramType = overload.Method.GetParameters()[i + a].ParameterType;
			var PKParamType = paramType;
			
			var e = EvaluateExpression(arg, ctx);
			// VarRef args are resolved per-item at runtime; skip compile-time type check.
			bool addedWithCast = false;
			if (e is VarRef evr)
			{
				//todo: we need to check casting overloads.
				if (evr.Type != PKParamType && PKParamType != PKType.Any)
				{
					foreach (var cast in _catalog.GetImplicitCasts(evr.Type))
					{
						if (cast.OutType == PKParamType)
						{
							//GetOrBuildInvoker exists inside of OperatorResolver... which arguments don't have...
							
							//OpInvoker castInvoker = GetOrBuildInvoker(ctx.StackTop, cast, out var call);
							//replace e with e+cast.
							args.Add(new VarRef(evr.Name, evr.ReachOut,cast.OutType, cast));
							addedWithCast = true;
							break;
						}
					}
				}

				if (!addedWithCast)
				{
					args.Add(e);
				}
				continue;
			}
			
			var etype = e.GetType();
			if (etype != PKParamType && PKParamType != PKType.Any)
			{
				foreach (var cast in _catalog.GetImplicitCasts(etype))
				{
					if (cast.OutType == PKParamType)
					{
						e = cast.ApplyNow(e);
						goto WithCorrectType;
					}
				}
				
				throw new CompilerException(arguments[i].Start, $"Invalid Type for {etype} when expected {PKParamType} for parameter {i} of {overload.Method.Name}");
				
			}
			WithCorrectType:
			//check if it's an ArgumentEvalNode or a const i guess.
			args.Add(e);
		}
		return new Arguments(args.ToArray());
	}

	private object EvaluateExpression(ExpressionNode literal, CompileContext ctx)
	{
		switch (literal)
		{
			case LiteralExpressionNode literalNode:
				return literalNode.Value;
			case LabelNode labelNode:
				var t = ctx.GetVariableType(labelNode.Name, labelNode.ReachOut);
				// `@name` / `@^name` reference — resolved per-item at runtime by Context.
				//todo: we don't know the type, because we aren't saving it on branch openings.
				return new VarRef(labelNode.Name, labelNode.ReachOut, t);
			// case EmptyListLiteralExpression literalExpressionNode:
			// 	return literalExpressionNode.Value;
			default:
				throw new NotImplementedException($"{literal.GetType()} not yet compilable");
		}
	}
}