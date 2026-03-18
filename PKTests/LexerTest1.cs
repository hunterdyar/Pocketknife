using PocketKnifeCore.Parser;

namespace PKTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        
    }

    [TestCase("0",0)]
    [TestCase("-1",-1)]
    [TestCase("2",2)]
    [TestCase("3.01",3.01)]
    [TestCase("-3.02",-3.02)]
    [TestCase(".1234",.1234)]
    public void LexOneNumberToken(string source, double expected)
    {
        var l = new Lexer(source);
        Assert.That(l.TokenCount, Is.EqualTo(1));
        Assert.That(double.Parse(l.Tokens[0].GetSource(source)), Is.EqualTo(expected));
    }
    
    [TestCase(">",TokenType.Input)]
    [TestCase("<",TokenType.Output)]
    [TestCase("><",TokenType.UnpackList)]
    [TestCase("<>",TokenType.PackList)]
    [TestCase("|",TokenType.Pipe)]
    [TestCase(".",TokenType.StartBranch)]
    [TestCase("^",TokenType.EndBranch)]
    [TestCase("^", TokenType.EndBranch)]
    [TestCase("<", TokenType.Output)]
    [TestCase("<<", TokenType.Output)]
    [TestCase("<<<<<", TokenType.Output)]
    [TestCase("--", TokenType.Break)]
    [TestCase("----", TokenType.Break)]
    public void TestOneToken(string source, TokenType expected)
    {
        var l = new Lexer(source);
        Assert.That(l.TokenCount, Is.EqualTo(1));
        Assert.That(l.Tokens[0].Type, Is.EqualTo(expected));
    }
    
}