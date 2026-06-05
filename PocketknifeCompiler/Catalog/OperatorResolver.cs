using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace PocketknifeCore;

//handles overloads
public class OperatorResolver
{
	public string Name;
	private List<OperatorDescription> _overloads = new List<OperatorDescription>(1);

	public OperatorResolver(string name)
	{
		Name = name;
	}

	public void AddOp(OperatorDescription description)
	{
		Debug.Assert(description.Method != null);
		if (GetTypes().Contains((description.InType, description.OutType)))
		{
			throw new ArgumentException($"Overload already exists for {description.InType} -> {description.OutType}");
		}

		if (description.InType == PKKind.None)
		{
			if(_overloads.Any(x => x.InType == PKKind.None))
			{
				throw new Exception($"Operator {Name} has multiple generator overloads, which is not supported");
			}
		}
		_overloads.Add(description);
	}
	public List<(PKKind input, PKKind output)> GetTypes()
	{
		return _overloads.Select(x => (x.InType, x.OutType)).ToList()!;
	}

	public OperatorDescription GetGenerator()
	{
		var gen = _overloads.FirstOrDefault(x => x.InType == null);
		if (gen == null)
		{
			throw new Exception($"Operator {Name} is not a generator (or has no generator overloads)");
		}
		else
		{
			return gen;
		}
	}
	
	private Dictionary<OperatorDescription, OpInvoker> Invokers = new Dictionary<OperatorDescription, OpInvoker>();
	public OpInvoker GetOrBuildInvoker(PKKind input, out OperatorDescription overload)
	{
		overload = _overloads.FirstOrDefault(x => x.InType == input);
		
		if (overload != null)
		{
			//get
			if (Invokers.TryGetValue(overload, out var invoker))
			{
				return invoker;
			}
			
			//or build
			var builtInvoker = BuildInvoker(overload);
			Invokers.Add(overload, builtInvoker);
			return builtInvoker;
		}
		else
		{
			throw new Exception($"Operator {Name} does not have an overload for incoming type {input}");
		}
	}

	static OpInvoker BuildInvoker(OperatorDescription description)
	{
		var target = description.Method;
		
		var pInput = Expression.Parameter(typeof(PKValue), "input");
		var pArgs = Expression.Parameter(typeof(ReadOnlySpan<PKValue>), "args");
		var pCtx = Expression.Parameter(typeof(Context), "ctx");
		
		var parameters = target.GetParameters();
		var callArgs = new Expression[parameters.Length];

		int argIndex = 0;
		for (int i = 0; i < parameters.Length; i++)
		{
			var pType = parameters[i].ParameterType;

			if (pType == typeof(Context))
			{
				callArgs[i] = pCtx;
				continue;
			}

			//if it's a generator, we don't have an input parameter; 
			if (i == 0 && description.InType != PKKind.None)
			{
				callArgs[i] = ConvertPKValue(pInput, pType);
				continue;
			}

			callArgs[i] = ReadArg(pArgs, argIndex++, pType, parameters[i]);
		}

		Expression body;
		if (target.IsStatic)
		{
			body = Expression.Call(target, callArgs);
			body = WrapResult(body, target.ReturnType);
		}
		else
		{
			throw new NotImplementedException();
		}
		

		var result = Expression.Lambda<OpInvoker>(body, pInput, pArgs, pCtx).Compile();

		Debug.WriteLine($"Built invoker for {target.Name}:");
		Debug.WriteLine(Expression.Lambda<OpInvoker>(body, pInput, pArgs, pCtx).ToString());
		return result;
	}

	//expression that returns expression from value, cast.
	private static Expression ConvertPKValue(Expression pkValueExpr, Type targetType)
	{
		// Already a PKValue? Pass through.
		if (targetType == typeof(PKValue))
			return pkValueExpr;

		// Map CLR type -> the PKValue accessor method name.
		string accessor = targetType switch
		{
			_ when targetType == typeof(string) => nameof(PKValue.AsString),
			_ when targetType == typeof(int) => nameof(PKValue.AsInt),
			_ when targetType == typeof(long) => nameof(PKValue.AsLong),
			_ when targetType == typeof(bool) => nameof(PKValue.AsBool),
			_ when targetType == typeof(double) => nameof(PKValue.AsDouble),
			// ...
			_ => throw new NotSupportedException($"No PKValue accessor for target type {targetType}")
		};

		var method = typeof(PKValue).GetMethod(accessor, Type.EmptyTypes) ?? throw new InvalidOperationException($"PKValue.{accessor} missing");

		return Expression.Call(pkValueExpr, method);
	}

	static readonly PropertyInfo SpanIndexer = typeof(ReadOnlySpan<PKValue>).GetProperty("Item")!;
	static readonly PropertyInfo SpanLength = typeof(ReadOnlySpan<PKValue>).GetProperty(nameof(ReadOnlySpan<PKValue>.Length))!;

	//get an expression that returns arg[i] for compiling.
	private static Expression ReadArg(ParameterExpression pArgs, int index, Type targetType, ParameterInfo paramInfo)
	{
		// args[index]  — note: indexer returns 'ref readonly PKValue', but for our
		// purposes it materializes as a PKValue value in the expression tree.
		var indexed = Expression.Property(pArgs, SpanIndexer, Expression.Constant(index));
		var converted = ConvertPKValue(indexed, targetType);

		// No default? Just read and convert; out-of-range will throw at runtime.
		if (!paramInfo.HasDefaultValue){
			return converted;
		}

		// Has a default: emit args.Length > index ? Convert(args[index]) : <default>
		var hasIt = Expression.GreaterThan(Expression.Property(pArgs, SpanLength), Expression.Constant(index));
		var defaultExpr = Expression.Constant(paramInfo.DefaultValue, targetType);

		return Expression.Condition(hasIt, converted, defaultExpr);
	}

	//wrap result just changes the functions return types so the lambda body always matches the delegate.
	private static Expression WrapResult(Expression callExpr, Type returnType)
	{
		// Already a PKValue — nothing to do.
		if (returnType == typeof(PKValue))
		{
			return callExpr;
		}

		// 2. void — execute the call, then value is PKValue.None.
		if (returnType == typeof(void))
		{
			var noneField = typeof(PKValue).GetField(nameof(PKValue.None), BindingFlags.Public | BindingFlags.Static)!;
			//runt the call first, then return PKValue.None
			return Expression.Block(callExpr, Expression.Field(null, noneField)); // value of the block = PKValue.None
		}

		// 3) Some other CLR type — wrap via a PKValue.From(T) factory. this will probably need to be rewritten i think?
		//or we can do a better job enforcing the values we want return a PKValue type, and this will never matter.
		//it's basically sugar so that the library functions can be lazily written.
		string factory = returnType switch
		{
			_ when returnType == typeof(string) => nameof(PKValue.FromString),
			_ when returnType == typeof(int) => nameof(PKValue.FromInt),
			_ when returnType == typeof(long) => nameof(PKValue.FromLong),
			_ when returnType == typeof(bool) => nameof(PKValue.FromBool),
			_ when returnType == typeof(double) => nameof(PKValue.FromDouble),
			_ => throw new NotSupportedException($"No PKValue factory for {returnType}")
		};

		var method = typeof(PKValue).GetMethod(factory, new[] { returnType }) ?? throw new InvalidOperationException($"PKValue.{factory}({returnType}) missing");

		return Expression.Call(method, callExpr); // PKValue.FromX(<call>)
	}

	public bool HasOp(PKKind top)
	{
		return _overloads.Any(x => x.InType == top);
	}
}