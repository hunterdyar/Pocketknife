namespace PocketKnife.Compiler;

//just a baseclass so we can catch 'handled vs. unhandled in try/catch.
public class PocketknifeException : Exception
{
	public PocketknifeException(string message) : base(message)
	{
		
	}
}