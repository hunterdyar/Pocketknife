using System.Text;
using Raylib_cs;
using Vellum.Rendering;

namespace Vellum.Web;

public class Program
{

	public static int InitialWindowHeight { get; set; } = 720;
	public static int InitialWindowWidth { get; set; } = 1280;
	
	private static Ui _ui;
	private static RaylibRenderer _renderer;
	private static AppState _state;
	private static bool _initialized;
	public static void Main(string[] args)
	{
		Initialize();
		while (!Raylib.WindowShouldClose())
		{
			UpdateFrame();
		}

		Shutdown();
	}

	private static void Initialize()
	{
		if (_initialized)
		{
			return;
		}

		Raylib.SetTargetFPS(120);
		ConfigFlags flags = ConfigFlags.ResizableWindow;
		if (!OperatingSystem.IsBrowser())
			flags |= ConfigFlags.Msaa4xHint;
		Raylib.SetConfigFlags(flags);
		Raylib.InitWindow(InitialWindowWidth, InitialWindowHeight, "Pocketknife");

		_renderer = new RaylibRenderer();
		_ui = new Ui(_renderer)
		{
			FontStack = UiFont.Merge(UiFont.Source(UiFonts.DefaultSans), UiFont.Source(MaterialSymbols.Font, offsetY: 4f)),
			DefaultFontSize = 18f,
			Lcd = true,
			Platform = new RaylibUiPlatform()
		};
		
		_state = new AppState();
		_initialized = true;
	}


	private static void UpdateFrame()
	{
		if (_ui is null || _renderer is null || !_initialized)
		{
			return;
		}

		_state.UiCpuTimeMs = (float)_ui.LastCpuFrameMs;
		var mp = Raylib.GetMousePosition();
		RenderFrameInfo frame = new RenderFrameInfo(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
		UiInputState input = CollectUiInput();

		_ui.Frame(frame, mp, input, new FrameContext(_state,frame.LogicalHeight), static (root, context) =>
		{
			DrawRoot(root, context);
		});
	}

	private static UiInputState CollectUiInput()
	{
		var keys = new HashSet<UiKey>();
    var mouseButtons = new HashSet<UiMouseButton>();
    AddKey(keys, UiKey.Left, KeyboardKey.Left);
    AddKey(keys, UiKey.Right, KeyboardKey.Right);
    AddKey(keys, UiKey.Up, KeyboardKey.Up);
    AddKey(keys, UiKey.Down, KeyboardKey.Down);
    AddKey(keys, UiKey.Home, KeyboardKey.Home);
    AddKey(keys, UiKey.End, KeyboardKey.End);
    AddKey(keys, UiKey.Tab, KeyboardKey.Tab);
    AddKey(keys, UiKey.Enter, KeyboardKey.Enter);
    AddKey(keys, UiKey.Escape, KeyboardKey.Escape);
    AddKey(keys, UiKey.Space, KeyboardKey.Space);
    AddKey(keys, UiKey.Backspace, KeyboardKey.Backspace);
    AddKey(keys, UiKey.Delete, KeyboardKey.Delete);
    AddKey(keys, UiKey.A, KeyboardKey.A);
    AddKey(keys, UiKey.C, KeyboardKey.C);
    AddKey(keys, UiKey.V, KeyboardKey.V);
    AddKey(keys, UiKey.X, KeyboardKey.X);

    string textInput = CollectTextInput();
    bool shift = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);
    bool ctrl = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);
    bool alt = Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt);
    System.Numerics.Vector2 wheelDelta;
   
    wheelDelta = new System.Numerics.Vector2(0, Raylib.GetMouseWheelMove());
    AddMouseButton(mouseButtons, UiMouseButton.Left, MouseButton.Left);
    AddMouseButton(mouseButtons, UiMouseButton.Right, MouseButton.Right);
    AddMouseButton(mouseButtons, UiMouseButton.Middle, MouseButton.Middle);
    
    return new UiInputState(
        textInput,
        keys.Count > 0 ? keys : null,
        wheelDelta,
        shift,
        ctrl,
        alt,
        meta: false,
        downMouseButtons: mouseButtons.Count > 0 ? mouseButtons : null,
        timeSeconds: Raylib.GetTime());
	}

	static void AddKey(HashSet<UiKey> keys, UiKey uiKey, KeyboardKey raylibKey)
	{
		if (Raylib.IsKeyPressed(raylibKey) || Raylib.IsKeyPressedRepeat(raylibKey))
		{
			keys.Add(uiKey);
		}
	}

	static void AddMouseButton(HashSet<UiMouseButton> buttons, UiMouseButton uiButton, MouseButton raylibButton)
	{
		if (Raylib.IsMouseButtonDown(raylibButton))
		{
			buttons.Add(uiButton);
		}
	}

	static string CollectTextInput()
	{
		var builder = new StringBuilder();

		while (true)
		{
			int codepoint = Raylib.GetCharPressed();
			if (codepoint == 0) break;
			builder.Append(char.ConvertFromUtf32(codepoint));
		}

		return builder.ToString();
	}

	private static void DrawRoot(Ui root, FrameContext context)
	{
		root.FillViewport(root.Theme.SurfaceBg);
		// using (root.MaxWidth(1040, UiAlign.Center))
		// {
		// }
		
		DrawMenuBar(root, context.State);
	}

	static Response DrawMenuBar(Ui host, AppState state)
	{
		 return host.MenuBar(host.AvailableWidth, state, static (bar, state) =>
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
		       Shutdown();
	        }

            
        }, popupWidth: 260f);

        bar.Menu("Edit", state, static (menu, state) =>
        {

          
        });

        bar.Menu("View", state, static (menu, state) =>
        {
            
        });
    });
	}

	private static void Shutdown()
	{
		_ui?.Dispose();
		_renderer?.Shutdown();
		_ui = null;
		_renderer = null;

		if (_initialized)
		{
			Raylib.CloseWindow();
		}

		_initialized = false;
	}

	
}