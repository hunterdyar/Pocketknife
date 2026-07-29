namespace Website;

public class Site
{
	public List<PagePath> Pages = new List<PagePath>();

	public void AddPage(Page page, string path)
	{
		Pages.Add(new PagePath()
		{
			Page = page,
			Path = path,
		});
	}


	public void RenderFullSite(RenderData data, string outputDir)
	{
		foreach (var pagePath in Pages)
		{
			var dir = outputDir+pagePath.Directory();
			var name = pagePath.FileName();
			var fullPath = dir + "/"+name + ".html";
			var output = pagePath.Page.Render(data);

			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			using (var sw = new StreamWriter(fullPath))
			{
				sw.Write(output);
			}
		}
		
	}
}