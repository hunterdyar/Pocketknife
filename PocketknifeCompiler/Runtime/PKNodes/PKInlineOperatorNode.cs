using PocketknifeCore.Compiler;

namespace PocketknifeCore;

public class PKInlineOperatorNode : PKNode
{
	public OpInvoker Invoker => _invoker;
	private OpInvoker _invoker;

	public string Name => _name;
	private string _name;

	public Arguments Arguments => _arguments;
	private Arguments _arguments;

	public PKInlineOperatorNode(string name, OpInvoker invoker, Arguments arguments, SourceSlice span) : base(span)
	{
		_name = name;
		_invoker = invoker;
		_arguments = arguments;
	}
}

public class PKFilterOperatorNode : PKInlineOperatorNode
{
	public PKFilterOperatorNode(string name, OpInvoker invoker, Arguments arguments, SourceSlice span) : base(name, invoker, arguments, span)
	{
	}
}

public class PKSignalOperatorNode : PKInlineOperatorNode
{
	public PKSignalOperatorNode(string name, OpInvoker invoker, Arguments arguments, SourceSlice span) : base(name, invoker, arguments, span)
	{
	}
}