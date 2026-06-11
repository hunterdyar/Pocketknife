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
	public List<(Type input, Type output)> GetTypes()
	{
		return _overloads.Select(x => (x.InType, x.OutType)).ToList();
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
	private Dictionary<(OperatorDescription, CastingDescription), OpInvoker> _castedInvokers = new Dictionary<(OperatorDescription, CastingDescription), OpInvoker>();
	public OpInvoker GetOrBuildInvoker(Type input, out OperatorDescription overload)
	{
		var found = _overloads.FirstOrDefault(x => x.InType == input);
		
		//is generic?
		if (found == null)
		{
			found = _overloads.FirstOrDefault(x => x.InType == PKType.Any.Lift(input.GetLiftLevel()));
		}
		
		if (found != null)
		{
			overload = found;
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

	public OpInvoker GetOrBuildInvoker(Type input, CastingDescription cast, out OperatorDescription overload)
	{
		var found = _overloads.FirstOrDefault(x => x.InType == cast.OutType);

		//is generic?
		if (found == null)
		{
			found = _overloads.FirstOrDefault(x => x.InType == PKType.Any.Lift(cast.OutType.GetLiftLevel()));
		}

		if (found != null)
		{
			overload = found;
			//get
			if (_castedInvokers.TryGetValue((overload, cast), out var invoker))
			{
				return invoker;
			}

			//or build
			var builtInvoker = BuildInvoker(overload, cast);
			_castedInvokers.Add((overload, cast), builtInvoker);
			return builtInvoker;
		}
		else
		{
			throw new Exception($"Operator {Name} does not have an overload for casted type {cast.OutType} (from {input})");
		}
	}
	
	private Dictionary<OperatorDescription, GenInvoker> _generators = new Dictionary<OperatorDescription, GenInvoker>();
	private Dictionary<OperatorDescription, PipeGenInvoker> _pipeGenerators = new Dictionary<OperatorDescription, PipeGenInvoker>();
	private Dictionary<(OperatorDescription, CastingDescription), PipeGenInvoker> _castedPipeGenerators = new Dictionary<(OperatorDescription, CastingDescription), PipeGenInvoker>();

	public GenInvoker GetOrBuildGenerator(out OperatorDescription overload)
	{
		var found = _overloads.FirstOrDefault(x => PKType.IsNone(x.InType));

		if (found != null)
		{
			overload = found;
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
	public PipeGenInvoker GetOrBuildPipeGenerator(Type input, out OperatorDescription overload)
	{
		var found = _overloads.FirstOrDefault(x => x.InType == input);

		
		if (found != null)
		{
			overload = found;
			//get
			if (_pipeGenerators.TryGetValue(overload, out var invoker))
			{
				return invoker;
			}

			//or build
			var builtInvoker = BuildPipeGenerator(overload);
			_pipeGenerators.Add(overload, builtInvoker);
			return builtInvoker;
		}
		else
		{
			throw new Exception($"Operator {Name} does not exist or is not a generator (>)");
		}
	}

	//todo: casting overload for Pipeline Generator

	//expression that returns expression from value, cast.
	private static Expression ConvertPKValue(Expression pkValueExpr, Type targetType)
	{
		if (targetType == typeof(object)) return pkValueExpr;

		//generic collection target: List<T> / IEnumerable<T>
		if (targetType.IsGenericType && (targetType.GetGenericTypeDefinition() == typeof(List<>) || targetType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
		{
			var elementType = targetType.GenericTypeArguments[0];
			var method = typeof(PKRuntimeHelpers).GetMethod(nameof(PKRuntimeHelpers.ConvertList))!.MakeGenericMethod(elementType);
			return Expression.Call(method, pkValueExpr);
		}

		return Expression.Convert(pkValueExpr, targetType);
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
		if (returnType == typeof(void))
		{
			return Expression.Block(callExpr, Expression.Constant(null, typeof(object)));
		}

		if (returnType.IsGenericType && (returnType.GetGenericTypeDefinition() == typeof(List<>) || returnType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
		{
			var method = typeof(PKRuntimeHelpers).GetMethod(nameof(PKRuntimeHelpers.NormalizeList))!;
			return Expression.Call(method, Expression.Convert(callExpr, typeof(object)));
		}

		return Expression.Convert(callExpr, typeof(object));
	}

	public bool HasOp(Type top)
	{
		//todo: if we have list<list<int>>... that won't work?
		return _overloads.Any(x => x.InType == top || x.InType == PKType.Any.Lift(top.GetLiftLevel()));
	}

	static OpInvoker BuildInvoker(OperatorDescription description, CastingDescription? cast = null)
	{
		var target = description.Method;

		var pInput = Expression.Parameter(typeof(object), "input");
		var pArgs = Expression.Parameter(typeof(object[]), "args");
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

			if (i == 0 && !PKType.IsNone(description.InType))
			{
				if (cast != null)
				{
					var castTarget = cast.Method;
					var castParamType = castTarget.GetParameters()[0].ParameterType;
					Expression castInput = ConvertPKValue(pInput, castParamType);
					Expression castCall = castTarget.IsStatic ? Expression.Call(castTarget, castInput) : throw new NotImplementedException("Non-static casting methods are not supported");

					if (castTarget.ReturnType != pType)
					{
						callArgs[i] = Expression.Convert(castCall, pType);
					}
					else
					{
						callArgs[i] = castCall;
					}
				}
				else
				{
					callArgs[i] = ConvertPKValue(pInput, pType);
				}

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
	private GenInvoker BuildGenerator(OperatorDescription description)
	{
		var target = description.Method;

		var pArgs = Expression.Parameter(typeof(object[]), "args");
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

	private PipeGenInvoker BuildPipeGenerator(OperatorDescription description, CastingDescription? cast = null)
	{
		var target = description.Method;

		var pInput = Expression.Parameter(typeof(object), "input");
		var pArgs = Expression.Parameter(typeof(object[]), "args");
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

			if (i == 0 && !PKType.IsNone(description.InType))
			{
				if (cast != null)
				{
					var castTarget = cast.Method;
					var castParamType = castTarget.GetParameters()[0].ParameterType;
					Expression castInput = ConvertPKValue(pInput, castParamType);
					Expression castCall = castTarget.IsStatic ? Expression.Call(castTarget, castInput) : throw new NotImplementedException("Non-static casting methods are not supported");

					if (castTarget.ReturnType != pType)
					{
						callArgs[i] = Expression.Convert(castCall, pType);
					}
					else
					{
						callArgs[i] = castCall;
					}
				}
				else
				{
					callArgs[i] = ConvertPKValue(pInput, pType);
				}

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


		var result = Expression.Lambda<PipeGenInvoker>(body, pInput, pArgs, pCtx).Compile();

		Debug.WriteLine($"Built invoker for {target.Name}:");
		Debug.WriteLine(Expression.Lambda<PipeGenInvoker>(body, pInput, pArgs, pCtx).ToString());
		return result;
	}
}