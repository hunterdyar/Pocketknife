using System.Text;
using PocketKnifeCore.Parser;

namespace PocketKnifeCore;
public class ParserException : PocketKnifeException
{
	private readonly Token? _errToken;
	private readonly Parser.Parser _parser;
	private string _message;
	public ParserException(Parser.Parser parser, Token errToken, string message) : base(message)
	{
		_parser = parser;
		_errToken = errToken;
		_message = message;
	}

	public ParserException(Parser.Parser parser, string message) : base(message)
	{
		parser = _parser;
		_errToken = null;
		_message = message;
	}

	public override string Message => GetMessage();

	private string GetMessage()
	{
		StringBuilder sb = new StringBuilder();
		if (_errToken != null)
		{
			var t = _errToken.Value;
			var source = t.GetSource(_parser.Lexer.Source);
			var pos = _parser.Lexer.PrettyLineCol(t.Source.StartLoc);
			sb.Append($"Parsing Error at '{source}'. {pos}. {_message}");
			return sb.ToString();

		}
		else
		{
			sb.Append($"Parsing error. {_message}");
			return sb.ToString();
		}
	}
}