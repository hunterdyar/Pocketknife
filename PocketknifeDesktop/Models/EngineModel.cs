using System.ComponentModel;
using System.Runtime.CompilerServices;
using Qt.MetaObject;
using Qt.Quick;

namespace PocketknifeDesktop;

[QObject]
[QmlElement(Name = "Engine", Singleton = true)]
public class EngineModel : INotifyPropertyChanged
{
	public EvaluatorModel Evaluator;
	public EditorModel Editor;

	public EngineModel()
	{
		Evaluator = new EvaluatorModel(this);
		Editor = new EditorModel(this);
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Evaluator)));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Editor)));
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	
}