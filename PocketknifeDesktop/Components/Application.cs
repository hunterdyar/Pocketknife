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
		using (ui.Row())
		{
			float rowWidth = MathF.Max(0, ui.AvailableWidth);
			float editorWidth = MathF.Min(460f, MathF.Max(236f, rowWidth * 0.32f));
			
			using (ui.FixedWidth(editorWidth))
			{
				_codeEditor.Draw(ui, state);
				_consoleView.Draw(ui, state);
			}
			using (ui.FixedWidth(MathF.Max(0, ui.AvailableWidth)))
			{
				_contextView.Draw(ui, state);
			}
		}

		

	}
}