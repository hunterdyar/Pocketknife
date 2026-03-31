using System.Linq.Expressions;
using System.Reflection.Emit;

namespace PocketKnifeCore.Parser;

public class Parser
{
    private string Source => _lexer.Source;
    public Lexer Lexer => _lexer;
    private Lexer _lexer;
    private Queue<Token> _tokens;
    public PKScriptNode Program;
    public void Parse(string input)
    {
        _lexer = new Lexer(input);
        Parse(_lexer);
    }
    public void Parse(Lexer input)
    {
        //Reset/Initiate Command List.
        _lexer = input;
        _tokens = new Queue<Token>(input.Tokens);
        Program = ParseProgram();
    }

    private PKScriptNode ParseProgram()
    {
        List<RootNode> nodesList = new List<RootNode>();
        while (_tokens.TryPeek(out var token))
        {
            //on root level, linebreaks are extra and thats fine!
            //parseRootNodes, but there's only two right now so no need to extract method.
           nodesList.Add(ParseRootNode());
        }

        return new PKScriptNode(nodesList);
    }

    private RootNode ParseRootNode()
    {
        EatOptionalLinebreaks();
        //todo: switch/case
        if (_tokens.TryPeek(out var token))
        {
            return token.Type switch
            {
                TokenType.Input => ParseInputToOutputBranch(),
                TokenType.StartBranch => ParseBranch(),
                TokenType.PipeOut => ParsePipeOut(),
                TokenType.SignalOut => ParseSignalOut(),
                TokenType.PipeSetLabel => ParsePipeSetLabel(),
                _ => ParseCommand()
            };
        }
        throw new  Exception($"Unexpected end of stream");
    }




    private RootNode ParsePipeSetLabel()
    {
        Consume(TokenType.PipeSetLabel);
        var label = ParseLabel();
        return new PipeSetLabelNode(label);
    }

    private InputBranchNode ParseInputToOutputBranch()
    {
        var input = ParseInputCommand();
        List<RootNode> commands = new List<RootNode>();

        while (_tokens.TryPeek(out var token))
        {
            if (token.Type == TokenType.Output)
            {
                //todo:output can be a command? like <save?
                Consume(TokenType.Output);
                break;
            }else if (token.Type == TokenType.Break)
            {
                Consume(TokenType.Break);
            }
            else
            {
                commands.Add(ParseRootNode());
            }

            EatOptionalLinebreaks();
        }
        
        var b = new InputBranchNode(input, commands);
        return b;
    }

    private void EatOptionalLinebreaks()
    {
        while (_tokens.TryPeek(out var token))
        {
            if (token.Type == TokenType.LineBreak)
            {
                _tokens.Dequeue();
            }
            else
            {
                return;
            }
        }
    }

    private CommandNode ParseCommand()
    {
        var peek = _tokens.Peek();
        switch (peek.Type)
        {
            case TokenType.Pipe:
                return ParsePipeCommand();
            case TokenType.Input:
                return ParseInputCommand();
            case TokenType.Signal:
                return ParseSignalCommand();
            case TokenType.Filter:
                return ParseFilterCommand();
            case TokenType.PipeIn:
                return ParsePipeIn();
            case TokenType.Output:
            //<

            default:
                throw new ParserException(this, "Unexpected token " + peek.Type);
        }
    }

    private SignalOutNode ParseSignalOut()
    {
        Consume(TokenType.SignalOut);
        if (_tokens.Count == 0 || _tokens.Peek().Type == TokenType.Break)
        {
            ConsumeLinebreakOrEndOfFile();
            return new SignalOutNode();
        }else if (_tokens.Peek().Type == TokenType.Identifier)
        {
            var name = ConsumeIdent();
            ConsumeLinebreakOrEndOfFile();
            return new SignalOutNode(name);
        }

        throw new ParserException(this,$"Unexpected Token: {_tokens.Peek()}");
    }
    
