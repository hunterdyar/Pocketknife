using PocketKnife.Compiler;

namespace PocketknifeCore.Errors;

public class CompilerException : PocketknifeException
{
	public SourceSlice Source;
	public CompilerException(SourceSlice source, string message) : base(message)
	{
		Source = source;
	}

	public override string Message => PrettyMessage();

	private string PrettyMessage()
	{
		return $"Compiler Error at '{Source}': {base.Message}";
	}
}