using System.Text;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using PocketknifeCore;
using YamlDotNet.Serialization;

namespace Website;

class Program
{
	public static RenderData RenderData;
	public static Site Site;
	public static string OutputDir;
	public static string StaticDir => Environment.CurrentDirectory + "/Resources/Static/";
	public static string TemplateDir => Environment.CurrentDirectory + "/Resources/Templates/";
	public static string ContentDir => Environment.CurrentDirectory + "/Resources/Content/";
	public static string ConfigFile => Environment.CurrentDirectory + "/Resources/Content/config.yaml";

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
			try
			{
				Directory.Delete(OutputDir, true);
			}
			catch (Exception e)
			{
				Console.WriteLine("can't delete the existing folder. I dunno why, ignoring.");
			}

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
		DirectoryInfo templateDir = new DirectoryInfo(TemplateDir);
		if (!templateDir.Exists)
		{
			throw new Exception($"Template Directory Not Found: {TemplateDir}");
		}

		foreach (var file in templateDir.EnumerateFiles("*.mustache"))
		{
			RenderData.LoadTemplate(file.FullName);
		}
		
	}

	private static void LoadSiteData()
	{
		Site = new Site();
		LoadConfig();
		LoadMarkdownPages();
		LoadPocketKnifeFeatures();
	}

	private static void LoadConfig()
	{
		var configInfo = new FileInfo(ConfigFile);
		if (!configInfo.Exists)
		{
			return;
		}
		//todo: make static readonly
		var yamlDeserializer = new DeserializerBuilder().Build();

		using (StreamReader stream = new StreamReader(configInfo.FullName))
		{
			var configYAML = stream.ReadToEnd();
			var configData = yamlDeserializer.Deserialize(configYAML);
			Site.SiteData.Add("config",configData!);
		}
	}

	private static void LoadMarkdownPages()
	{
		var mdpipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseYamlFrontMatter().Build();
		var yamlDeserializer = new DeserializerBuilder().Build();
		
		DirectoryInfo contentDir = new DirectoryInfo(ContentDir);
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
				var code = document.Descendants<CodeBlock>().ToList();
				var indented = document.Descendants<CodeInline>().ToList();

				//add a class to the inline code blocks vs. the block ones.
				for (int i = 0; i < indented.Count; i++)
				{
					indented[i].SetAttributes(new HtmlAttributes()
					{
						Classes = new List<string>(["inline-code-block"]),
					});
				}
				
				var yaml = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
				string template = "main";//default template.
				object? data = null;
				if (yaml != null)
				{
					string yamlText = mdContent.Substring(yaml.Span.Start, yaml.Span.Length);
					data = yamlDeserializer.Deserialize(yamlText);
				}
				
				var relPath = Path.GetRelativePath(contentDir.FullName, file.FullName);
				
				Site.AddPage(new Page(template, new Dictionary<string, object>()
				{
					{"content", document.ToHtml()},
					{"page", data!}
				}), relPath);
			}
		}
	}

	private static void LoadPocketKnifeFeatures()
	{
		var catalog = OpCatalog.GetDefaultOpCatalog();
		Dictionary<string, Dictionary<string, object>> opContent = new Dictionary<string, Dictionary<string, object>>();
		foreach (var op in catalog.Operators.Values)
		{
			var content = new Dictionary<string, object>();
			var name = op.Name;
			var overloadDescriptions = op.Overloads;
			var descs = new List<Dictionary<string, string>>();
			foreach (var description in overloadDescriptions)
			{
				var overloadData = new Dictionary<string, string>();
				overloadData.Add("argCount", description.ArgCount.ToString());
				overloadData.Add("inType", description.InType.ToString());
				overloadData.Add("outType", description.OutType.ToString());
				overloadData.Add("kind",description.OpKind.ToString());
				overloadData.Add("pretty", GetPrettyDescription(name,description));
				// overloadData.Add("",description.Method.);
				descs.Add(overloadData);
			}
			
			content.Add("overloads", descs);
			content.Add("name", name);
			if (!opContent.TryAdd(name, content))
			{
				throw new Exception($"duplicate op {name}?");
			}
			Site.AddPage(new Page("operator", content), "ops/"+name);
		}

		//index data to navigate to pages and such.
		Site.SiteData.Add("ops", opContent);
	}

	private static string GetPrettyDescription(string name, OperatorDescription description)
	{
		StringBuilder sb = new StringBuilder();
		
		//inType
		if (description.InType != typeof(void))
		{
			sb.Append("> " + description.InType.ToString() + "<br />");
		}

		switch (description.OpKind)
		{
			case OpKind.Filter:
				sb.Append('~');
				break;
			case OpKind.Generator:
				sb.Append('>');
				break;
			case OpKind.PipeIn:
				sb.Append("|>");
				break;
			case OpKind.Pipeline:
				sb.Append('|');
				break;
			case OpKind.Signal:
				sb.Append(':');
				break;
		}

		sb.Append("<strong>");
		sb.Append(name);
		sb.Append("</strong>");
		sb.Append(' ');
		if (description.ArgCount > 0)
		{
			sb.Append('(');
			var paramy = description.Method.GetParameters();
			for (var i = 0; i < paramy.Length; i++)
			{
				var pi = description.Method.GetParameters()[i];
				sb.Append('[');
				sb.Append(pi.ParameterType.Name);
				sb.Append(' ');
				sb.Append("<strong>");
				sb.Append(pi.Name.ToString());
				sb.Append("</strong>");

				sb.Append(']');
				if (i < paramy.Length - 1)
				{
					sb.Append(' ');
				}
			}

			sb.Append(") ");
		}

		if (description.OutType != typeof(void))
		{
			sb.Append("<br />< " + description.OutType.ToString());
		}

		return sb.ToString();
	}


	private static void RenderSite()
	{
		Directory.CreateDirectory(OutputDir);

		//copy static data
		if (Directory.Exists(StaticDir))
		{
			Helpers.CopyDirectory(StaticDir, OutputDir, true);
		}

		//render the rest of the site.
		Site.RenderFullSite(RenderData, OutputDir);
	}
}