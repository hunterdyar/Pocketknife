using Vellum;
using Vellum.Web;

namespace PocketKnifeDesktop;

public class EvalToolbar : ComponentBase
{
	private const float _buttonWidth = 48;
	public override void Draw(Ui host, AppState state)
	{
		host.Panel(host.AvailableWidth, state, static (panel, state) =>
		{
			// panel.Label(subtitle, color: panel.Theme.TextSecondary, maxWidth: panel.AvailableWidth, wrap: TextWrapMode.WordWrap);
			using (panel.Row())
			{
				if (panel.Button($"{MaterialSymbols.PlayArrow}", width: _buttonWidth).Clicked)
				{
					state.RecompileIfNeeded();
					state.LineEvaluator.RunCurrentToEnd();
				}

				if (panel.Button($"{MaterialSymbols.StepInto}", width: _buttonWidth).Clicked)
				{
					state.RecompileIfNeeded();
					state.LineEvaluator.Step();
				}

				if (panel.Button($"{MaterialSymbols.StepOut}", width: _buttonWidth).Clicked)
				{
					//step
				}

				if (panel.Button($"{MaterialSymbols.Refresh}", width: _buttonWidth).Clicked)
				{
					//reset
					//state.RecompileIfNeeded();
					state.LineEvaluator.Reset();
				}
			}
		});
	}
}