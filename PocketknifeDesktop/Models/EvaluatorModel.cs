using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using PocketKnife.Compiler;
using PocketknifeCore;
using PocketknifeCore.SimpleEvaluator;
using Qt.MetaObject;
using Qt.Quick;

namespace PocketknifeDesktop;

[QObject]
[QmlElement(Name = "Evaluator")]
public class EvaluatorModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	private readonly OpCatalog _catalog = OpCatalog.GetDefaultOpCatalog();

	private string _consoleOutput = "";
	private string _errorsOutput = "No errors.";
	private bool _isRunning;
	private int _stepCount;
	private QmlAstNode _root = new("(no program)");

	// Step-by-step state.
	private IEnumerator<EvalState>? _stepIter;
	private Context? _context;
	private readonly Stack<string> _consoleHistory = new();

	public EvaluatorModel(EngineModel engineModel)
	{
	}
	public string ConsoleOutput
	{
		get => _consoleOutput;
		private set { if (_consoleOutput == value) return; _consoleOutput = value; OnPropertyChanged(); }
	}

	public string ErrorsOutput
	{
		get => _errorsOutput;
		private set { if (_errorsOutput == value) return; _errorsOutput = value; OnPropertyChanged(); }
	}

	public bool IsRunning
	{
		get => _isRunning;
		private set { if (_isRunning == value) return; _isRunning = value; OnPropertyChanged(); }
	}

	public int StepCount
	{
		get => _stepCount;
		private set { if (_stepCount == value) return; _stepCount = value; OnPropertyChanged(); }
	}

	public QmlAstNode Root
	{
		get => _root;
		private set { _root = value; OnPropertyChanged(); }
	}

	//reset context and run from the top.
	public void Run(string source)
	{
		ClearOutputs();
		if (!TryParseAndCompile(source, out var program)) return;

		IsRunning = true;
		try
		{
			_context = new Context();
			CaptureConsoleWhile(() => SimpleEvaluator.EvaluateAll(program!, _context));
		}
		catch (Exception ex)
		{
			ReportError(ex);
		}
		finally
		{
			IsRunning = false;
			_stepIter = null;
		}
	}
	
	public void Step(string source)
	{
		if (_stepIter == null)
		{
			ClearOutputs();
			if (!TryParseAndCompile(source, out var program)) return;
			_context = new Context();
			_stepIter = SimpleEvaluator.Evaluate(program!, _context).GetEnumerator();
		}

		try
		{
			CaptureConsoleWhile(() =>
			{
				if (_stepIter!.MoveNext())
				{
					StepCount++;
					_consoleHistory.Push(ConsoleOutput);
				}
				else
				{
					// Finished — clear stepping state.
					_stepIter = null;
					_context = null;
				}
			});
		}
		catch (Exception ex)
		{
			ReportError(ex);
			_stepIter = null;
			_context = null;
		}
	}
	
	public void Undo()
	{
		if (_consoleHistory.Count == 0) return;
		_consoleHistory.Pop(); // discard current
		ConsoleOutput = _consoleHistory.Count > 0 ? _consoleHistory.Peek() : "";
		if (StepCount > 0) StepCount--;
	}

	public void Reset()
	{
		_stepIter = null;
		_context = null;
		_consoleHistory.Clear();
		StepCount = 0;
		ClearOutputs();
		Root = new QmlAstNode("(no program)");
	}

	//helpers

	private bool TryParseAndCompile(string source, out PKNode? program)
	{
		program = null;
		try
		{
			var parser = new Parser();
			parser.Parse(source ?? "");
			Root = BuildAstTree(parser.Program);

			var compiler = new PocketknifeCore.Compiler.Compiler(_catalog);
			program = compiler.StartCompile(parser.Program);
			return true;
		}
		catch (PocketknifeException ex)
		{
			ReportError(ex);
			return false;
		}
		catch (Exception ex)
		{
			ReportError(ex);
			return false;
		}
	}

	private void ClearOutputs()
	{
		ConsoleOutput = "";
		ErrorsOutput = "No errors.";
	}

	private void ReportError(Exception ex)
	{
		ErrorsOutput = ex.GetType().Name + ": " + ex.Message;
	}
	
	private void CaptureConsoleWhile(Action action)
	{
		var prev = Console.Out;
		var sw = new StringWriter();
		Console.SetOut(sw);
		try { action(); }
		finally { Console.SetOut(prev); }

		var captured = sw.ToString();
		if (!string.IsNullOrEmpty(captured))
		{
			ConsoleOutput = ConsoleOutput + captured;
		}
	}

	// --- AST → QML tree conversion -----------------------------------------------

	private static QmlAstNode BuildAstTree(ASTNode? node)
	{
		var root = new QmlAstNode("Script");
		if (node != null) Visit(node, root);
		return root;
	}

	private static void Visit(ASTNode node, QmlAstNode parent)
	{
		var label = node.GetType().Name;
		var detail = SafeToString(node);
		var qmlNode = new QmlAstNode(label, detail);
		parent.AddChild(qmlNode);

		// Recurse into known container shapes. This is intentionally shallow:
		// it just makes the tree useful in the AST tab without coupling tightly
		// to every node type.
		switch (node)
		{
			case ScriptNode s:
				foreach (var c in s.RootNodes) Visit(c, qmlNode);
				break;
			case CommandSetNode cs:
				foreach (var c in cs.Commands) Visit(c, qmlNode);
				break;
			case InputBranchNode ib:
				Visit(ib.Input, qmlNode);
				Visit(ib.CommandSet, qmlNode);
				break;
			case BranchNode b:
				Visit(b.Commands, qmlNode);
				break;
			case PatternMatch pm:
				foreach (var arm in pm.Arms) Visit(arm, qmlNode);
				break;
			case PatternBranchArm arm:
				if (arm.FilterToMatch != null) Visit(arm.FilterToMatch, qmlNode);
				Visit(arm.Commands, qmlNode);
				break;
		}
	}

	private static string SafeToString(ASTNode node)
	{
		try { return (node.ToString() ?? "").Trim(); }
		catch { return ""; }
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
