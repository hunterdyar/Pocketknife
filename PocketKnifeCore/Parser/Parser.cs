using System.Linq.Expressions;
using System.Reflection.Emit;

namespace PocketKnifeCore.Parser;

public class Parser
{
    private string Source => lexer.Source;
    private Lexer lexer;
    private Queue<Token> tokens;
    public PKScriptNode Program;
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

    private PKScriptNode ParseProgram()
    {
        List<RootNode> nodesList = new List<RootNode>();
        while (tokens.TryPeek(out var token))
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
        if (tokens.TryPeek(out var token))
        {
            if (token.Type == TokenType.Input)
            {
                return ParseInputToOutputBranch();
            }
            if (token.Type == TokenType.StartBranch)
            {
               return ParseBranch();
            }else if (token.Type == TokenType.PipeOut)
            {
                return ParsePipeOut();
            }else if (token.Type == TokenType.SignalOut)
            {
                return ParseSignalOut();
            }
            else if (token.Type == TokenType.PipeSetLabel)
            {
                return ParsePipeSetLabel();
            }
            else
            {
                return ParseCommand();
            }
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

        while (tokens.TryPeek(out var token))
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

    private SignalOutNode ParseSignalOut()
    {
        Consume(TokenType.SignalOut);
        if (tokens.Count == 0 || tokens.Peek().Type == TokenType.Break)
        {
            ConsumeLinebreakOrEndOfFile();
            return new SignalOutNode();
        }else if (tokens.Peek().Type == TokenType.Identifier)
        {
            var name = ConsumeIdent();
            ConsumeLinebreakOrEndOfFile();
            return new SignalOutNode(name);
        }

        throw new Exception($"Unexpected Token: {tokens.Peek()}");
    }
    
    private PipeOutNode ParsePipeOut()
    {
        Consume(TokenType.PipeOut);
        if (tokens.Count == 0 || tokens.Peek().Type == TokenType.Break)
        {
            ConsumeLinebreakOrEndOfFile();
            return new PipeOutNode();
        }else if (tokens.Peek().Type == TokenType.OpenParen)
        {
            var opts = ParseParenOptionList();
            ConsumeLinebreakOrEndOfFile();
            return new PipeOutNode(opts);
        }
        else
        {
            var name = ConsumeIdent();
            if (tokens.Peek().Type == TokenType.OpenParen)
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
        EatOptionalLinebreaks();
        List<RootNode> branchCommands = new List<RootNode>();
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
        
        return new BranchNode(branchCommands);
    }

    private Command ParseFilterCommand()
    {
        Consume(TokenType.Filter);
        var (name, args, opts) = ParseStandardCommand();
        ConsumeLinebreakOrEndOfFile();
        return new FilterCommandNode(name, args, opts);
    }

    private Command ParseSignalCommand()
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

    private (string name, List<ExpressionNode> args, List<KeyValuePairNode>? opts) ParseStandardCommand()
    {
        var name = ConsumeIdent();
        List<ExpressionNode> args = new List<ExpressionNode>();
        List<KeyValuePairNode>? opts = null;
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

    private List<KeyValuePairNode> ParseParenOptionList()
    {
        List<KeyValuePairNode> props = new List<KeyValuePairNode>();
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
                EatOptionalLinebreaks();
                Consume(TokenType.Equals);
                EatOptionalLinebreaks();
                var e = ParseExpression();
                props.Add(new KeyValuePairNode(p,e));
            }
            else
            {
                throw new Exception($"Unexpected token {token.Type}");
            }
        }

        Consume(TokenType.CloseParen);
        return props;
    }

    private ExpressionNode ParseExpression()
    {
        if (tokens.TryPeek(out var token))
        {
            switch (token.Type)
            {
                case TokenType.Identifier:
                    var t = tokens.Dequeue();
                    return new IdentifierNode(t.GetSource(Source));
                case TokenType.Number:
                    t = tokens.Dequeue();
                    return new NumberNode(t.GetSource(Source));
                case TokenType.Label:
                    return ParseLabel();
                case TokenType.String:
                     t = tokens.Dequeue();
                    return new StringLiteralNode(t.GetSource(Source));
                default:
                    throw new Exception($"Unexpected token {token.Type}");
            }
        }
        else
        {
            throw new Exception($"Unexpected end of stream. Expected expression.");
        }
    }

    private LabelNode ParseLabel()
    {
        Consume(TokenType.Label);
        var id = ConsumeIdent();
        return new LabelNode(id);
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

