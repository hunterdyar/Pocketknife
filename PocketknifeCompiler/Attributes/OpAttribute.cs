using System.Diagnostics;
using System.Reflection;

namespace PocketknifeCore.Attributes;

public abstract class OpAttribute : Attribute
{
	public abstract void Register(OpCatalog catalog, MethodInfo method);

}

[AttributeUsage(AttributeTargets.Method)]
public class PipelineAttribute : OpAttribute
{
	public string Name { get; set; }
	public override void Register(OpCatalog catalog, MethodInfo method)
	{
		var inType = method.GetParameters()[0].ParameterType;
		var outType = method.ReturnType;
		
		Debug.Assert(!PKType.IsNone(inType));
		Debug.Assert(!PKType.IsNone(outType));
		catalog.RegisterOp(Name, new OperatorDescription(OpKind.Pipeline)
		{
			InType = inType,
			OutType = outType,
			Method = method
		});
	}
}

[AttributeUsage(AttributeTargets.Method)]
public class SignalAttribute : OpAttribute
{
	public string Name { get; set; }

	public override void Register(OpCatalog catalog, MethodInfo method)
	{
		var inType = method.GetParameters()[0].ParameterType;
		var outType = method.ReturnType;

		if (outType != typeof(void))
		{
			Debug.Assert(false, "Signal operators should return void. return type is ignored. This doesn't matter, but enforcing it because it means i messed something else up.");
		}
		Debug.Assert(!PKType.IsNone(inType));
		catalog.RegisterOp(Name, new OperatorDescription(OpKind.Signal)
		{
			InType = inType,
			OutType = PKType.None,
			Method = method
		});
	}
}

[AttributeUsage(AttributeTargets.Method)]
public class FilterAttribute : OpAttribute
{
	public string Name { get; set; }

	public override void Register(OpCatalog catalog, MethodInfo method)
	{
		var inType = method.GetParameters()[0].ParameterType;
		var outType = method.ReturnType;

		Debug.Assert(!PKType.IsNone(inType));
		Debug.Assert(outType == typeof(bool));
		catalog.RegisterOp(Name, new OperatorDescription(OpKind.Filter)
		{
			InType = inType,
			OutType = typeof(bool),
			Method = method
		});
	}
}

[AttributeUsage(AttributeTargets.Method)]
public class GeneratorAttribute : OpAttribute
{
	public string Name { get; set; }

	public override void Register(OpCatalog catalog, MethodInfo method)
	{
		foreach (var param in method.GetParameters())
		{
			var inPK = param.ParameterType;
			if (inPK == PKType.None)
			{
				throw new Exception($"Could not determine type of parameter {param.Name} in method {method.Name}");
			}
		}
		var outType = method.ReturnType;
		if (!outType.IsStream())
		{
			throw new Exception($"Generator {method.Name} must return a stream type (e.g. List<T>)");
		}

		catalog.RegisterOp(Name, new OperatorDescription(OpKind.Generator)
		{
			InType = PKType.None,
			OutType = outType,
			Method = method
		});
	}
}

[AttributeUsage(AttributeTargets.Method)]
public class PipeGeneratorAttribute : OpAttribute
{
	public string Name { get; set; }

	public PipeGeneratorAttribute(string name)
	{
		Name = name;
	}


	public override void Register(OpCatalog catalog, MethodInfo method)
	{
		var paramCount = method.GetParameters().Length;
		Debug.Assert(paramCount >= 1);
		
		var inPK= method.GetParameters()[0].ParameterType;
		if (inPK == PKType.None)
		{
			throw new Exception($"Could not determine type of first parameter {inPK.Name} in method {method.Name}");
		}

		var outType = method.ReturnType;
		if (!outType.IsStream())
		{
			throw new Exception($"Generator {method.Name} must return a stream type (e.g. List<T>)");
		}

		catalog.RegisterOp(Name, new OperatorDescription(OpKind.PipeIn)
		{
			InType = inPK,
			OutType = outType,
			Method = method
		});
	}
}

public class CastingAttribute : OpAttribute
{
	public bool IsImplicit => _isImplicit;
	private bool _isImplicit;
	public CastingAttribute(bool isImplicit = true)
	{
		_isImplicit = isImplicit;
	}
	public override void Register(OpCatalog catalog, MethodInfo method)
	{
		if (method.GetParameters().Length != 1)
		{
			throw new Exception($"Casting operator {method.Name} must have exactly one parameter");
		}

		var inPK = method.GetParameters()[0].ParameterType;
		var outPK = method.ReturnType;

		if (inPK == PKType.None)
		{
			throw new Exception($"Could not determine type of parameter {method.GetParameters()[0].Name} in method {method.Name}.");
		}

		if (outPK == PKType.None)
		{
			throw new Exception($"Could not determine return type of method {method.Name}, our is void");
		}		

		catalog.RegisterCast(new CastingDescription(_isImplicit)
		{
			InType = inPK,
			OutType = outPK,
			Method = method
		});
	}
}

