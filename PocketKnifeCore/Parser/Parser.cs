using System.Reflection.Emit;

namespace PocketKnifeCore.Parser;

public class Parser
{
    private string Source => lexer.Source;
    private Lexer lexer;
    private Queue<Token> tokens;
    public PKScript Program;
    public void Parse(string input)
    {
        lexer = new Lexer(input);
        Parse(lexer);
    }
    public void Parse(Lexer input)
    {
        //Reset/Initiate Command List.
        lexer = input;
        tokens = new Queue<Token>(input.Tokens);
        Program = ParseProgram();
    }

    private PKScript ParseProgram()
    {
        List<RootNodes> nodesList = new List<RootNodes>();
        while (tokens.TryPeek(out var token))
        {
            //on root level, linebreaks are extra and thats fine!
            //parseRootNodes, but there's only two right now so no need to extract method.
           nodesList.Add(ParseRootNode());
        }

        return new PKScript(nodesList);
    }

    private RootNodes ParseRootNode()
    {
        EatOptionalLinebreaks();
        if (tokens.TryPeek(out var token))
        {
            if (token.Type == TokenType.StartBranch)
            {
               return ParseBranch();
            }else if (token.Type == TokenType.PipeOut)
            {
                return ParsePipeOut();
            }
            else
            {
                return ParseCommand();
            }
        }
        throw new  Exception($"Unexpected end of stream");
    }

    private void EatOptionalLinebreaks()
    {
        while (tokens.TryPeek(out var token))
        {
            if (token.Type == TokenType.LineBreak)
            {
                tokens.Dequeue();
            }
            else
            {
                return;
            }
        }
    }

    private Command ParseCommand()
    {
        var peek = tokens.Peek();
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
            case TokenType.Output:
                //<
            default:
                throw new Exception("Unexpected token " + peek.Type);
        }
        
    }

    private PipeOut ParsePipeOut()
    {
        Consume(TokenType.PipeOut);
        var e = ParseExpression();
        return new PipeOut(e);
    }
    private Branch ParseBranch()
    {
        Consume(TokenType.StartBranch);
        EatOptionalLinebreaks();
        List<RootNodes> branchCommands = new List<RootNodes>();
        while (tokens.TryPeek(out var token))
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
        
        return new Branch(branchCommands);
    }

    private Command ParseFilterCommand()
    {
        Consume(TokenType.Filter);
        var (name, args, opts) = ParseStandardCommand();
        ConsumeLinebreakOrEndOfFile();
        return new InputProvider(name, args, opts);
    }

    private Command ParseSignalCommand()
    {
        Consume(TokenType.Signal);
        var (name, args, opts) = ParseStandardCommand();
        ConsumeLinebreakOrEndOfFile();
        return new InputProvider(name, args, opts);
    }

    private InputProvider ParseInputCommand()
    {
        Consume(TokenType.Input);
        var (name, args, opts) = ParseStandardCommand();
        ConsumeLinebreakOrEndOfFile();
        return new InputProvider(name, args, opts);
    }

    private PipelineCommand ParsePipeCommand()
    {
        Consume(TokenType.Pipe);
        var (name, args, opts) = ParseStandardCommand();
        ConsumeLinebreakOrEndOfFile();
        return new PipelineCommand(name, args, opts);
    }


    private void ConsumeLinebreakOrEndOfFile()
    {
        if (tokens.TryPeek(out var token))
        {
            if (token.Type == TokenType.LineBreak)
            {
                tokens.Dequeue();
                return;
            }
            else
            {
                throw new Exception($"Unexpected token {token.Type}");
            }
        }
        else
        {
            return;
        }
    }

    private (string name, List<Expression> args, List<PropertyValuePair>? opts) ParseStandardCommand()
    {
        var name = ConsumeIdent();
        List<Expression> args = new List<Expression>();
        List<PropertyValuePair>? opts = null;
        while (tokens.Peek().Type != TokenType.LineBreak)
        {
            
            if (tokens.Peek().Type == TokenType.OpenParen)
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

    private List<PropertyValuePair> ParseParenOptionList()
    {
        List<PropertyValuePair> props = new List<PropertyValuePair>();
        Consume(TokenType.OpenParen);

        while (tokens.TryPeek(out var token))
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
                var e = ParseExpression();
                props.Add(new PropertyValuePair(p,e));
            }
            else
            {
                throw new Exception($"Unexpected token {token.Type}");
            }
        }

        Consume(TokenType.CloseParen);
        return props;
    }

    private Expression ParseExpression()
    {
        if (tokens.TryPeek(out var token))
        {
            switch (token.Type)
            {
                case TokenType.Identifier:
                    var t = tokens.Dequeue();
                    return new Identifier(t.GetSource(Source));
                case TokenType.Number:
                    t = tokens.Dequeue();
                    return new Number(t.GetSource(Source));
                case TokenType.Label:
                    return ParseLabel();
                case TokenType.String:
                     t = tokens.Dequeue();
                    return new StringLiteral(t.GetSource(Source));
                default:
                    throw new Exception($"Unexpected token {token.Type}");
            }
        }
        else
        {
            throw new Exception($"Unexpected end of stream. Expected expression.");
        }
    }

    private Expression ParseLabel()
    {
        Consume(TokenType.Label);
        var id = ConsumeIdent();
        return new Label(id);
    }

    private string ConsumeIdent()
    {
        if (tokens.Count == 0)
        {
            throw new Exception($"Unexpected End of Stream. Expected Identifier");
        }

        if (tokens.Peek().Type == TokenType.Identifier)
        {
            var t = tokens.Dequeue();
            return t.GetSource(Source);
        }
        else
        {
            throw new Exception($"Unexpected token {tokens.Peek()}. Expected Identifier");
        }
    }

    private void Consume(TokenType tokenType, bool optional = false)
    {
        if (tokens.Count == 0)
        {
            if (optional)
            {
                return;
            }
            else
            {
                throw new Exception($"Unexpected End of Stream. Expected {tokenType}");
            }
        }
        
        if (tokens.Peek().Type == tokenType)
        {
            tokens.Dequeue();
        }
        else
        {
            if (!optional)
            {
                throw new Exception($"Unexpected Token {tokenType}");
            }
        }
    }
}

