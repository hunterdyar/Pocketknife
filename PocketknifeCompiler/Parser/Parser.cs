using System.Diagnostics;
using PocketknifeCore;

namespace PocketKnife.Compiler;

public class Parser
{
    private string Source => _lexer.Source;
    public Lexer Lexer => _lexer;
    private Lexer _lexer;
    private int _tokenIndex;
    public ScriptNode Program;
    public void Parse(string input)
    {
        _lexer = new Lexer(input);
        Parse(_lexer);
    }
    
    public void Parse(Lexer input)
    {
        //Reset/Initiate Command List.
        _lexer = input;
        _tokenIndex = 0;
        Program = ParseProgram();
    }

    private ScriptNode ParseProgram()
    {
        List<RootNode> nodesList = new List<RootNode>();
        while (_tokenIndex < _lexer.TokenCount)
        {
            //on root level, linebreaks are extra and thats fine!
            //parseRootNodes, but there's only two right now so no need to extract method.
           nodesList.Add(ParseRootNode());
        }

        return new ScriptNode(nodesList);
    }

    private RootNode ParseRootNode()
    {
        EatOptionalLinebreaks();
        if (_tokenIndex < _lexer.TokenCount)
        {
            var token = _lexer.Tokens[_tokenIndex];
            return token.Type switch
            {
                TokenType.Input => ParseInputToOutputBranch(),
                TokenType.PipeIn => ParseInputToOutputBranch(),
                TokenType.StartBranch => ParseBranch(),//.
                TokenType.PackList => ParsePackList(),
                TokenType.UnpackList => ParseUnpackList(),
                TokenType.PatternStart => ParsePatternMatch(),
                // TokenType.Signal => ParseSignalCommand(),//we overload : for things like "pretty-print-whatever" and "print-this-data, so i'm going to not allow this for now. 99% of time it wont be allowed.
                _ => ParseCommand()
            };
        }
        throw new  Exception($"Unexpected end of stream");
    }

    private RootNode ParsePatternMatch()
    {
        Consume(TokenType.PatternStart);
        if (_tokenIndex < _lexer.TokenCount)
        {
            var token = _lexer.Tokens[_tokenIndex];
            if (token.Type == TokenType.Identifier)
            {
                var matchName = ConsumeIdent();
                //?try, ?help
                //or takes the same standard command as any filter?
                //~contains could be ?contains and //+true, +false.
                //these are edge cases, deal with it later.
                throw new NotImplementedException();

            }else if (token.Type == TokenType.LineBreak)
            {
                EatOptionalLinebreaks();
                //?
                //+branches...
                //^
                BranchType closer = default;
                var arms = ParsePatternBranchArms();
                if (arms.Count > 0)
                {
                    closer = arms[^1].CloseType;
                    arms[^1].CloseType = BranchType.Unknown;//we take it's token!
                }
                else
                {
                    //no arms, but ?^ is allowed.
                    if (!TryEndBranch(out closer, false))
                    {
                        throw new Exception("sometin broke with patterh match block i think.");
                    }
                }

                return new NakedPatternMatch(arms, closer);
            }
            else
            {
                EatOptionalLinebreaks();
                var e = ParseExpression();
                //? expression is shorthand for if/else.
                //+ true
                //+ false
                //^
                var branches = ParsePatternBranchArms();
                throw new NotImplementedException();
            }
        }
        else
        {
            throw new ParserException(this, "Unexpected end of stream");
        }

    }

    private List<PatternBranchArm> ParsePatternBranchArms()
    {
        BranchType branchType = default;
        List<PatternBranchArm> arms = new List<PatternBranchArm>();
        while (_tokenIndex < _lexer.TokenCount)
        {
            var token = _lexer.Tokens[_tokenIndex];
            if (token.Type != TokenType.PatternBranch)
            {
                break;
            }
            
            arms.Add(ParsePatternBranchArm());
            EatOptionalLinebreaks();
            //consume root nodes and add to list like elsewhere.
            if (TryEndBranch(out branchType, false))
            {
                break;
            }
        }

        if (arms.Count(x => x.IsDefault) > 1)
        {
            throw new ParserException(this, "Cannot have more than one default arm (~~) in a pattern expression");
        }
        
        return arms;
    }

