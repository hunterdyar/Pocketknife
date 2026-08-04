using Vellum;
using Vellum.Web;

namespace PocketKnifeDesktop;

public class CodeEditor : ComponentBase
{
	public override void Draw(Ui host, AppState state)
	{
		host.Panel(host.AvailableWidth, state, static (panel, state) =>
		{
			panel.Label("Editor", color: panel.Theme.Accent);
			// panel.Label(subtitle, color: panel.Theme.TextSecondary, maxWidth: panel.AvailableWidth, wrap: TextWrapMode.WordWrap);
			using (panel.Row())
			{
				using (panel.Column())
				{
					//hacky temporary alignment
					panel.Spacing(10);
					for (int i = 0; i < 540/10/2; i++)
					{
						if (state.LineEvaluator.Current.Evaluated.Line == i && state.State == "running...")
						{
							panel.Label($"({(i+1).ToString()})", maxLines: 1, align: UiAlign.End, size: 11);
						}
						else
						{
							panel.Label((i + 1).ToString(), maxLines: 1, align: UiAlign.End, size: 11);
						}
					}
				}
				var r = panel.TextArea("Code", ref state.Code, panel.AvailableWidth, 540, placeholder: "");
			}
		});
	}
}