namespace Website;

class Program
{
	public static RenderData RenderData;
	public static Site Site;
	public static string OutputDir;
	
	
	static void Main(string[] args)
	{
		if (args.Length > 0)
		{
			OutputDir = args[0];
		}
		else
		{
			OutputDir = Environment.CurrentDirectory + "/site/";
		}

		if (!Directory.Exists(OutputDir))
		{
			Directory.CreateDirectory(OutputDir);
		}
		else
		{
			//clear content
			Directory.Delete(OutputDir, true);
			Directory.CreateDirectory(OutputDir);
		}
		
		LoadRenderData();
		//load/check server...
		LoadSiteData();
		RenderSite();
	}


	private static void LoadRenderData()
	{
		RenderData = new RenderData();
		DirectoryInfo templateDir = new DirectoryInfo(Environment.CurrentDirectory + "/Resources/Templates/");
		if (!templateDir.Exists)
		{
			throw new Exception("Template Directory Not Found");
		}

		foreach (var file in templateDir.EnumerateFiles("*.mustache"))
		{
			RenderData.LoadTemplate(file.FullName);
		}
		
	}

	private static void LoadSiteData()
	{
		Site = new Site();
		Site.AddPage(new Page("home", new Dictionary<string, object>() { }), "/");
	}


	private static void RenderSite()
	{
		Site.RenderFullSite(RenderData, OutputDir);
	}
}