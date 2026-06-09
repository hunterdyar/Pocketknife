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
		var inPK = PKType.GetPKType(inType);
		var outPK = PKType.GetPKType(outType);
		
		Debug.Assert(!PKType.IsNone(inPK));
		Debug.Assert(!PKType.IsNone(outPK));
		catalog.RegisterOp(Name, new OperatorDescription(OpKind.Pipeline)
		{
			InType = inPK,
			OutType = outPK,
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
		var inPK = PKType.GetPKType(inType);

		if (outType != typeof(void))
		{
			Debug.Assert(false, "Signal operators should return void. return type is ignored. This doesn't matter, but enforcing it because it means i messed something else up.");
		}
		Debug.Assert(!PKType.IsNone(inPK));
		catalog.RegisterOp(Name, new OperatorDescription(OpKind.Signal)
		{
			InType = inPK,
			OutType = PKType.None,
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
			var inPK = PKType.GetPKType(param.ParameterType);
			if (inPK == PKType.None)
			{
				throw new Exception($"Could not determine type of parameter {param.Name} in method {method.Name}");
			}
		}
		var outType = method.ReturnType;
		var outPK = PKType.GetPKType(outType);
		if (!outPK.IsStream)
		{
			throw new Exception($"Generator {method.Name} must return a stream type (e.g. List<T>)");
		}

		catalog.RegisterOp(Name, new OperatorDescription(OpKind.Generator)
		{
			InType = PKType.None,
			OutType = outPK,
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

		var inPK = PKType.GetPKType(method.GetParameters()[0].ParameterType);
		var outPK = PKType.GetPKType(method.ReturnType);

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

