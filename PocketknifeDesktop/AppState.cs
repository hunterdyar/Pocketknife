using PocketknifeCore;
using PocketknifeCore.Compiler;
using PocketknifeCore.SimpleEvaluator;

namespace Vellum.Web;

readonly record struct FrameContext(AppState State);
public class AppState
{
	public PocketKnifeDesktop.Application Application { get; set; } = new PocketKnifeDesktop.Application();
	public Context? Context => _context;
	private Context? _context = null;
	
	public OpCatalog OpCatalog => _opCatalog;
	private OpCatalog _opCatalog;
	public Compiler Compiler => _compiler;
	private Compiler _compiler;
	public Parser Parser => _parser;
	private Parser _parser = new Parser();
	
	public Lexer? Lexer => _lexer;
	private Lexer? _lexer = null;

	//compiled code root
	public ScriptNode? Script => _script;
	private ScriptNode? _script;

	public PKNode? Compiled => _compiled;
	private PKNode? _compiled;
	
	public LineEvaluator LineEvaluator => _lineEvaluator;
	private LineEvaluator _lineEvaluator = new LineEvaluator();
	public string Code = """
	                     >range 0 6
	                     |mul 2
	                     :print
	                     """;
	public float UiCpuTimeMs { get; set; }

	public AppState()
	{
		_opCatalog = OpCatalog.GetDefaultOpCatalog();
		_compiler = new Compiler(_opCatalog);
	}
	public void RecompileIfNeeded()
	{
		if(_lexer == null || Code != _lexer.Source)
		{
			_lexer = new Lexer(Code);
			_parser.Parse(_lexer);
			_script = _parser.Program;
			var ctx = new CompileContext();
			_compiled = Compiler.Compile(_script, ctx);
			_lineEvaluator.SetRoot(_compiled);
			_context = new Context();
		}
	}
}