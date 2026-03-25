namespace PocketKnifeCore.Engine;

public class Saver
{
	public Action<FileStream, PKItem> Writer;
	public string DefaultExtension;
	public Type? OnlyValidOn;
	public string Name;

	public void Execute(Context context, PKItem[] args)
	{
		throw new NotImplementedException();
		var item = context.Item;
		//arg 0 will be the type of the saver (csv, text, etc)
		//args 1....^1 should be a combined directory (optional)
		//the last argument should be the file name to save as. Extension is optional, if not provided, will use defaultExtension.

		if (args.Length == 1)
		{
			//we have been given nothing! uh oh.
		}

		// var filename = args[^1];
		FileInfo fi = new FileInfo("");
		using (var fs = fi.OpenWrite())
		{
			Writer.Invoke(fs, item);
		}
		
	}
}