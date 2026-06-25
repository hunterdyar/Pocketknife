using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Qt.MetaObject;
using Qt.Quick;

namespace PocketknifeDesktop;

[QObject]
[QmlElement(Name = "Editor", Singleton = true)]
public class EditorModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	private string _text = ">range 0 6\n|mul 2\n:print";
	private int _lineCount = 1;
	private int _executionLine = 0; // 0 = none

	public string Text
	{
		get => _text;
		set
		{
			if (_text == value) return;
			_text = value;
			OnPropertyChanged();
			RecomputeLineCount();
		}
	}

	public int LineCount
	{
		get => _lineCount;
		private set { if (_lineCount == value) return; _lineCount = value; OnPropertyChanged(); }
	}

	public int ExecutionLine
	{
		get => _executionLine;
		set { if (_executionLine == value) return; _executionLine = value; OnPropertyChanged(); }
	}

	public ObservableCollection<GutterMarker> Markers { get; } = new();

	public void ToggleBreakpoint(int line)
	{
		if (line <= 0) return;
		var existing = Markers.FirstOrDefault(m => m.Line == line && m.Kind == GutterMarker.KindBreakpoint);
		if (existing != null)
		{
			Markers.Remove(existing);
		}
		else
		{
			Markers.Add(new GutterMarker { Line = line, Kind = GutterMarker.KindBreakpoint, Tooltip = $"Breakpoint @ line {line}" });
		}
	}

	public bool HasBreakpoint(int line)
		=> Markers.Any(m => m.Line == line && m.Kind == GutterMarker.KindBreakpoint);

	public void SetExecutionArrow(int line)
	{
		//remove any previous execution-arrow marker.
		var prev = Markers.FirstOrDefault(m => m.Kind == GutterMarker.KindExecutionArrow);
		if (prev != null) Markers.Remove(prev);

		ExecutionLine = line > 0 ? line : 0;
		if (line > 0)
		{
			Markers.Add(new GutterMarker { Line = line, Kind = GutterMarker.KindExecutionArrow, Tooltip = "Current step" });
		}
	}

	public void AddErrorMarker(int line, string tooltip)
	{
		if (line <= 0) return;
		Markers.Add(new GutterMarker { Line = line, Kind = GutterMarker.KindError, Tooltip = tooltip });
	}

	public void ClearErrorMarkers()
	{
		var errs = Markers.Where(m => m.Kind == GutterMarker.KindError).ToList();
		foreach (var m in errs) Markers.Remove(m);
	}

	public void ClearAllMarkers()
	{
		Markers.Clear();
		ExecutionLine = 0;
	}

	private void RecomputeLineCount()
	{
		// Count line separators; mirror TextEdit.lineCount semantics
		// (at least 1 line, even for empty text).
		if (string.IsNullOrEmpty(_text)) { LineCount = 1; return; }
		int n = 1;
		for (int i = 0; i < _text.Length; i++) if (_text[i] == '\n') n++;
		LineCount = n;
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
