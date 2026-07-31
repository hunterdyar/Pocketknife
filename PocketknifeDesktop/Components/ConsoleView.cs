using Vellum;
using Vellum.Web;

namespace PocketKnifeDesktop;

public class ConsoleView : ComponentBase
{
	private static UiId _consoleScrollID = UiId.FromString("console_scroll");

	public override void Draw(Ui host, AppState state)
	{

		host.Panel(host.AvailableWidth, state, static (panel, state) =>
		{
			using (panel.Row())
			{
				panel.Label("Console", color: panel.Theme.Accent);
				panel.Separator();
				if (panel.Button("Clear", 36).Clicked)
				{
					state.ClearConsole();
				}
			}

			// panel.Label(subtitle, color: panel.Theme.TextSecondary, maxWidth: panel.AvailableWidth, wrap: TextWrapMode.WordWrap);
			using (panel.Row())
			{
				panel.ScrollArea(_consoleScrollID, panel.AvailableWidth, 200,ui =>
				{
					ui.Label(state.Console.ToString());
				});
			}
		});
	}
}