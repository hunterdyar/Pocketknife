namespace PocketKnifeCore.Parser;

public class Lexer
{
    private string _source;
    private List<Token> _tokens;
    public List<Token> Tokens => _tokens;
    public int TokenCount => _tokens.Count;
    private int loc;
    private char Current => _source[loc];
    public string Source => _source;

    public Lexer(string source)
    {
        _source = source;
        _tokens = new List<Token>();
        LexTokens();
    }

    private void LexTokens()
    {
        while (loc < _source.Length)
        {
            LexNextToken();
        }
    }

    private void LexNextToken()
    {
        ConsumeWhitespace();
        switch (Current)
        {
            case '\n':
                ConsumeCurrentCharAsToken(TokenType.LineBreak);
                return;
            case '/':
                if (PeekMatches('/'))
                {
                    //consume a comment to end of line or document.
                    Consume('/');
                    Consume('/');
                    while (Current != '\n')
                    {
                        Consume();
                    }

                    if (loc >= _source.Length)
                    {
                        return;
                    }
                    //wait we need linebreaks in the source for inline comments...
                    //Consume('\n');
                    return;
                }
                else
                {
                    // /can be part of a directory identifier without needing " around them.
                    goto default;
                }
                
            case ':':
                if (PeekMatches('<'))
                {
                    AddToken(TokenType.SignalOut, loc, 2);
                    Consume(':');
                    Consume('<');
                    return;
                }
                ConsumeCurrentCharAsToken(TokenType.Signal);
                return;
            case '|':
                if (PeekMatches('>'))
                {
                    AddToken(TokenType.PipeIn,loc,2);
                    Consume('|');
                    Consume('>');
                    return;
                }else if (PeekMatches('<'))
                {
                    AddToken(TokenType.PipeOut,loc,2);
                    Consume('|');
                    Consume('<');
                    return;
                }
                else if (PeekMatches('='))
                {
                    AddToken(TokenType.PipeSetLabel, loc, 2);
                    Consume('|');
                    Consume('=');
                    return;
                }
                ConsumeCurrentCharAsToken(TokenType.Pipe);
                return;
            case '>':
                //check if next is < or not.
                if (PeekMatches('<'))
                {
                    AddToken(TokenType.UnpackList,loc,2);
                    Consume('>');
                    Consume('<');
                }
                else
                {
                    ConsumeCurrentCharAsToken(TokenType.Input);
                }
                return;
            case '<':
                //check if next is > ir not.
                if (PeekMatches('>'))
                {
                    AddToken(TokenType.PackList,loc,2);
                    Consume('<');
                    Consume('>');
                }
                else if (PeekMatches('<'))//<<<<< is allowed for more visual weight.
                {
                    var startChev = loc;
                    Consume('<');
                    int chevLength = 1;
                    while (Current=='<')
                    {
                        Consume('<');
                        chevLength++;
                        if (loc >= _source.Length)
                        {
                            break;
                        }
                    }

                    AddToken(TokenType.Output, startChev, chevLength);
                    return;
                }
                else
                {
                    ConsumeCurrentCharAsToken(TokenType.Output);
                }
                return;
            case '(':
                ConsumeCurrentCharAsToken(TokenType.OpenParen);
                return;
            case ')':
                ConsumeCurrentCharAsToken(TokenType.CloseParen);
                return;
            case '!':
                ConsumeCurrentCharAsToken(TokenType.Bang);
                return;
            case '=':
                ConsumeCurrentCharAsToken(TokenType.Equals);
                return;
            case '~':
                ConsumeCurrentCharAsToken(TokenType.Filter);
                return;
            case '@':
                ConsumeCurrentCharAsToken(TokenType.Label);
                return;
            case ',':
                ConsumeCurrentCharAsToken(TokenType.Comma);
                return;
            case '*':
                ConsumeCurrentCharAsToken(TokenType.Command);
                return;
            case '.':
                if (TryPeek(out var p))
                {
                    if (char.IsDigit(p))
                    {
                        //this is a number, not a dot.
                        goto default;
                    }else if (char.IsWhiteSpace(p))
                    {
                        ConsumeCurrentCharAsToken(TokenType.StartBranch);
                        return;
                    }
                    else
                    {
                        //we can start a directory identifier with ./ or ../
                        goto default;
                    }
                }
                //or, the end of the file...
                ConsumeCurrentCharAsToken(TokenType.StartBranch);
                return;
            case '^':
                ConsumeCurrentCharAsToken(TokenType.EndBranch);
                return;
            case '-':
                if (PeekMatches('-'))
                {
                    var startDash = loc;
                    int dashLength = 1;
                    Consume('-');
                    while (Current == '-')
                    {
                        Consume('-');
                        dashLength++;
                        if (loc >= _source.Length)
                        {
                            break;
                        }
                    }
                    AddToken(TokenType.Break,startDash,dashLength);
                    return;
                }
                else
                {
                    //single dash could be a negative number
                    goto default;
                }
                return;
            case '"':
                var length = 0;
                bool escaped = false;
                Consume('"');
                var start = loc;
                while (loc < _source.Length)
                {
                    var peek = Current;
                    
                    if (peek == '\\')
                    {
                        escaped = true;
                    }
                    
                    if (peek == '"' && !escaped)
                    {
                        Consume('"');
                        break;
                    }
                    
                    escaped = false;//reset
                    Consume();
                    length++;
                }
                string value = _source.Substring(start, length);
                AddToken(TokenType.String,start,length);
                return;//we already consumed the closing "
            default:
                //ConsumeWhitespace();
                //if it starts with a minus or an integer, it might be a number.
                int identStart = loc;
                int identLength = 0;
                //todo: hex, etc.
                while (char.IsDigit(Current) || Current == '.' || Current == '-')
                {
                    identLength++;
                    Consume();
                    if (loc >= _source.Length)
                    {
                        break;
                    }
                }

                if (identLength > 0)
                {
                    AddToken(TokenType.Number, identStart, identLength);
                    return;
                }
                //otherwise, it's valid-identifier-characters until we hit whitespace.

                while (IsValidIdentifier(Current))
                {
                    identLength++;
                    Consume();
                    if (loc >= _source.Length)
                    {
                        break;
                    }
                }

                if (identLength > 0)
                {
                    AddToken(TokenType.Identifier, identStart, identLength);
                    return;
                }

                if (loc >= _source.Length)
                {
                    throw new Exception("Unexpected end of file");
                }
                else
                {
                    throw new Exception($"Unexpected Token {Current}");
                }
                break;
        }
    }

