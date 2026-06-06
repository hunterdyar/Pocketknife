using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using PocketKnife.Compiler;

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
				var inputCommand = (PKInputProvider)Compile(inputBranchNode.Input, ctx);
				Debug.Assert(!PKType.IsNone(ctx.StackTop));
				var body = (PKNodeGroup)Compile(inputBranchNode.CommandSet, ctx);
				return new PKInputBranch(inputCommand, body);
			case InputLiteralProviderNode inputLiteralProviderNode:
				//
				if (inputLiteralProviderNode.Arguments.Length == 0)
				{
					throw new NotImplementedException();
				}
				else
				{
					List<PKValue> literals = new List<PKValue>(inputLiteralProviderNode.Arguments.Length);
					PKType argKind = PKType.None;
					for (var i = 0; i < inputLiteralProviderNode.Arguments.Length; i++)
					{
						var arg = inputLiteralProviderNode.Arguments[i];
						var resArg = EvaluateExpression(arg, ctx);
						if (i == 0)
						{
							argKind = resArg.Type;
							literals.Add(resArg);
						}
						else
						{
							if (resArg.Type == argKind)
							{
								literals.Add(resArg);
							}
							else
							{
								throw new Exception($"all arguments must be of the same type. {argKind} != {resArg.Type}");
							}
						}

					}

					ctx.PushType(argKind);
					return new PKInputProvider(argKind, inputLiteralProviderNode.Name, (args, context) => literals);

				}
				break;
			case InputProviderNode inputProviderNode:
				Debug.Assert(inputProviderNode.sigil == ">");
				if (_catalog.TryGetOp(inputProviderNode.Name, out var iopr))
				{
					// Build (or fetch a cached) OpInvoker for this overload.
					GenInvoker genInvoker = iopr.GetOrBuildGenerator(out var call);

					ctx.PushType(call.OutType);
					return new PKInputProvider(call.OutType, iopr.Name, genInvoker);
				}
				else
				{
					throw new Exception($"unknown operator {inputProviderNode.Name}");
				}
			case BranchNode branchNode:
				if (branchNode.Type == BranchType.Unknown)
				{
					throw new NotImplementedException();
				}
				
				if (string.IsNullOrEmpty(branchNode.Label))
				{
					var subbranch = (PKNodeGroup)Compile(branchNode.Commands, ctx);
					return new PKBranch(subbranch, branchNode.Type);
				}
				else
				{
					var subbranch = (PKNodeGroup)Compile(branchNode.Commands, ctx);
					return new PKNamedBranch(branchNode.Label, subbranch, branchNode.Type);
				}
			case PipelineCommandNode pipelineNode:
				Debug.Assert(pipelineNode.sigil == "|");
				if (_catalog.TryGetOp(pipelineNode.Name, out var popr))
				{
					if (popr.HasOp(ctx.StackTop))
					{
						OpInvoker invoker = popr.GetOrBuildInvoker(ctx.StackTop, out var call);
						ctx.PushType(call.OutType);
						return new PKInlineOperatorNode(popr.Name, invoker);
					}else if (popr.HasOp(PKType.Any))
					{
						OpInvoker invoker = popr.GetOrBuildInvoker(ctx.StackTop, out var call);
						ctx.PushType(call.OutType);
						return new PKInlineOperatorNode(popr.Name, invoker);
					}
					else
					{
						throw new Exception($"operator {popr.Name} does not have an overload for incoming type {ctx.StackTop}");
					}
					
				}
				else
				{
					throw new Exception($"unknown | operator {pipelineNode.Name}");
				}
			case SignalCommandNode pipelineNode:
				Debug.Assert(pipelineNode.sigil == ":");
				if (_catalog.TryGetOp(pipelineNode.Name, out var sopr))
				{
					//:'s can take the incoming type, or they can just ignore the stack completely.
					if (sopr.HasOp(ctx.StackTop))
					{
						OpInvoker invoker = sopr.GetOrBuildInvoker(ctx.StackTop, out var call);
						return new PKInlineOperatorNode(sopr.Name, invoker);
					}else if (sopr.HasOp(PKType.Any))
					{
						OpInvoker invoker = sopr.GetOrBuildInvoker(PKType.Any, out var call);
						return new PKInlineOperatorNode(sopr.Name, invoker);
					}
					else if (sopr.HasOp(PKType.None))
					{
						OpInvoker invoker = sopr.GetOrBuildInvoker(PKType.None, out var call);
						return new PKInlineOperatorNode(sopr.Name, invoker);
					}
					else
					{
						throw new Exception($"operator {sopr.Name} does not have an overload for incoming type {ctx.StackTop}");
					}
				}
				throw new Exception($"unknown : operator {pipelineNode.Name}");
			case FilterCommandNode filterNode:
				Debug.Assert(filterNode.sigil == "~");
				if(_catalog.TryGetOp(filterNode.Name, out var fopr))
				{
					if (fopr.HasOp(ctx.StackTop))
					{
						OpInvoker invoker = fopr.GetOrBuildInvoker(ctx.StackTop, out var call);
						ctx.PushType(call.OutType);
						return new PKFilterOperatorNode(fopr.Name, invoker);
					}
					else if (fopr.HasOp(PKType.Any))
					{
						OpInvoker invoker = fopr.GetOrBuildInvoker(ctx.StackTop, out var call);
						ctx.PushType(call.OutType);
						return new PKFilterOperatorNode(fopr.Name, invoker);
					}
					else
					{
						throw new Exception($"operator {fopr.Name} does not have an overload for incoming type {ctx.StackTop}");
					}
					
				}

				throw new Exception($"unknown ~ operator {filterNode.Name}");

			case PackListNode:
				ctx.Pack();
				return new PKPack();
			case UnpackListNode:
				ctx.Unpack();
				return new PKUnpack();
			case NakedPatternMatch nakedPatternMatch:
				throw new NotImplementedException();
			case PatternExpressionMatch patternExpressionMatchNode:
				throw new NotImplementedException();
			case PatternMatch patternMatchNode:
				throw new NotImplementedException();
			case PatternBranchArm patternNode:
				throw new NotImplementedException();
			
			default:
				throw new NotImplementedException($"{node.GetType()} not yet compilable");
		}
	}

	private PKValue EvaluateExpression(ExpressionNode literal, CompileContext ctx)
	{
		switch (literal)
		{
			case LiteralExpressionNode literalNode:
				return literalNode.Value;
			// case EmptyListLiteralExpression literalExpressionNode:
			// 	return literalExpressionNode.Value;
			default:
				throw new NotImplementedException($"{literal.GetType()} not yet compilable");
		}
	}
}