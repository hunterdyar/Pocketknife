using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("PocketKnife (V0.0). Provide an input pocketknife script as the first argument. For any following arguments, the script will be run from the top with this starting string.");
            return;
        } 
        FileInfo script = new FileInfo(args[0]);
        ExecuteScript(script, args);
    }

    private static void ExecuteScript(FileInfo script, string[] arguments)
    {
        if (!script.Exists)
        {
            Console.Error.WriteLine($"Error: Cannot Find File {script.FullName}.");
        }

        PKString[] pkStrings = new PKString[arguments.Length - 1];
        for (int i = 1; i < arguments.Length; i++)
        {
            pkStrings[i - 1] = new PKString(arguments[i]);
        }

        using var stream = script.OpenText();
        var source = stream.ReadToEnd();
        try
        {
            var p = new Parser.Parser();
            p.Parse(source);
            var compiler = new Compiler();
            compiler.CompileScript(p.Program);
            var i = new Interpreter();
            i.Execute(compiler.Script, pkStrings);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }
}