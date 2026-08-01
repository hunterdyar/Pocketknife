using System.Globalization;
using PocketknifeCore;
using Vellum;
using Vellum.Web;

namespace PocketKnifeDesktop;

public class ContextView : ComponentBase
{
	private static UiId _contextScrollID = UiId.FromString("console_scroll");
	private static UiId _tableID = UiId.FromString("table_id");

	private static readonly TableColumn[] _columns =
	[
		new("1"),
		new("2"),
		new("3"),
		new("4"),
		new("5", 92f, UiAlign.End)
	];
	public override void Draw(Ui host, AppState state)
	{
		host.Panel(host.AvailableWidth, state, static (panel, state) =>
		{
			panel.Label("Context", color: panel.Theme.Accent);
			panel.Label(state.State);

			panel.ScrollArea(_contextScrollID, panel.AvailableWidth, 200,ui =>
			{
				if (state.Context == null)
				{
					return;
				}
				
				using (ui.Row())
				{
					foreach (var layer in state.Context.Timeline)
					{
						using (ui.Column())
						{
							ui.Label("__");
							foreach (var item in layer.Items)
							{
								ui.Label(item.Value?.ToString() ?? "");
							}
						}
					}
				}
			});
		});
	}
}