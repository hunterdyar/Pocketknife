namespace PocketknifeCore;

public class Context
{
	public Stack<PKStream> Streams = new Stack<PKStream>();

	public void PushStream(PKKind kind, List<PKValue> list)
	{
		Streams.Push(new PKStream() { Kind = kind, Values = list });
	}
	public void PopStream()
	{
		Streams.Pop();
	}

	public void OperateOnEach(OpInvoker invoker)
	{
		var top = Streams.Peek();
		for (var i = 0; i < top.Values.Count; i++)
		{
			var value = Streams.Peek().Values[i];
			var result = invoker(value, new ReadOnlySpan<PKValue>(), this);
			top.Values[i] = result;
		}
	}
}