    //identifiers are basically normal c-style identifiers, but also... directories and filenames without spaces.
    //use a string if there are spaces.
    private bool IsValidIdentifier(char current)
    {
        return char.IsAsciiLetter(current)
               || char.IsDigit(current)
               || current == '_'
               || current == '-'
               || current == '/'
               || current == '\\'
               || current == '.';
    }

    private bool TryPeek(out char peek)
    {
        if (loc + 1 >= _source.Length)
        {
            peek = (char)0;
            return false;
        }

        peek = _source[loc + 1];
        return true;
    }

    private void Consume()
    {
        loc++;
    }
    private void Consume(char c)
    {
        if (loc >= _source.Length)
        {
            throw new Exception("Unexpected end of file");
        }
        if (_source[loc] == c)
        {
            loc++;
        }
        else
        {
            throw new Exception($"Unexpected Character {c}");
        }
    }

    private bool PeekMatches(char c)
    {
        if (loc + 1 >= _source.Length)
        {
            return false;
        }
        return _source[loc + 1] == c;
    }

    private void AddToken(TokenType type, int start, int length)
    {
        _tokens.Add(new Token()
        {
            Type = type,
            Source = new SourceSlice()
            {
                StartLoc = start,
                Length = length
            },
            Lexer = this
        });
    }

    private void ConsumeCurrentCharAsToken(TokenType type)
    {
        var t = new Token()
        {
            Type = type,
            Source = new SourceSlice()
            {
                Length = 1,
                StartLoc = loc
            },
            Lexer = this,
        };
        _tokens.Add(t);
        
        loc++;//consume
    }

    private void ConsumeWhitespace()
    {
        while (Current != '\n' && char.IsWhiteSpace(Current))
        {
            loc++;
            if (loc >= _source.Length)
            {
                break;
            }
        }
    }

    
}