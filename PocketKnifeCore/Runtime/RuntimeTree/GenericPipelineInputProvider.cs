using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class GenericPipelineInputProvider : IPKInputProvider
{
	public TraversalOrder TraversalOrder { get; }
	private Func<PKItem, PKItem[], IEnumerable<PKItem>> InputEnumerator;
	private PKItem[] args;
	private Context _context;
	public GenericPipelineInputProvider(Func<PKItem, PKItem[], IEnumerable<PKItem>> inputEnumerator, TraversalOrder order)
	{
		TraversalOrder = order;
		InputEnumerator = inputEnumerator;
	}
	
	public void SetArguments(bool asPipeline, Context context, PKItem[] args)
	{
		if (!asPipeline)
		{
			throw new NotImplementedException();
		}

		args = args;
		_context = context;
	}

	public IEnumerable<PKItem> Enumerate()
	{
		var item = _context.Item;
		return InputEnumerator.Invoke(item, args);
	}
}