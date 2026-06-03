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

	public void Execute(PocketKnifeScript script, PKString input)
	{
		var context = new Context(input);
		script.Execute(context);
	}

	public void Execute(PocketKnifeScript script, PKString[] input)
	{
		for (int i = 0; i < input.Length; i++)
		{
			var context = new Context(input[i]);
			script.Execute(context);
		}
	}
}