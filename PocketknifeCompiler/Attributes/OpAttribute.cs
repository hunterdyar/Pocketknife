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
		var inPK = PKValue.GetPKType(inType);
		var outPK = PKValue.GetPKType(outType);
		
		Debug.Assert(!PKType.IsNone(inPK));
		Debug.Assert(!PKType.IsNone(outPK));
		catalog.RegisterOp(Name, new OperatorDescription()
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
		var inPK = PKValue.GetPKType(inType);

		if (outType != typeof(void))
		{
			Debug.Assert(false, "Signal operators should return void. return type is ignored. This doesn't matter, but enforcing it because it means i messed something else up.");
		}
		Debug.Assert(!PKType.IsNone(inPK));
		catalog.RegisterOp(Name, new OperatorDescription()
		{
			InType = inPK,
			OutType = PKType.None,
			Method = method
		});
	}
}