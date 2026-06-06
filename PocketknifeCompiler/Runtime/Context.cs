namespace PocketknifeCore;

public class Context
{
	public Stack<PKFrame> Frames = new Stack<PKFrame>();
	private PKValue[] _argsBuffer = new PKValue[10];
	private int argCount;
	public void PushStream(PKType type, List<PKValue> list)
	{
		Frames.Push(new PKFrame() { Type = type, Values = list });
	}
	public void PopStream()
	{
		Frames.Pop();
	}
	
	//todo: will arguments ever have to be per-op? @Index, etc?
	//todo: differentiate arguments that can lookup up just once, or have to be inside the onEach.
	
	public void OperateOnEach(OpInvoker invoker)
	{
		var top = Frames.Peek();
		ReadOnlySpan<PKValue> args = _argsBuffer.AsSpan(0, argCount);
		for (var i = 0; i < top.Values.Count; i++)
		{
			var value = Frames.Peek().Values[i];
			var result = invoker(value, args, this);
			top.Values[i] = result;
		}
	}

	public void FilterOnEach(OpInvoker foprInvoker)
	{
		var top = Frames.Peek();
		ReadOnlySpan<PKValue> args = _argsBuffer.AsSpan(0, argCount);
		var result = new List<PKValue>(top.Values.Count);
		for (var i = 0; i < top.Values.Count; i++)
		{
			var v = top.Values[i];
			if (foprInvoker(v, args, this).AsBool())
			{
				result.Add(v);
			}
		}

		top.Values = result;
	}
	
	public void Pack()
	{
		var top = Frames.Pop();
		var packedStream = new PKFrame()
		{
			Type = top.Type, 
			Values = new List<PKValue>(1)
		};
		packedStream.Values.Add(new PKValue(top.Type.Lifted(), top.Values));
		Frames.Push(packedStream);

	}

	public void Unpack()
	{
		//stream<List<T>> -> stream<t>
		var top = Frames.Pop();
		if (!top.Type.IsStream)
		{
			throw new Exception("cannot unpack");
		}
		List<PKValue> values = new List<PKValue>();
		//todo: ensure all the same type.
		foreach (var value in top.Values)//probably just a single List<T>, but we will unpack them all sequentially.
		{
			//the actual unpacking, taking out of the [lists in the] stream (aslist) and adding them to the new stream.
			var l = value.AsList();
			values.AddRange(l);
		}
		
		var unpackedStream = new PKFrame()
		{
			Type = top.Type.Lifted(),
			Values = values
		};
		
		Frames.Push(unpackedStream);
	}


}