    private PipeOutNode ParsePipeOut()
    {
        Consume(TokenType.PipeOut);
        if (_tokens.Count == 0 || _tokens.Peek().Type == TokenType.Break)
        {
            ConsumeLinebreakOrEndOfFile();
            return new PipeOutNode();
        }else if (_tokens.Peek().Type == TokenType.Label)
        {
            var label = ParseLabel();
            if (_tokens.Peek().Type == TokenType.OpenParen)
            {
                var opts = ParseParenOptionList();
                ConsumeLinebreakOrEndOfFile();
                return new PipeOutNode(label, opts);
            }

            return new PipeOutNode(label);
        }
        
        if (_tokens.Peek().Type == TokenType.OpenParen)
        {
            var opts = ParseParenOptionList();
            ConsumeLinebreakOrEndOfFile();
            return new PipeOutNode(opts);
        }
        else
        {
            var name = ConsumeIdent();
            if (_tokens.Peek().Type == TokenType.OpenParen)
            {
                var opts = ParseParenOptionList();
                ConsumeLinebreakOrEndOfFile();
                return new PipeOutNode(name, opts);
            }
            ConsumeLinebreakOrEndOfFile();
            return new PipeOutNode(name);
        }
    }
    private BranchNode ParseBranch()
    {
        Consume(TokenType.StartBranch);
        string ident = "";
        if (_tokens.Peek().Type == TokenType.Identifier)
        {
            ident = ConsumeIdent();
        }

        EatOptionalLinebreaks();

        List<RootNode> branchCommands = new List<RootNode>();
        while (_tokens.TryPeek(out var token))
        {
            if (token.Type == TokenType.EndBranch)
            {
                Consume(TokenType.EndBranch);
                break;
            }
            else
            {
                
                branchCommands.Add(ParseRootNode());
            }
            EatOptionalLinebreaks();
        }
        
        return new BranchNode(ident, branchCommands);
    }

    private CommandNode ParseFilterCommand()
    {
        Consume(TokenType.Filter);
        var (name, args, opts) = ParseStandardCommand();
        ConsumeLinebreakOrEndOfFile();
        return new FilterCommandNode(name, args, opts);
    }

    private CommandNode ParseSignalCommand()
    {
        Consume(TokenType.Signal);
        var (name, args, opts) = ParseStandardCommand();
        ConsumeLinebreakOrEndOfFile();
        return new SignalCommandNode(name, args, opts);
    }

    private InputProviderNode ParseInputCommand()
    {
        Consume(TokenType.Input);
        var (name, args, opts) = ParseStandardCommand();
        ConsumeLinebreakOrEndOfFile();
        return new InputProviderNode(name, args, opts);
    }

    private PipelineCommandNode ParsePipeCommand()
    {
        Consume(TokenType.Pipe);
        var (name, args, opts) = ParseStandardCommand();
        ConsumeLinebreakOrEndOfFile();
        return new PipelineCommandNode(name, args, opts);
    }

    private CommandNode ParsePipeIn()
    {
        Consume(TokenType.PipeIn);
        var (name, args, opts) = ParseStandardCommand();
        EatOptionalLinebreaks();

        List<RootNode> branchCommands = new List<RootNode>();
        while (_tokens.TryPeek(out var token))
        {
            if (token.Type == TokenType.Output)
            {
                Consume(TokenType.Output);
                break;
            }
            else
            {
                branchCommands.Add(ParseRootNode());
            }

            EatOptionalLinebreaks();
        }
        
        return new PipeInCommandNode(branchCommands, name, args, opts);
    }

    private void ConsumeLinebreakOrEndOfFile()
    {
        if (_tokens.TryPeek(out var token))
        {
            if (token.Type == TokenType.LineBreak)
            {
                _tokens.Dequeue();
                return;
            }
            else
            {
                throw new ParserException(this, token, $"Unexpected token {token.Type}");
            }
        }
        else
        {
            return;
        }
    }

