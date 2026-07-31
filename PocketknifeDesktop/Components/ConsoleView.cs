using Vellum;
using Vellum.Web;

namespace PocketKnifeDesktop;

public class ConsoleView :ComponentBase
{
	public override void Draw(Ui host, AppState state)
	{
		host.Panel(host.AvailableWidth, state, static (panel, state) =>
		{
			panel.Label("Editor", color: panel.Theme.Accent);
			// panel.Label(subtitle, color: panel.Theme.TextSecondary, maxWidth: panel.AvailableWidth, wrap: TextWrapMode.WordWrap);
			using (panel.Row())
			{
				panel.TextArea("Code", ref state.Code, panel.AvailableWidth, 123, placeholder: "");
				panel.Label(state.Console.ToString());
			}
		});
	}
}