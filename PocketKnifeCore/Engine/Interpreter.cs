namespace PocketKnifeCore.Engine;

public class Interpreter
{
    private Environment _env;
    
    public void RunScript(PKScript script)
    {
        _env = new Environment();
        
        foreach (var rootNode in script.RootNodes)
        {
            Walk(rootNode);
        }
    }

    private void Walk(RootNode node)
    {
        switch (node)
        {
            case Branch branch:
                //walk the branch.
                PushContext();
                foreach (var nodes in branch.Commands)
                {
                    Walk(nodes);
                }

                PopContext();
                break;
            case InputProvider inputProvider:
                //create a new context set and run it.
                //>dir directoryPathString
                //go to our dictionary of InputProviders, which should take the arguments and return an IPKInputProvider
                    //so >dir path returns a PKDirectoryInfo(new DirectoryInfo(path))
                var arguments = inputProvider.Arguments.Select(x=>WalkExpression(x)).ToArray();
                if (inputProvider.Options != null)
                {
                    throw new NotImplementedException("Arguments not yet implemented");
                    //todo: convert to key/value pairs as a runtime?
                }
                var input = _env.GetInputProvider(inputProvider.Name, arguments);
                //push it on the stack. Then start enumerating!

                foreach (var item in input.Enumerate())
                {
                    //pushContext
                    //WalkOnItem
                        //abortable.
                }
                //create a new context from the source and start enumerating the pkitems.
                break;
            case PipelineCommand pipelineCommand:
                //call transformation and pass in the context object.
                break;
            case FilterCommand filterCommand:
                //we need to the current iteration list now according to the func<bool> invoked
                break;
            case SignalCommand signalCommand:
                break;
            case PipeOut pipeOut:
                //there should be one argument which is a label expression. 
                //that's not enforced by the parser, because i'm... not sure it's true!
           default:
                throw new Exception($"Unhandled node {node}");
                break;
            
        }
    }

    private PKItem WalkExpression(Expression expression)
    {
        switch (expression)
        {
            case Identifier identifier:
                return new PKString(identifier.Name);
            case StringLiteral stringLiteral:
                return new PKString(stringLiteral.Value);
            case Number number:
                return new PKNumber(number.Value);
            case Label label:
                throw new NotImplementedException("label value lookup not yet implemented");
            default:
                throw new Exception($"Unhandled node {expression}");
        }
    }

    private void WalkCommand(Command command)
    {
        switch (command)
        {
            
        }
    }

    private void PopContext()
    {
        
    }

    private void PushContext()
    {
        //
    }
}