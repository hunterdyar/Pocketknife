using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

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
			//Directory.Delete(OutputDir, true);
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
		LoadMarkdownPages();
	}

	private static void LoadMarkdownPages()
	{
		var mdpipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseYamlFrontMatter().Build();
		DirectoryInfo contentDir = new DirectoryInfo(Environment.CurrentDirectory + "/Resources/Content/");
		if (!contentDir.Exists)
		{
			throw new Exception("No content directory found!");
		}

		foreach (var file in contentDir.EnumerateFiles("*.md", new EnumerationOptions()
		         {
			         RecurseSubdirectories = true
		         }))
		{
			using (StreamReader streamReader = new StreamReader(file.FullName))
			{
				var mdContent = streamReader.ReadToEnd();
				var document = Markdown.Parse(mdContent, mdpipeline);
				var yaml = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
				string template = "page";//default template.
				if (yaml != null)
				{
					string yamlText = mdContent.Substring(yaml.Span.Start, yaml.Span.Length);
					//parse yaml, identify template, set it.
				}

				var relPath = Path.GetRelativePath(contentDir.FullName, file.FullName);
				
				Site.AddPage(new Page(template, new Dictionary<string, object>()
				{
					{"content", document.ToHtml()},
				}), relPath);
			}
		}
	}


	private static void RenderSite()
	{
		Site.RenderFullSite(RenderData, OutputDir);
	}
}