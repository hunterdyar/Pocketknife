using Vellum;
using Vellum.Web;

namespace PocketKnifeDesktop;

public class ContextView : ComponentBase
{
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
			// panel.Label(subtitle, color: panel.Theme.TextSecondary, maxWidth: panel.AvailableWidth, wrap: TextWrapMode.WordWrap);
			// panel.Table(state.TimelineTable, _columns, state, static (table, state) =>
			// {
			// 	if (state.Context == null)
			// 	{
			// 		return;
			// 	}
			// 	foreach (var layer in state.Context.Timeline)
			// 	{
			// 		table.Row(layer, static (row, state) =>
			// 		{
			// 			foreach (var item in state.Items)
			// 			{
			// 				row.Cell(item.ToString() ?? "");
			// 			}
			// 		});
			// 	}
			// }, width: panel.AvailableWidth);
				foreach (var layer in state.Context.Timeline)
				{
					using (panel.Row())
					{
						foreach (var item in layer.Items)
						{
							panel.Label(item.ToString() ?? "");
						}
					}
				}
		});
	}
}