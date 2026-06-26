using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Qt.MetaObject;
using Qt.Quick;

namespace PocketknifeDesktop;

[QObject]
[QmlElement(Name = "AstNode")]
public class QmlAstNode : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	private string _label = "";
	private string _detail = "";
	private bool _expanded = true;

	public QmlAstNode() { }

	public QmlAstNode(string label, string detail = "")
	{
		_label = label;
		_detail = detail;
	}

	public string Label
	{
		get => _label;
		set { if (_label == value) return; _label = value; OnPropertyChanged(); }
	}

	public string Detail
	{
		get => _detail;
		set { if (_detail == value) return; _detail = value; OnPropertyChanged(); }
	}

	public bool Expanded
	{
		get => _expanded;
		set { if (_expanded == value) return; _expanded = value; OnPropertyChanged(); }
	}

	public ObservableCollection<QmlAstNode> Children { get; } = new();

	public bool HasChildren => Children.Count > 0;

	public void AddChild(QmlAstNode child)
	{
		Children.Add(child);
		OnPropertyChanged(nameof(HasChildren));
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
