namespace PocketKnifeCore;

public class FilterProcess : RuntimeProcess
{
	private Func<PKItem, bool> _filter;
	public FilterProcess(Func<PKItem, bool> filter)
	{
		_filter = filter;
	}
}

public class PipelineProcess : RuntimeProcess
{
	private Func<PKItem, PKItem> _process;

	public PipelineProcess(Func<PKItem, PKItem> process)
	{
		_process = process;
	}
}

public class SignalProcess : RuntimeProcess
{
	
}
