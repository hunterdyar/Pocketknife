using System.Text;

namespace PocketKnifeCore.Engine;

public class CommandMethodWrapper<T>
{
	public string MethodName;
	//
	private Dictionary<Type, (T func, Type returnType)> Methods;
	public Type[] GetValidTypes => Methods.Keys.ToArray();
	
	public CommandMethodWrapper(string methodName)
	{
		MethodName = methodName;
		Methods = new Dictionary<Type, (T func, Type returnType)>();//todo i feel like, since most of these will just have one item, we should do a lazy init on the dict?
	}

	public void Add(Type type, T func, Type returnType)
	{
		if (Methods.ContainsKey(type))
		{
			throw new Exception("Cannot add method, type already registered!");
		}
		Methods.Add(type, (func, returnType));
	}
	
	public bool TryGetMethod(Type g, out T func, out Type returnType)
	{ 
		if(Methods.TryGetValue(g, out var fr))
		{
			func = fr.func;
			returnType = fr.returnType;
			return true;
		}
		func = default(T);
		returnType = null;
		return false;
	}

	public string GetValidTypesStringList()
	{
		StringBuilder sb = new StringBuilder();
		for (var i = 0; i < GetValidTypes.Length; i++)
		{
			var type = GetValidTypes[i];
			sb.Append(type.Name.ToString());
			if (i < GetValidTypes.Length - 1)
			{
				sb.Append(", ");
			}
		}

		return sb.ToString();
	}
}

public class FilterMethodsWrapper : CommandMethodWrapper<Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, bool>>>
{
	public FilterMethodsWrapper(string methodName) : base(methodName)
	{
	}
}

public class PipeInputsMethodsWrapper : CommandMethodWrapper<Func<Dictionary<string, PKItem>, Func<PKItem, PKItem[], IEnumerable<PKItem>>>>
{
	public PipeInputsMethodsWrapper(string methodName) : base(methodName)
	{
	}

}

public class PipelineMethodsProvider : CommandMethodWrapper<Func<Dictionary<string, PKItem>, Func<PKItem[], PKItem, PKItem>>>
{
	public PipelineMethodsProvider(string methodName) : base(methodName)
	{
	}
}

public class InputMethodsWrapper : CommandMethodWrapper<Func<RuntimeExpression[], Dictionary<string, PKItem>?, IPKInputProvider>>
{
	public InputMethodsWrapper(string methodName) : base(methodName)
	{
	}
}
