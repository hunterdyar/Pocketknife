using PocketKnifeCore.Engine;

namespace PocketKnifeCore;

public class GenericPipelineInputProvider : IPKInputProvider
{
	public TraversalOrder TraversalOrder { get; }
	private Func<PKItem, PKItem[], IEnumerable<PKItem>> InputEnumerator;
	private PKItem[] _args;
	private Context _context;
	public Type ProvidedType { get; }
	public GenericPipelineInputProvider(Func<PKItem, PKItem[], IEnumerable<PKItem>> inputEnumerator, TraversalOrder order, Type providedType)
	{
		TraversalOrder = order;
		InputEnumerator = inputEnumerator;
		ProvidedType = providedType;
	}
	
	public void SetArguments(bool asPipeline, Context context, PKItem[] args)
	{
		if (!asPipeline)
		{
			throw new NotImplementedException();
		}

		this._args = args;
		_context = context;
	}

	public IEnumerable<PKItem> Enumerate()
	{
		var item = _context.Item;
		return InputEnumerator.Invoke(item, _args);
	}
}