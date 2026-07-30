using System.Text;
using Stubble.Core;
using Stubble.Core.Builders;
using Stubble.Core.Loaders;

namespace Website;

public class RenderData
{
	public readonly Dictionary<string, string> Templates = new Dictionary<string, string>();

	//lazy getter
	public StubbleVisitorRenderer? Builder
	{
		get
		{
			if (_builder == null)
			{
				BuildRenderer();
			}

			return _builder;
		}
	}

	private StubbleVisitorRenderer? _builder;
	public void BuildRenderer()
	{
		_builder = new StubbleBuilder().Configure(settings =>
		{
			// settings.SetIgnoreCaseOnKeyLookup(true);
			settings.AddToPartialTemplateLoader(new DictionaryLoader(Templates));
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