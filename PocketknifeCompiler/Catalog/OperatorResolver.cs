using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
			//todo: compare arguments.
			throw new ArgumentException($"Overload already exists for {description.InType} -> {description.OutType}");
		}

		if (PKType.IsNone(description.InType))
		{
			if(_overloads.Any(x => PKType.IsNone(x.InType)))
			{
				throw new Exception($"Operator {Name} has multiple generator overloads, which is not supported");
			}
		}
		_overloads.Add(description);
	}
	public List<(PKType input, PKType output)> GetTypes()
	{
		return _overloads.Select(x => (x.InType, x.OutType)).ToList()!;
	}

	public OperatorDescription GetGenerator()
	{
		var gen = _overloads.FirstOrDefault(x => PKType.IsNone(x.InType));
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
	public OpInvoker GetOrBuildInvoker(PKType input, out OperatorDescription overload)
	{
		overload = _overloads.FirstOrDefault(x => x.InType.Equals(input));
		
		//is generic?
		if (overload == null)
		{
			overload = _overloads.FirstOrDefault(x => x.InType.Equals(new PKType(PKKind.Any, input.LiftLevel)));
		}
		
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

	private Dictionary<OperatorDescription, GenInvoker> _generators = new Dictionary<OperatorDescription, GenInvoker>();

	public GenInvoker GetOrBuildGenerator(out OperatorDescription overload)
	{
		overload = _overloads.FirstOrDefault(x => PKType.IsNone(x.InType));

		if (overload != null)
		{
			//get
			if (_generators.TryGetValue(overload, out var invoker))
			{
				return invoker;
			}

			//or build
			var builtInvoker = BuildGenerator(overload);
			_generators.Add(overload, builtInvoker);
			return builtInvoker;
		}
		else
		{
			throw new Exception($"Operator {Name} does not exist or is not a generator (>)");
		}
	}

	static OpInvoker BuildInvoker(OperatorDescription description)
	{
		var target = description.Method;
		
		var pInput = Expression.Parameter(typeof(PKValue), "input");
		var pArgs = Expression.Parameter(typeof(PKValue[]), "args");
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
			if (i == 0 && !PKType.IsNone(description.InType))
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
		//already a PKValue? pass through.
		if (targetType == typeof(PKValue))
		{
			return pkValueExpr;
		}

		//map CLR type -> the PKValue accessor method name.
		string accessor = null;
		switch (targetType)
		{
			case var _ when targetType == typeof(string):
				accessor = nameof(PKValue.AsString);
				break;
			case var _ when targetType == typeof(int):
				accessor = nameof(PKValue.AsInt);
				break;
			case var _ when targetType == typeof(long):
				accessor = nameof(PKValue.AsLong);
				break;
			case var _ when targetType == typeof(bool):
				accessor = nameof(PKValue.AsBool);
				break;
			case var _ when targetType == typeof(double):
				accessor = nameof(PKValue.AsDouble);
				break;
			
		}

		if (accessor != null)
		{
			var method = typeof(PKValue).GetMethod(accessor, Type.EmptyTypes) ?? throw new InvalidOperationException($"PKValue.{accessor} missing");
			return Expression.Call(pkValueExpr, method);
		}

		//generic collection target: List<T> / IEnumerable<T>
		if (targetType.GenericTypeArguments.Length > 0)
		{
			var oftd = targetType.GetGenericTypeDefinition();
			var elementType = targetType.GenericTypeArguments[0];
			var oft = PKValue.GetPKType(elementType);
			if (oft != PKType.None && (oftd == typeof(List<>) || oftd == typeof(IEnumerable<>)))
			{
				//pkValueExpr.AsList() -> List<PKValue>
				var asListMethod = typeof(PKValue).GetMethod(nameof(PKValue.AsList), Type.EmptyTypes)
					?? throw new InvalidOperationException("PKValue.AsList missing");
				Expression listExpr = Expression.Call(pkValueExpr, asListMethod);

				//list.Select(v => ConvertPKValue(v, elementType))
				var vParam = Expression.Parameter(typeof(PKValue), "v");
				var convertedElement = ConvertPKValue(vParam, elementType);
				var selectorType = typeof(Func<,>).MakeGenericType(typeof(PKValue), elementType);
				var selectorLambda = Expression.Lambda(selectorType, convertedElement, vParam);

				var selectMethod = typeof(Enumerable).GetMethods()
					.First(m => m.Name == nameof(Enumerable.Select)
								&& m.GetParameters().Length == 2
								&& m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>))
					.MakeGenericMethod(typeof(PKValue), elementType);
				Expression projected = Expression.Call(selectMethod, listExpr, selectorLambda);

				if (oftd == typeof(List<>))
				{
					var toListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))!
						.MakeGenericMethod(elementType);
					return Expression.Call(toListMethod, projected);
				}
				//IEnumerable<T>: Select already returns IEnumerable<T>
				return projected;
			}
		}

		throw new ArgumentOutOfRangeException(nameof(targetType), $"No PKValue accessor for target type {targetType}");
	}
	//get an expression that returns arg[i] for compiling.
	private static Expression ReadArg(ParameterExpression pArgs, int index, Type targetType, ParameterInfo paramInfo)
	{
		//args[index]: array element access; the JIT inlines the bounds check.
		var indexed = Expression.ArrayIndex(pArgs, Expression.Constant(index));
		var converted = ConvertPKValue(indexed, targetType);

		//no default? just read and convert; out-of-range will throw at runtime.
		if (!paramInfo.HasDefaultValue)
		{
			return converted;
		}

		//has a default: emit args.Length > index ? Convert(args[index]) : <default>
		var hasIt = Expression.GreaterThan(Expression.ArrayLength(pArgs), Expression.Constant(index));
		var defaultExpr = Expression.Constant(paramInfo.DefaultValue, targetType);

		return Expression.Condition(hasIt, converted, defaultExpr);
	}
	//wrap result just changes the functions return types so the lambda body always matches the delegate.
	private static Expression WrapResult(Expression callExpr, Type returnType)
	{
		//already a PKValue, nothing to do.
		if (returnType == typeof(PKValue))
		{
			return callExpr;
		}

		//void: execute the call, then value is PKValue.None.
		if (returnType == typeof(void))
		{
			var noneField = typeof(PKValue).GetField(nameof(PKValue.None), BindingFlags.Public | BindingFlags.Static)!;
			//run the call first, then return PKValue.None
			return Expression.Block(callExpr, Expression.Field(null, noneField));
		}

		//generic stream return: List<T> / IEnumerable<T> -> PKValue holding List<PKValue>
		if (returnType.IsGenericType)
		{
			var rtd = returnType.GetGenericTypeDefinition();
			if (rtd == typeof(List<>) || rtd == typeof(IEnumerable<>))
			{
				var elementType = returnType.GenericTypeArguments[0];
				var elementPKType = PKValue.GetPKType(elementType);
				if (elementPKType != PKType.None)
				{
					var fromListMethod = typeof(PKValue).GetMethod(nameof(PKValue.FromList), new[] { typeof(List<PKValue>), typeof(PKType) })
						?? throw new InvalidOperationException("PKValue.FromList missing");

					//micro-opt: element is already PKValue, skip the per-element projection
					if (elementType == typeof(PKValue))
					{
						Expression asListDirect = rtd == typeof(List<>)
							? callExpr
							: Expression.Call(typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))!.MakeGenericMethod(typeof(PKValue)), callExpr);
						return Expression.Call(fromListMethod, asListDirect, Expression.Constant(elementPKType, typeof(PKType)));
					}

					//for each element t: WrapResult(t, elementType) -> PKValue
					var tParam = Expression.Parameter(elementType, "t");
					var wrappedElement = WrapResult(tParam, elementType);
					var selectorType = typeof(Func<,>).MakeGenericType(elementType, typeof(PKValue));
					var selectorLambda = Expression.Lambda(selectorType, wrappedElement, tParam);

					//src.Select(t => WrapResult(t))
					var selectMethod = typeof(Enumerable).GetMethods()
						.First(m => m.Name == nameof(Enumerable.Select)
									&& m.GetParameters().Length == 2
									&& m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>))
						.MakeGenericMethod(elementType, typeof(PKValue));
					Expression projected = Expression.Call(selectMethod, callExpr, selectorLambda);

					//.ToList() so we have a concrete List<PKValue> to stash in PKValue._ref
					var toListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))!
						.MakeGenericMethod(typeof(PKValue));
					Expression asList = Expression.Call(toListMethod, projected);

					//PKValue.FromList(list, elementPKType)
					return Expression.Call(fromListMethod, asList, Expression.Constant(elementPKType, typeof(PKType)));
				}
			}
		}

		//some other CLR type, wrap via a PKValue.From(T) factory. this will probably need to be rewritten i think?
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

		return Expression.Call(method, callExpr);
	}

	public bool HasOp(PKType top)
	{
		//todo: if we have list<list<int>>... that won't work?
		return _overloads.Any(x => x.InType.Equals(top) || x.InType.Equals(new PKType(PKKind.Any, top.LiftLevel)));
	}

	private GenInvoker BuildGenerator(OperatorDescription description)
	{
		var target = description.Method;

		var pArgs = Expression.Parameter(typeof(PKValue[]), "args");
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


		var result = Expression.Lambda<GenInvoker>(body, pArgs, pCtx).Compile();

		Debug.WriteLine($"Built invoker for {target.Name}:");
		Debug.WriteLine(Expression.Lambda<GenInvoker>(body, pArgs, pCtx).ToString());
		return result;
	}
}