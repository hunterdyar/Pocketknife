using System.ComponentModel;
using Vellum;
using Vellum.Web;

namespace PocketKnifeDesktop;

public class Application : ComponentBase
{
	private MenuBar _menuBar = new MenuBar();
	private EvalToolbar _evalToolbar = new EvalToolbar();
	private CodeEditor _codeEditor = new CodeEditor();
	private ConsoleView _consoleView = new ConsoleView();
	private ContextView _contextView = new ContextView();
	public override void Draw(Ui ui, AppState state)
	{
		_menuBar.Draw(ui, state);
		_evalToolbar.Draw(ui, state);
		_codeEditor.Draw(ui, state);
		_consoleView.Draw(ui, state);
		_contextView.Draw(ui, state);

	}
}