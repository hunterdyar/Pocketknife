using System.Diagnostics;
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
	//what i previously called a 'plugin environment' - the static list of all available operators and OS and current path. So the runtime environment.

	public PKNode Compile(ASTNode node)
	{
		switch (node)
		{
			case ScriptNode scriptNode:
				List<PKNode> nodes = new();
				foreach (var rootNode in scriptNode.RootNodes)
				{
					var n = Compile(rootNode);
					nodes.Add(n);
				}
				return new PKGroupNode(nodes);
			case InputBranchNode inputBranchNode:
				List<PKNode> inputBranchCommands = new();
				var inputCommand = Compile(inputBranchNode.Input);
					
				foreach (var rootNode in inputBranchNode.CommandSet.Commands)
				{
					var n = Compile(rootNode);
					inputBranchCommands.Add(n);
				}
				return new PKGroupNode(inputBranchCommands);
			case InputLiteralProviderNode inputLiteralProviderNode:
				//
				break;
			case InputProviderNode inputProviderNode:
				Debug.Assert(inputProviderNode.sigil == ">");
				if (_catalog.TryGetOp(inputProviderNode.Name, out var op))
				{
					var ins = op.GetTypes().Where(t => t.input == null).ToList();
					var x = op.GetGenerator();
					//opCall(x)
				}
				else
				{
					throw new Exception($"unknown operator {inputProviderNode.Name}");
				}
				
				throw new NotImplementedException();
			default:
				throw new NotImplementedException($"{node.GetType()} not yet compilable");
		}
		
		
		throw new NotImplementedException();
	}
}