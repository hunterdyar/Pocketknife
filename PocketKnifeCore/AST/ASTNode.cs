using PocketKnifeCore.Parser;

namespace PocketKnifeCore;

public abstract class ASTNode
{
    public SourceSlice Start;
}

public class PKScript : ASTNode
{
    public List<RootNode> RootNodes;

    public PKScript(List<RootNode> nodes)
    {
        RootNodes = nodes;
    }
}

public class RootNode : ASTNode
{
    
}
public class Branch : RootNode
{
    public List<RootNode> Commands;
    public Branch(List<RootNode> commands)
    {
        Commands = commands;
    }
}
public class PipeOut : RootNode
{
    public PipeOut(Expression expression) 
    {
    }
}