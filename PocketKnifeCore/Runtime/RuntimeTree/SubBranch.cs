namespace PocketKnifeCore;

public class SubBranch : RuntimeProcess, IProcessCollection
{
	public List<RuntimeProcess> Commands = new List<RuntimeProcess>();
	
	public void AddProcess(RuntimeProcess rp)
	{
		Commands.Add(rp);
	}

	public void SetProvider(IPKInputProvider input)
	{
		throw new NotImplementedException("wrong type of IProcessCollection");
	}
}
