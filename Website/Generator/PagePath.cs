namespace Website;

public struct PagePath
{
	public string Path;
	public Page Page;

	public string FileName()
	{
		if (Path.EndsWith("/"))
		{
			return "index";
		}
		else
		{
			return System.IO.Path.GetFileNameWithoutExtension(Path);
		}
	}

	public string Directory()
	{
		var i = Path.LastIndexOf("/", StringComparison.Ordinal);
		string p = i <= 0 ? "/" : Path.Substring(0, i);
		if (!p.StartsWith("/"))
		{
			return "/" + p;
		}
		else
		{
			return p;
		}
	}
}