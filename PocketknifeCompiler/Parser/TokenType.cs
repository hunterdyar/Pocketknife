namespace PocketKnife.Compiler;
public enum TokenType
{
    Identifier, //asdf
    String, //"asdf"
    Number, 
    Label, //@
    Pipe, //|
    PipeIn, //|>
    StartBranch,//.
    EndBranchStop,//^
    EndBranchAppend,//&
    EndBranchReplace,//<
    Signal,//:
    Filter,//~
    PackList,//<>
    UnpackList,//><
    Command,//
    OpenParen,//(
    CloseParen,//)
    Equals,//=
    Comma,//,
    Input,//>
    LineBreak,//\n
    Bang,//!
    GroupStart,//[
    GroupEnd//]
}