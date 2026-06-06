namespace PocketknifeCore;

public class Context
{
	public Stack<PKStream> Streams = new Stack<PKStream>();

	public void PushStream(PKType type, List<PKValue> list)
	{
		Streams.Push(new PKStream() { Type = type, Values = list });
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
	
	public void Pack()
	{
		var top = Streams.Peek();
		//type => list of type.
		
	}

	public void Unpack()
	{
		
	}
}