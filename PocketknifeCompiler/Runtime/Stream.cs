using System.Collections;
using System.Diagnostics;

namespace PocketknifeCore;

//A Stream is an N-dimensional list of values packed into a single-dimensional list. It provides an iterator that returns the value and the indices of parent arrays,
//those parents are 
public class Stream : IEnumerable<(int, object)>
{
	public Type Type;
	public List<object> Values;
	public List<Stream>? Children;

	public Stream()
	{
		Values = new List<object>();
		Children = null;
	}
	public Stream(List<object> objects)
	{
		Values = objects;
		Children = null;
	}
	
	public Stream(Stream baseStream, List<object> values)
	{
		Type = baseStream.Type.Lower();
		Values = values;
		Children = null;
	}

	public bool IsLeaf => Children == null;

	public IEnumerator<(int, object)> GetEnumerator()
	{
		if (Children != null)
		{
			foreach (var child in Children)
			{
				var e = child.GetEnumerator();
				while (e.MoveNext())
				{
					yield return e.Current;
				}
				e.Dispose();
			}
		}
	}
	
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public static Stream CreateStream(Stream baseStream, List<object> values)
	{
		//if base is [a,b,c] r[3] and values is [1,2]
		//then new stream is [1,2,1,2,1,2] with rank[3,2]
		
		//we could also ask the stream next up on the stack for a value at [0,1,or 2].
		throw new NotImplementedException();
	}

	public void GenerateChildStream(Type inputType, object[] ia, GenInvoker generator, Context context)
	{
		if (!IsLeaf)
		{
			foreach (var child in Children!)
			{
				child.GenerateChildStream(inputType, ia, generator, context);
			}

			return;
		}
		
		Debug.Assert(Children == null);
		Children = new List<Stream>();
		
		foreach (var item in Values)
		{
			Children.Add(new Stream(generator.Invoke(ia, context)));
		}
	}

	public void OperateOnEach(OpInvoker invoker, object[] args, Context context)
	{
		if (!IsLeaf)
		{
			foreach (var child in Children!)
			{
				child.OperateOnEach(invoker, args, context);
			}
			return;
		}
		
		for(var i = 0; i < Values.Count; i++)
		{
			var value = Values[i];
			var result = invoker(value, args, context);
			Values[i] = result;
		}
	}

	public void SignalOnEach(OpInvoker invoker, object[] args, Context context)
	{
		if (!IsLeaf)
		{
			foreach (var child in Children!)
			{
				child.SignalOnEach(invoker, args, context);
			}

			return;
		}

		for (var i = 0; i < Values.Count; i++)
		{
			var value = Values[i];
			invoker(value, args, context);
		}
	}
	
	public void FilterOnEach(OpInvoker foprInvoker, object[] args, Context context)
	{
		if (!IsLeaf)
		{
			foreach (var child in Children!)
			{
				child.FilterOnEach(foprInvoker, args, context);
			}

			return;
		}
		
		var result = new List<object>(Values.Count);
		for (var i = 0; i < Values.Count; i++)
		{
			var v = Values[i];
			if ((bool)foprInvoker(v, args, context))
			{
				result.Add(v);
			}
		}

		Values = result;
	}

	public Stream Clone()
	{
		return new Stream()
		{
			Children = this.Children == null ? null : [..this.Children],
			Type = this.Type,
			Values = [..this.Values]
		};
	}


}