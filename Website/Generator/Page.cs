using System.Text;
using Stubble.Core.Builders;
using Stubble.Core.Tokens;

namespace Website;

public class Page
{
	public string Template;
	public Dictionary<string, object> Data;

	public Page(string template, Dictionary<string, object> content)
	{
		Template = template;
		Data = content;
	}

	public virtual string Render(RenderData data)
	{
		var stubble = new StubbleBuilder().Build();
		return stubble.Render(data.GetTemplate(Template), Data);
	}
}