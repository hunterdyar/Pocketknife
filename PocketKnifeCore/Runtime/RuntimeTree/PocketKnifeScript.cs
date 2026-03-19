using System.Text;

namespace PocketKnifeCore;

//The compiled Program lives in here.
public class PocketKnifeScript
{
	public PKInputToOutputBranch RootInputToOutputBranch;
}

public class PKInputToOutputBranch : RuntimeProcess, IProcessCollection
{
	public IPKInputProvider InputProvider => _inputProvider;
	private IPKInputProvider _inputProvider;
	public List<RuntimeProcess> RootBranches;
	public PKInputToOutputBranch()
	{
		
	}

	public void SetProvider(IPKInputProvider inputProvider)
	{
		_inputProvider = inputProvider;
	}

	public void AddProcess(RuntimeProcess rp)
	{
		RootBranches.Add(rp);
	}

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine(_inputProvider.ToString());
		foreach (var runtimeProcess in RootBranches)
		{
			sb.AppendLine(runtimeProcess.ToString());
		}

		return sb.ToString();
	}
}