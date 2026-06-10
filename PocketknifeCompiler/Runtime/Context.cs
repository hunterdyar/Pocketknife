using System.Buffers;
using System.Collections;

namespace PocketknifeCore;

public class Context
{
	private PKFrame[] _frames = new PKFrame[128];
	private int _frameCount;
	private object[] _argsBuffer = new object[10];//todo: pick a bigger number, make a const, then do a max-arg check
	private ArrayPool<object> _args = ArrayPool<object>.Shared;
	private int argCount;
	public void PushStream(Type type, List<object> list)
	{
		_frames[_frameCount] = new PKFrame() { Type = type, Stream = new Stream(list) };
		_frameCount++;
	}

	public void PushStreamWithGenerator(Type inputType, object[] ia, GenInvoker generator)
	{
		if (_frameCount == 0)
		{
			_frames[0] = new PKFrame();
			_frames[0].Stream = new Stream(generator.Invoke(ia, this));
			_frameCount++;
			return;
		}
		//else
		var top = Peek();
		top.Stream.GenerateChildStream(inputType, ia, generator, this);
	}
	
	private void Push(PKFrame frame)
	{
		_frames[_frameCount] = frame;
		_frameCount++;
	}

	private PKFrame Peek()
	{
		return _frames[_frameCount - 1];
	}
	public PKFrame Pop()
	{
		var f = _frames[_frameCount-1];
		_frames[_frameCount-1] = null; //let garbage collector do it's thing
		_frameCount--;
		return f;
	}
	
	//todo: will arguments ever have to be per-op? @Index, etc?
	//todo: differentiate arguments that can lookup up just once, or have to be inside the onEach.
	
	public void OperateOnEach(object[] arguments, OpInvoker invoker)
	{
		var top = Peek();
		argCount = arguments.Length;
		var args = _args.Rent(argCount);
		arguments.CopyTo(args, 0);
		//todo: no, wait; rent provides the buffer. we need to populate it.
		//todo: check if any of the args are per-iteration. If so, mark them somehow and move to inside that for loop.
		//todo: compile the args...

		top.Stream.OperateOnEach(invoker, args, this);
		_args.Return(args);
	}

	public void SignalOnEach(object[] arguments, OpInvoker sInvoker)
	{
		var top = Peek();
		argCount = arguments.Length;
		var args = _args.Rent(argCount);
		arguments.CopyTo(args, 0);
		//todo:see opOnEach
		top.Stream.SignalOnEach(sInvoker, args, this);
		_args.Return(args);
	}

	public void FilterOnEach(object[] arguments, OpInvoker foprInvoker)
	{
		var top = Peek();
		argCount = arguments.Length;
		var args = _args.Rent(argCount);
		arguments.CopyTo(args, 0);//todo: none of this makes sense
		//todo: same arg stuff as OperateOnEach
		
		top.Stream.FilterOnEach(foprInvoker, args, this);
		
	}
	
	public void Pack()
	{
		var top = Pop();
		var packedStream = new PKFrame()
		{
			Type = top.Type.Lift(), 
			Stream = new Stream(new List<object>(1))
		};
		packedStream.Stream.Values.Add(top.Stream.Values);
		Push(packedStream);
	}

	public void Unpack()
	{
		//stream<List<T>> -> stream<t>
		var top = Pop();
		if (!top.Type.IsStream())
		{
			throw new Exception("cannot unpack a non-stream type");
		}
		List<object> values = new List<object>();
		
		foreach (var vt in top.Stream)
		{
			var value = vt.Item2;
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
			Stream = new Stream(values)
		};
		
		Push(unpackedStream);
	}


	//copy the previous frame up.
	public void NewFrame()
	{
		var top = Peek();

		var clonedStream = top.Stream.Clone();
		Push(new PKFrame()
		{
			Type = top.Type,
			Stream = clonedStream,
		});
	}
	
	public void NewNamedFrame(string? name = null)
	{
		var top = Peek();
		var clonedStream = top.Stream.Clone();
		Push(new PKFrame(name)
		{
			Type = top.Type,
			Stream = clonedStream,
		});
	}

	public void PopFrame(BranchType frameType)
	{
		if (Peek().Name != null)
		{
			throw new NotImplementedException();
		}
		
		switch (frameType)
		{
			case BranchType.SideEffect:
				//assign value.
				Pop();
				break;
			case BranchType.ListAppend:
				//todo: we need to typecheck. if the base type is object and empty, then we need to replace the type.
				var branchFrame = Pop();
				
				//you can & onto nothing, and it implicitly creates a new list.
				if (_frameCount == 0)
				{
					Push(new PKFrame()
					{
						Type = branchFrame.Type,
						Stream = new Stream(),
					});
				}
				
				foreach (var value in branchFrame.Stream)
				{
					Peek().Stream.Values.Add(value);
				}
				break;
			case BranchType.Replace:
				var replaceFrame = Pop();
				Peek().Stream = replaceFrame.Stream;
				Peek().Type = replaceFrame.Type;
				break;
		}
	}


}