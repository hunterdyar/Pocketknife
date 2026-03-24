using System.ComponentModel;

namespace PocketKnifeCore.Engine;

public class Interpreter
{
	public void Execute(PocketKnifeScript script, string input)
	{
		var context = new Context(new PKString(input));
		script.Execute(context);
		
		//yep, after compilation, all we do is call 'execute' on the root, and it does the rest!
	}
}