using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using PocketknifeCore.Attributes;

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
		oc.RegisterMethods(typeof(Standard));
		oc.RegisterMethods(typeof(StringMethods));
		oc.RegisterMethods(typeof(ListMethods));
		return oc;
	}

	public void RegisterOp(string name, OperatorDescription operatorDescription)
	{
		if (Operators.ContainsKey(name))
		{
			Operators[name].AddOp(operatorDescription);
		}
		else
		{
			var opr = new OperatorResolver(name);
			opr.AddOp(operatorDescription);
			Operators.Add(name, opr);
		}
	}
	public void RegisterMethods(Type type)
	{
		foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
		{
			foreach (var attribute in method.GetCustomAttributes<OpAttribute>())
			{
				attribute.Register(this, method);
			}
		}
	}


}

//a single function