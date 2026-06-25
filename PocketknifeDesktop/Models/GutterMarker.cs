using System.ComponentModel;
using System.Runtime.CompilerServices;
using Qt.MetaObject;
using Qt.Quick;

namespace PocketknifeDesktop;

[QObject]
[QmlElement(Name = "GutterMarker")]
public class GutterMarker : INotifyPropertyChanged
{
	public const string KindBreakpoint = "breakpoint";
	public const string KindExecutionArrow = "executionArrow";
	public const string KindError = "error";

	public event PropertyChangedEventHandler? PropertyChanged;

	private int _line;
	private string _kind = KindBreakpoint;
	private string _tooltip = "";

	public int Line
	{
		get => _line;
		set { if (_line == value) return; _line = value; OnPropertyChanged(); }
	}

	public string Kind
	{
		get => _kind;
		set { if (_kind == value) return; _kind = value; OnPropertyChanged(); }
	}

	public string Tooltip
	{
		get => _tooltip;
		set { if (_tooltip == value) return; _tooltip = value; OnPropertyChanged(); }
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
