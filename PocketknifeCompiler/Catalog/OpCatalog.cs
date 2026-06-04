using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace PocketknifeCore;

public class OpCatalog
{
	public Dictionary<string, OperatorResolver> Operators = new Dictionary<string, OperatorResolver>();

	public void AddOp(string name, OperatorDescription description)
	{
		if (!Operators.ContainsKey(name))
		{
			Operators.Add(name, new OperatorResolver(name));
		}

		Operators[name].AddOp(description);
		return;
	}

	public bool TryGetOp(string name, [MaybeNullWhen(false)] out OperatorResolver resolver)
	{
		return Operators.TryGetValue(name, out resolver);
	}
	
	public static OpCatalog GetDefaultOpCatalog()
	{
		var oc =  new OpCatalog();
		//testing here.
		oc.AddOp("to-lower", new OperatorDescription()
		{
			InType = typeof(string),
			OutType = typeof(string),
			Method = (MethodInfo)typeof(string).GetMethod("ToLower", BindingFlags.Static | BindingFlags.Public)
		});
		oc.AddOp("to-upper", new OperatorDescription()
		{
			InType = typeof(string),
			OutType = typeof(string),
			Method = (MethodInfo)typeof(string).GetMethod("ToUpper", BindingFlags.Static | BindingFlags.Public)
		});
		oc.AddOp("print", new OperatorDescription()
		{
			InType = typeof(string),
			OutType = null,
			Method = (MethodInfo)typeof(Console).GetMethod("WriteLine", BindingFlags.Static | BindingFlags.Public)
		});
		
		return oc;
	}
}

//handles overloads
public class OperatorResolver
{
	public string Name;
	private List<OperatorDescription> Overloads = new List<OperatorDescription>(1);

	public OperatorResolver(string name)
	{
		Name = name;
	}

	public void AddOp(OperatorDescription description)
	{
		if (GetTypes().Contains((description.InType, description.OutType)))
		{
			throw new ArgumentException($"Overload already exists for {description.InType} -> {description.OutType}");
		}

		if (description.InType == null)
		{
			if(Overloads.Any(x => x.InType == null))
			{
				throw new Exception($"Operator {Name} has multiple generator overloads, which is not supported");
			}
		}
		Overloads.Add(description);
	}
	public List<(Type? input, Type? output)> GetTypes()
	{
		return Overloads.Select(x => (x.InType, x.OutType)).ToList()!;
	}

	public OperatorDescription GetGenerator()
	{
		var gen = Overloads.FirstOrDefault(x => x.InType == null);
		if (gen == null)
		{
			throw new Exception($"Operator {Name} is not a generator (or has no generator overloads)");
		}
		else
		{
			return gen;
		}
	}
}

//a single function
public class OperatorDescription
{
	public Type? InType;
	public Type? OutType;
	public required MethodInfo Method;
	//isList, isGenerator, etc.
}