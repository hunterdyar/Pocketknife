using PocketKnifeCore.Parser;

namespace PocketKnifeCore;

public class LexerException : PocketKnifeException
{
	private Lexer _lexer;
	private int _position;
	private string _message;
	public LexerException(Lexer lexer, int position, string message) : base(message)
	{
		_position = position;
		_lexer = lexer;
		_message = message;
		Data["position"] = position;
		Data["tokens"] = _lexer.Tokens;
	}

	public override string ToString()
	{
		return base.ToString();
	}

	public override string Message => $"Lexer Error on {_lexer.PrettyLineCol(_position)}. {_message}";
}