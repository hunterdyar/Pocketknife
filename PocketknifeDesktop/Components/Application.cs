using System.ComponentModel;
using Vellum;
using Vellum.Web;

namespace PocketKnifeDesktop;

public class Application : ComponentBase
{
	private MenuBar _menuBar = new MenuBar();
	private EvalToolbar _evalToolbar = new EvalToolbar();
	private CodeEditor _codeEditor = new CodeEditor();

	public override void Draw(Ui ui, AppState state)
	{
		_menuBar.Draw(ui, state);
		_evalToolbar.Draw(ui, state);
		_codeEditor.Draw(ui, state);
	}
}