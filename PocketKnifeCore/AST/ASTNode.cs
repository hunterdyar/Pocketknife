using PocketKnifeCore.Parser;

namespace PocketKnifeCore;

public abstract class ASTNode
{
    public SourceSlice Start;
}

public class PKScript : ASTNode
{
    public List<RootNodes> RootNodes;

    public PKScript(List<RootNodes> nodes)
    {
        RootNodes = nodes;
    }
}

public class RootNodes : ASTNode
{
    
}
public class Branch : RootNodes
{
    List<RootNodes> Commands;
    public Branch(List<RootNodes> commands)
    {
        Commands = commands;
    }
}
public class PipeOut : RootNodes
{
    public PipeOut(Expression expression) 
    {
    }
}