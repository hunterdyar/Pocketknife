using System.IO.Pipelines;

namespace PocketknifeCore;

public class PKPipeInputProvider : PKInputProvider
{
	public PipeGenInvoker PipeGenerator => _pipeGenerator;
	private PipeGenInvoker _pipeGenerator;

	public PKPipeInputProvider(Type type, string opName, PipeGenInvoker pipeGenerator, Arguments arguments, SourceSlice span) : base(type, opName, arguments, span)
	{
		_pipeGenerator = pipeGenerator;
	}

	public override string ToString()
	{
		return $"PKPipeInputProvider({_name})";
	}
}