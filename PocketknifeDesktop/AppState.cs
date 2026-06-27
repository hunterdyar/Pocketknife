using PocketKnife.Compiler;
using PocketknifeCore.SimpleEvaluator;

namespace Vellum.Web;

readonly record struct FrameContext(AppState State, int ScreenHeight);
public class AppState
{
	private Parser _parser;
	public float UiCpuTimeMs { get; set; }
}