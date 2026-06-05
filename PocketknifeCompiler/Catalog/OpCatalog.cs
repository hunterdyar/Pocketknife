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
	
	public static string ToLower(string s) => s.ToLower();
	public static string ToUpper(string s) => s.ToUpper();

	public static OpCatalog GetDefaultOpCatalog()
	{
		var oc =  new OpCatalog();
		//testing here.
		oc.AddOp("to-lower", new OperatorDescription()
		{
			InType = PKKind.String,
			OutType = PKKind.String,
			Method = typeof(OpCatalog).GetMethod("ToLower", BindingFlags.Static | BindingFlags.Public, binder: null, types: new Type[] { typeof(string) }, modifiers: null)

		});
		oc.AddOp("to-upper", new OperatorDescription()
		{
			InType = PKKind.String,
			OutType = PKKind.String,
			Method = typeof(OpCatalog).GetMethod("ToUpper", BindingFlags.Static | BindingFlags.Public)!
		});
		oc.AddOp("print", new OperatorDescription()
		{
			InType = PKKind.String,
			OutType = PKKind.None,
			Method = (MethodInfo)typeof(Console).GetMethod("WriteLine", BindingFlags.Static | BindingFlags.Public, binder: null, types: new Type[] { typeof(string) }, modifiers:null)
		});
		
		// pipein (generator from input) |> types aren't defined by the InType or OutType. guess we need a struct to hold kind/isstream? 
		// oc.AddOp("lines", new OperatorDescription()
		// {
		// 	
		// 	InType = PKKind.String,
		// 	OutType = PKKind.String,
		// 	Method = typeof(string).GetMethod("Split", new[] { typeof(string), typeof(StringSplitOptions) })
		// });
		
		return oc;
	}
}

//a single function