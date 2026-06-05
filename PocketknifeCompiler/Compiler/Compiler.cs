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
				return new PKNodeGroup(commandSetNode.Commands.Select(n => Compile(n, ctx)).ToList());
			case InputBranchNode inputBranchNode:
				var inputCommand = (PKInputProvider)Compile(inputBranchNode.Input, ctx);
				Debug.Assert(ctx.StackTop != PKKind.None);
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
					PKKind argKind = PKKind.None;
					for (var i = 0; i < inputLiteralProviderNode.Arguments.Length; i++)
					{
						var arg = inputLiteralProviderNode.Arguments[i];
						var resArg = EvaluateExpression(arg, ctx);
						if (i == 0)
						{
							argKind = resArg.Kind;
							literals.Add(resArg);
						}
						else
						{
							if (resArg.Kind == argKind)
							{
								literals.Add(resArg);
							}
							else
							{
								throw new Exception($"all arguments must be of the same type. {argKind} != {resArg.Kind}");
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
			case PipelineCommandNode pipelineNode:
				Debug.Assert(pipelineNode.sigil == "|");
				if (_catalog.TryGetOp(pipelineNode.Name, out var popr))
				{
					OpInvoker invoker = popr.GetOrBuildInvoker(ctx.StackTop, out var call);
					ctx.PushType(call.OutType);
					return new InlineOperatorNode(popr.Name, invoker);
				}
				else
				{
					throw new Exception($"unknown operator {pipelineNode.Name}");
				}
			case SignalCommandNode pipelineNode:
				Debug.Assert(pipelineNode.sigil == ":");
				if (_catalog.TryGetOp(pipelineNode.Name, out var sopr))
				{
					//:'s can take the incoming type, or they can just ignore the stack completely.
					if (sopr.HasOp(ctx.StackTop))
					{
						OpInvoker invoker = sopr.GetOrBuildInvoker(ctx.StackTop, out var call);
						return new InlineOperatorNode(sopr.Name, invoker);
					}else if (sopr.HasOp(PKKind.None))
					{
						OpInvoker invoker = sopr.GetOrBuildInvoker(PKKind.None, out var call);
						return new InlineOperatorNode(sopr.Name, invoker);
					}
				}
				else
				{
					throw new Exception($"unknown operator {pipelineNode.Name}");
				}
				break;
			default:
				throw new NotImplementedException($"{node.GetType()} not yet compilable");
		}
		
		throw new NotImplementedException();
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