    private PatternBranchArm ParsePatternBranchArm()
    {
        Consume(TokenType.PatternBranch);//+
        BranchType branchType = default;
        FilterCommandNode? filter = null;
        if (_tokenIndex < _lexer.TokenCount)
        {
            var t = _lexer.Tokens[_tokenIndex];
            if (t.Type == TokenType.Filter)
            {
                filter = ParseFilterCommand();
            }else if (t.Type == TokenType.PatternDefault)
            {
                Consume(TokenType.PatternDefault);
                filter = new DefaultFilterCommandNode();
            }
            else
            {
                throw new ParserException(this, t, $"Unexpected token {t.Type}. Branches (+) should be followed by filters (~) or 'other/else' (~~)");
            }
        }
        
        List<RootNode> commands = new List<RootNode>();
        while (_tokenIndex < _lexer.TokenCount)
        {
            //consume root nodes and add to list like elsewhere.
            if (TryEndBranch(out branchType, true))
            {
                break;
            }
            //parse root nodes list.
            commands.Add(ParseRootNode());
            EatOptionalLinebreaks();
        }
        
        //end-of-file can implicitly end a branch. which is allowed (but maybe shouldn't be for pattern branch arms? can be ambiguous?)
        if(_tokenIndex >= _lexer.TokenCount)
        {
            branchType = BranchType.SideEffect;
        }
        
        return new PatternBranchArm(filter, commands, branchType);
    }

    private PackListNode ParsePackList()
    {
        Consume(TokenType.PackList);
        return new PackListNode();
    }

    private UnpackListNode ParseUnpackList()
    {
        Consume(TokenType.UnpackList);
        return new UnpackListNode();
    }

    private InputBranchNode ParseInputToOutputBranch()
    {
        var input = ParseInputCommand();
        List<RootNode> commands = new List<RootNode>();
        BranchType btype = default;
        while (_tokenIndex < _lexer.TokenCount)
        {
            if (TryEndBranch(out btype))
            {
                break;
            }
            else
            {
                commands.Add(ParseRootNode());
            }

            EatOptionalLinebreaks();
        }

        if (_tokenIndex >= _lexer.TokenCount)
        {
            btype = BranchType.SideEffect;
        }
        
        var b = new InputBranchNode(input, btype, commands);
        return b;
    }

    private void EatOptionalLinebreaks()
    {
        while (_tokenIndex < _lexer.TokenCount)
        {
            if (_lexer.Tokens[_tokenIndex].Type == TokenType.LineBreak)
            {
                _tokenIndex++;
            }
            else
            {
                return;
            }
        }
    }

