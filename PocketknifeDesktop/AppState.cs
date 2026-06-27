using PocketKnife.Compiler;
using PocketknifeCore;
using PocketknifeCore.SimpleEvaluator;

namespace Vellum.Web;

readonly record struct FrameContext(AppState State);
public class AppState
{
	public PocketKnifeDesktop.Application Application { get; set; } = new PocketKnifeDesktop.Application();
	public Context? Context => _context;
	private Context? _context = null;
	public Parser? Parser => _parser;
	private Parser? _parser = null;

	public string Code = """
	                     >range 0 6
	                     |mul 2
	                     :print
	                     """;
	public float UiCpuTimeMs { get; set; }
}