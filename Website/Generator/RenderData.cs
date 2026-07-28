using System.Text;
using Stubble.Core;
using Stubble.Core.Builders;

namespace Website;

public class RenderData
{
	public Dictionary<string, string> Templates = new Dictionary<string, string>();

	public StubbleVisitorRenderer Builder;
	public RenderData()
	{
		Builder = new StubbleBuilder().Configure(settings =>
		{
			// settings.SetIgnoreCaseOnKeyLookup(true);
			
		}).Build();
	}
	public void LoadTemplate(string path)
	{
		var name = Path.GetFileNameWithoutExtension(path);
		if (Templates.ContainsKey(name))
		{
			throw new Exception($"Template with name {name} already exists. Sorry no directory support for templates.");
		}
		
		using (StreamReader streamReader = new StreamReader(path))
		{
			var content = streamReader.ReadToEnd();
			Templates.Add(name, content);
		}
	}

	public string GetTemplate(string template)
	{
		if (Templates.TryGetValue(template, out string value))
		{
			return value;
		}

		throw new Exception($"template {template} not found!");
	}
}