    private CommandNode ParseCommand()
    {
        var peek = _lexer.Tokens[_tokenIndex];
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
            case TokenType.Bang:
                return ParseAbortCommand();
            case TokenType.PipeIn:
                return ParsePipeInInputProvider();
            case TokenType.PatternStart:
                throw new NotImplementedException();
            default:
                throw new ParserException(this, "Unexpected token " + peek.Type);
        }
    }

    private bool TryEndBranch(out BranchType branchType, bool allowPatternBranchAutoClose = false)
    {
        if (_tokenIndex < _lexer.TokenCount)
        {
            var t = _lexer.Tokens[_tokenIndex];
            switch (t.Type)
            {
                case TokenType.EndBranchStop:
                    Consume(TokenType.EndBranchStop);
                    branchType = BranchType.SideEffect;
                    return true;
                case TokenType.EndBranchAppend:
                    Consume(TokenType.EndBranchAppend);
                    branchType = BranchType.ListAppend;
                    return true;
                case TokenType.EndBranchReplace:
                    Consume(TokenType.EndBranchReplace);
                    branchType = BranchType.Replace;
                    return true;
                case TokenType.PatternBranch:
                    branchType = BranchType.Unknown;
                    return allowPatternBranchAutoClose; 
                default:
                    branchType = BranchType.Unknown;
                    return false;
            }
        }
        //end of file.
        branchType = BranchType.Unknown;
        return false;
    }
    
    private BranchNode ParseBranch()
    {
        Consume(TokenType.StartBranch);
        LabelNode? label;
        if (_lexer.Tokens[_tokenIndex].Type == TokenType.Label)
        {
            label = ParseLabel();
        }
        else
        {
            label = null;
        }
        EatOptionalLinebreaks();

        List<RootNode> branchCommands = new List<RootNode>();
        BranchType branchType = BranchType.Unknown;
        while (_tokenIndex < _lexer.TokenCount)
        {
            if (TryEndBranch(out branchType))
            {
                break;
            }
            else
            {
                branchCommands.Add(ParseRootNode());
            }
            
            EatOptionalLinebreaks();
        }

        if (_tokenIndex >= _lexer.TokenCount)
        {
            //the branch ended with the end of the file, which is allowed
            branchType = BranchType.SideEffect;
        }else if (branchType == BranchType.Unknown)
        {
            throw new ParserException(this, _lexer.Tokens[_tokenIndex],$"Unexpected token {_lexer.Tokens[_tokenIndex]}. Expected ^, <, or & to end a branch.");
        }
        
        return new BranchNode(label, branchType, branchCommands);
    }

    private FilterCommandNode ParseFilterCommand()
    {
        Consume(TokenType.Filter);
        var (name, args, opts) = ParseStandardCommand();
        ConsumeLinebreakOrEndOfFile();
        return new FilterCommandNode(name, args, opts);
    }

    private AbortCommandNode ParseAbortCommand()
    {
        Consume(TokenType.Bang);
        var (name, args, opts) = ParseStandardCommand();
        ConsumeLinebreakOrEndOfFile();
        return new AbortCommandNode(name, args, opts);
    }

    private SignalCommandNode ParseSignalCommand()
    {
        Consume(TokenType.Signal);
        var (name, args, opts) = ParseStandardCommand();
        ConsumeLinebreakOrEndOfFile();
        return new SignalCommandNode(name, args, opts);
    }

    private InputProviderNode ParseInputCommand()
    {
        if (_lexer.Tokens[_tokenIndex].Type == TokenType.PipeIn)
        {
            return ParsePipeInInputProvider();
        }//else
        
        Consume(TokenType.Input);
        //we now have either a literal or an identifier.
        if (_lexer.Tokens[_tokenIndex].Type == TokenType.Identifier)
        {
            var (name, args, opts) = ParseStandardCommand();
            ConsumeLinebreakOrEndOfFile();
            return new InputProviderNode(name, args, opts);
        }
        else
        {
            var (args, opts) = ParseNakedCommand();
            
            ConsumeLinebreakOrEndOfFile();
            return new InputLiteralProviderNode(args, opts);
        }
    }

    private PipeInInputProviderNode ParsePipeInInputProvider()
    {
        Consume(TokenType.PipeIn);
        var (name, args, opts) = ParseStandardCommand();
        EatOptionalLinebreaks();

        // List<RootNode> branchCommands = new List<RootNode>();
        // while (_tokenIndex < _lexer.TokenCount)
        // {
        //     var token = _lexer.Tokens[_tokenIndex];
        //     if (token.Type == TokenType.EndBranchReplace)
        //     {
        //         Consume(TokenType.EndBranchReplace);
        //         break;
        //     }
        //     else
        //     {
        //         branchCommands.Add(ParseRootNode());
        //     }
        //
        //     EatOptionalLinebreaks();
        // }

        return new PipeInInputProviderNode(name, args, opts);
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
        if (_tokenIndex < _lexer.TokenCount)
        {
            var token = _lexer.Tokens[_tokenIndex];
            if (token.Type == TokenType.LineBreak)
            {
                _tokenIndex++;
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

    private (string name, List<ExpressionNode> args, Options? opts) ParseStandardCommand()
    {
        var name = ConsumeIdent();
        List<ExpressionNode> args = new List<ExpressionNode>();
        List<KeyValuePairNode>? opts = null;
        while (_tokenIndex < _lexer.TokenCount && _lexer.Tokens[_tokenIndex].Type != TokenType.LineBreak)
        {
            if (_lexer.Tokens[_tokenIndex].Type == TokenType.OpenParen)
            { 
                opts = ParseParenOptionList();
            }
            else
            {
                var e = ParseExpression();
                args.Add(e);
            }
        }
        return (name, args, new Options(opts));
    }

    private (List<ExpressionNode> args, Options? opts) ParseNakedCommand()
    {
        List<ExpressionNode> args = new List<ExpressionNode>();
        List<KeyValuePairNode>? opts = null;
        while (_tokenIndex < _lexer.TokenCount && _lexer.Tokens[_tokenIndex].Type != TokenType.LineBreak)
        {
            if (_lexer.Tokens[_tokenIndex].Type == TokenType.OpenParen)
            {
                opts = ParseParenOptionList();
            }
            else
            {
                var e = ParseExpression();
                args.Add(e);
            }
        }

        return (args, new Options(opts));
    }

    private List<KeyValuePairNode> ParseParenOptionList()
    {
        List<KeyValuePairNode> props = new List<KeyValuePairNode>();
        Consume(TokenType.OpenParen);

        while (_tokenIndex < _lexer.TokenCount)
        {
            var token = _lexer.Tokens[_tokenIndex];
            while (token.Type == TokenType.LineBreak)
            {
                Consume(TokenType.LineBreak);
                if (_tokenIndex >= _lexer.TokenCount) break;
                token = _lexer.Tokens[_tokenIndex];
                continue;
            }
            
            if (_tokenIndex >= _lexer.TokenCount || token.Type == TokenType.CloseParen)
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
        if (_tokenIndex < _lexer.TokenCount)
        {
            var token = _lexer.Tokens[_tokenIndex];
            switch (token.Type)
            {
                case TokenType.Identifier:
                    _tokenIndex++;
                    return new IdentifierNode(token.GetSource(Source));
                case TokenType.Number:
                    _tokenIndex++;
                    return NumberNode.FromString(token.GetSource(Source));
                case TokenType.Label:
                    return ParseLabel();
                case TokenType.String:
                    _tokenIndex++;
                    return new StringLiteralNode(token.GetSource(Source));
                case TokenType.GroupStart:
                    //[] is empty-list literal. maybe it shouldn't be?
                    if(_tokenIndex + 1 < _lexer.TokenCount && _lexer.Tokens[_tokenIndex + 1].Type == TokenType.GroupEnd)
                    {
                        //todo: we'll need to save our source span always when parsing.
                        _tokenIndex += 2;
                        return new EmptyListLiteralExpression();
                    }
                    //else:
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
        int reachOutCount = 0;
        while (_tokenIndex < _lexer.TokenCount && _lexer.Tokens[_tokenIndex].Type == TokenType.EndBranchStop)
        {
            reachOutCount++;
            Consume(TokenType.EndBranchStop);
        }
        var id = ConsumeIdent();
        return new LabelNode(id, reachOutCount);
    }

    private ExpressionNode ParseCommandGroupExpression()
    {
        var commands = new List<CommandNode>();
        Consume(TokenType.GroupStart);
        EatOptionalLinebreaks();

        while (_tokenIndex < _lexer.TokenCount)
        {
            var token = _lexer.Tokens[_tokenIndex];
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
      
        return new CommandGroupExpression(commands);
    }

    private string ConsumeIdent()
    {
        if (_tokenIndex >= _lexer.TokenCount)
        {
            throw new ParserException(this, $"Unexpected End of Stream. Expected Identifier");
        }

        var t = _lexer.Tokens[_tokenIndex];
        if (t.Type == TokenType.Identifier)
        {
            _tokenIndex++;
            return t.GetSource(Source);
        }
        else
        {
            throw new ParserException(this, t,$"Unexpected token {t}. Expected Identifier");
        }
    }

    private void Consume(TokenType tokenType, bool optional = false)
    {
        if (_tokenIndex >= _lexer.TokenCount)
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
        
        if (_lexer.Tokens[_tokenIndex].Type == tokenType)
        {
            _tokenIndex++;
        }
        else
        {
            if (!optional)
            {
                throw new ParserException(this, _lexer.Tokens[_tokenIndex],$"Unexpected Token {_lexer.Tokens[_tokenIndex].Type}. Expected {tokenType}");
            }
        }
    }
}

