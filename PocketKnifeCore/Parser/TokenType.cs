namespace PocketKnifeCore.Parser;

public enum TokenType
{
    Identifier,
    String,
    Number,
    Label,
    Pipe,
    PipeOut,
    PipeIn,
    StartBranch,
    EndBranch,
    Signal,
    Filter,
    PackList,
    UnpackList,
    Command,
    Break,
    OpenParen,
    CloseParen,
    Equals,
    Comma,
    Input,
    Output,
    LineBreak,
    Bang,
}