    private (string name, List<ExpressionNode> args, List<KeyValuePairNode>? opts) ParseStandardCommand()
    {
        var name = ConsumeIdent();
        List<ExpressionNode> args = new List<ExpressionNode>();
        List<KeyValuePairNode>? opts = null;
        while (_tokens.Count > 0 && _tokens.Peek().Type != TokenType.LineBreak)
        {
            if (_tokens.Peek().Type == TokenType.OpenParen)
            { 
                opts = ParseParenOptionList();
            }
            else
            {
                var e = ParseExpression();
                args.Add(e);
            }
        }
        return (name, args, opts);
    }

    private List<KeyValuePairNode> ParseParenOptionList()
    {
        List<KeyValuePairNode> props = new List<KeyValuePairNode>();
        Consume(TokenType.OpenParen);

        while (_tokens.TryPeek(out var token))
        {
            while (token.Type == TokenType.LineBreak)
            {
                Consume(TokenType.LineBreak);
                continue;
            }
            
            if (token.Type == TokenType.CloseParen)
            {
                break;
            }

            if (token.Type == TokenType.Identifier)
            {
                var p = ConsumeIdent();
                EatOptionalLinebreaks();
                Consume(TokenType.Equals);
                EatOptionalLinebreaks();
                var e = ParseExpression();
                props.Add(new KeyValuePairNode(p,e));
            }
            else
            {
                throw new ParserException(this, token, $"Unexpected token {token.Type}");
            }
        }

        Consume(TokenType.CloseParen);
        return props;
    }

    private ExpressionNode ParseExpression()
    {
        if (_tokens.TryPeek(out var token))
        {
            switch (token.Type)
            {
                case TokenType.Identifier:
                    var t = _tokens.Dequeue();
                    return new IdentifierNode(t.GetSource(Source));
                case TokenType.Number:
                    t = _tokens.Dequeue();
                    return new NumberNode(t.GetSource(Source));
                case TokenType.Label:
                    return ParseLabel();
                case TokenType.String:
                     t = _tokens.Dequeue();
                    return new StringLiteralNode(t.GetSource(Source));
                case TokenType.GroupStart:
                    return ParseCommandGroupExpression();
                default:
                    throw new ParserException(this, token,$"Unexpected token {token.Type}");
            }
        }
        else
        {
            throw new ParserException(this,$"Unexpected end of stream. Expected expression.");
        }
    }

    private LabelNode ParseLabel()
    {
        Consume(TokenType.Label);
        var id = ConsumeIdent();
        return new LabelNode(id);
    }

    private ExpressionNode ParseCommandGroupExpression()
    {
        var commands = new List<CommandNode>();
        Consume(TokenType.GroupStart);
        EatOptionalLinebreaks();

        while (_tokens.TryPeek(out var token))
        {
            if (token.Type == TokenType.GroupEnd)
            {
                Consume(TokenType.GroupEnd);
                break;
            }
            else
            {
                commands.Add(ParseCommand());
            }

            EatOptionalLinebreaks();
        }
        EatOptionalLinebreaks();
        return new CommandGroupExpression(commands);
    }

    private string ConsumeIdent()
    {
        if (_tokens.Count == 0)
        {
            throw new ParserException(this, $"Unexpected End of Stream. Expected Identifier");
        }

        if (_tokens.Peek().Type == TokenType.Identifier)
        {
            var t = _tokens.Dequeue();
            return t.GetSource(Source);
        }
        else
        {
            throw new ParserException(this, _tokens.Peek(),$"Unexpected token {_tokens.Peek()}. Expected Identifier");
        }
    }

    private void Consume(TokenType tokenType, bool optional = false)
    {
        if (_tokens.Count == 0)
        {
            if (optional)
            {
                return;
            }
            else
            {
                throw new ParserException(this, "Unexpected End of Stream. Expected {tokenType}");
            }
        }
        
        if (_tokens.Peek().Type == tokenType)
        {
            _tokens.Dequeue();
        }
        else
        {
            if (!optional)
            {
                throw new ParserException(this, _tokens.Peek(),$"Unexpected Token {_tokens.Peek().Type}. Expected {tokenType}");
            }
        }
    }
}

