using System.Buffers;
using System.Collections;

namespace PocketknifeCore;

public class Context
{
	public Stack<PKFrame> Frames = new Stack<PKFrame>();
	private object[] _argsBuffer = new object[10];//todo: pick a bigger number, make a const, then do a max-arg check
	private ArrayPool<object> _args = ArrayPool<object>.Shared;
	private int argCount;
	public void PushStream(Type type, List<object> list)
	{
		Frames.Push(new PKFrame() { Type = type, Values = list });
	}
	public void PopStream()
	{
		Frames.Pop();
	}
	
	//todo: will arguments ever have to be per-op? @Index, etc?
	//todo: differentiate arguments that can lookup up just once, or have to be inside the onEach.
	
	public void OperateOnEach(object[] arguments, OpInvoker invoker)
	{
		var top = Frames.Peek();
		argCount = arguments.Length;
		var args = _args.Rent(argCount);
		arguments.CopyTo(args, 0);
		//todo: no, wait; rent provides the buffer. we need to populate it.
		//todo: check if any of the args are per-iteration. If so, mark them somehow and move to inside that for loop.
		//todo: compile the args...
		
		for (var i = 0; i < top.Values.Count; i++)
		{
			var value = Frames.Peek().Values[i];
			var result = invoker(value, args, this);
			top.Values[i] = result;
		}
		_args.Return(args);
	}

	public void FilterOnEach(object[] arguments, OpInvoker foprInvoker)
	{
		var top = Frames.Peek();
		argCount = arguments.Length;
		var args = _args.Rent(argCount);
		arguments.CopyTo(args, 0);//todo: none of this makes sense
		//todo: same arg stuff as OperateOnEach
		var result = new List<object>(top.Values.Count);
		for (var i = 0; i < top.Values.Count; i++)
		{
			var v = top.Values[i];
			if ((bool)foprInvoker(v, args, this))
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
			Type = top.Type.Lift(), 
			Values = new List<object>(1)
		};
		packedStream.Values.Add(top.Values);
		Frames.Push(packedStream);
	}

	public void Unpack()
	{
		//stream<List<T>> -> stream<t>
		var top = Frames.Pop();
		if (!top.Type.IsStream())
		{
			throw new Exception("cannot unpack a non-stream type");
		}
		List<object> values = new List<object>();
		
		foreach (var value in top.Values)
		{
			if (value is IEnumerable enumerable && !(value is string))
			{
				foreach (var item in enumerable)
				{
					values.Add(item!);
				}
			}
			else
			{
				values.Add(value);
			}
		}
		
		var unpackedStream = new PKFrame()
		{
			Type = top.Type.Lower(),
			Values = values
		};
		
		Frames.Push(unpackedStream);
	}


	//copy the previous frame up.
	public void NewFrame()
	{
		var top = Frames.Peek();
		
		var clonedValues = new List<object>(top.Values.Count);
		clonedValues.AddRange(top.Values);
		Frames.Push(new PKFrame()
		{
			Type = top.Type,
			Values = clonedValues,
		});
	}
	
	public void NewNamedFrame(string? name = null)
	{
		var top = Frames.Peek();
		var clonedValues = new List<object>(top.Values.Count);
		clonedValues.AddRange(top.Values);
		Frames.Push(new PKFrame(name)
		{
			Type = top.Type,
			Values = clonedValues,
		});
	}

	public void PopFrame(BranchType frameType)
	{
		if (Frames.Peek().Name != null)
		{
			throw new NotImplementedException();
		}
		
		switch (frameType)
		{
			case BranchType.SideEffect:
				//assign value.
				Frames.Pop();
				break;
			case BranchType.ListAppend:
				//todo: we need to typecheck. if the base type is object and empty, then we need to replace the type.
				var branchFrame = Frames.Pop();
				foreach (var value in branchFrame.Values)
				{
					Frames.Peek().Values.Add(value);
				}
				break;
			case BranchType.Replace:
				var replaceFrame = Frames.Pop();
				Frames.Peek().Values = replaceFrame.Values;
				Frames.Peek().Type = replaceFrame.Type;
				break;
		}
	}
}