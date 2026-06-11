using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using PocketknifeCore.Attributes;

namespace PocketknifeCore;

public class OpCatalog
{
	public readonly Dictionary<string, OperatorResolver> Operators = new Dictionary<string, OperatorResolver>();
	private readonly Dictionary<Type, List<CastingDescription>> ImplicitCasts = new Dictionary<Type, List<CastingDescription>>();
	private readonly Dictionary<Type, List<CastingDescription>> ExplicitCasts = new Dictionary<Type, List<CastingDescription>>();
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
		oc.RegisterMethods(typeof(IntMethods));
		oc.RegisterMethods(typeof(BoolMethods));
		oc.RegisterMethods(typeof(DoubleMethods));
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


	public void RegisterCast(CastingDescription cast)
	{
		if (cast.Implicit)
		{
			if(!ImplicitCasts.ContainsKey(cast.InType))
			{
				ImplicitCasts.Add(cast.InType, new List<CastingDescription>());
			}
			ImplicitCasts[cast.InType].Add(cast);
		}
		else
		{
			if(!ExplicitCasts.ContainsKey(cast.InType))
			{
				ExplicitCasts.Add(cast.InType, new List<CastingDescription>());
			}
			ExplicitCasts[cast.InType].Add(cast);
		}
	}
	

	public IEnumerable<CastingDescription> GetImplicitCasts(Type inType)
	{
		if(ImplicitCasts.TryGetValue(inType, out var casts))
		{
			return casts;
		}
		return Enumerable.Empty<CastingDescription>();
	}
	
	public IEnumerable<CastingDescription> GetExplicitCasts(Type inType)
	{
		if(ExplicitCasts.TryGetValue(inType, out var casts))
		{
			return casts;
		}
		return Enumerable.Empty<CastingDescription>();
	}
}

//a single function