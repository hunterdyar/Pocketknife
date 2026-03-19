namespace PocketKnifeCore;

public interface IProcessCollection
{
	public void AddProcess(RuntimeProcess rp);
	void SetProvider(IPKInputProvider input);
}