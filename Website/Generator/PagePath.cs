namespace Website;

public struct PagePath
{
	public string Path;
	public Page Page;

	public string FileName()
	{
		var i = Path.LastIndexOf("/", StringComparison.Ordinal);
		string p = i <= 0 ? "/" : Path.Substring(i, Path.Length-i);
		
		if (!p.EndsWith("/"))
		{
			return p;
		}
		else
		{
			return "index";
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