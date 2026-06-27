using Vellum;
using Vellum.Web;

namespace PocketKnifeDesktop;

public class MenuBar : ComponentBase
{
	public override void Draw(Ui host, AppState state)
	{
		host.MenuBar(host.AvailableWidth, state, static (bar, state) =>
			{
				bar.Menu("File", state, static (menu, state) =>
				{
					if (menu.MenuItem("open", closeOnActivate: true, shortcut: "Ctrl+O").Clicked)
					{
						//
					}

					if (menu.MenuItem("save", closeOnActivate: true, shortcut: "Ctrl+O").Clicked)
					{
						//
					}

					menu.MenuSeparator();

					if (menu.MenuItem("exit", closeOnActivate: true, shortcut: "Ctrl+Q").Clicked)
					{
						Program.Shutdown();
					}


				}, popupWidth: 260f);

				bar.Menu("Edit", state, static (menu, state) => { });

				bar.Menu("View", state, static (menu, state) => { });
			});
	}
}