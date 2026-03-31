namespace PocketKnifeCore;

public class InternalException : Exception
{
	public InternalException(string message) : base(message)
	{

	}

	public override string Message => "ya boi done goofed! " + base.Message;
}