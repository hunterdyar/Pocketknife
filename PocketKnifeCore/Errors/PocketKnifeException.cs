namespace PocketKnifeCore;

//just a baseclass so we can catch 'handled vs. unhandled in try/catch.
public class PocketKnifeException : Exception
{
	public PocketKnifeException(string message) : base(message)
	{
		
	}
}