using System.IO.Pipelines;

namespace PocketknifeCore;

public class PKPipeInputProvider : PKInputProvider
{
	public PipeGenInvoker PipeGenerator => _pipeGenerator;
	private PipeGenInvoker _pipeGenerator;

	public PKPipeInputProvider(Type type, string opName, PipeGenInvoker pipeGenerator, Arguments arguments) : base(type, opName, arguments)
	{
		_pipeGenerator = pipeGenerator;
	}

	public override string ToString()
	{
		return $"PKPipeInputProvider({_name})";
	}
}