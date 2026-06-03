using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class CompilerException : PocketKnifeException
{
	private Compiler _compiler;
	private ASTNode _errNode;
	public CompilerException(Compiler compiler, ASTNode errNode, string message) : base(message)
	{
		_compiler = compiler;
		_errNode = errNode;
	}

	public override string Message => GetMessage();

	private string GetMessage()
	{
		var pos = _compiler.Parser.Lexer.PrettyLineCol(_errNode.Start.StartLoc);
		return $"Compiler Error {pos}. {base.Message}";
	}
}