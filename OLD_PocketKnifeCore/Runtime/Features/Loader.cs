using System.Reflection;

namespace PocketKnifeCore.Engine;

public class Loader
{
	private MethodInfo LoaderFunc;
	public Type ProvidedType;

	public static Loader GetLoader(MethodInfo method)
	{
		var provided = method.ReturnType;
		
		if (!provided.IsSubclassOf(typeof(PKItem)))
		{
			throw new Exception("invalid loader");
		}
		
		return new Loader()
		{
			LoaderFunc = method,
			ProvidedType = provided,
		};
	}
	
	private Loader()
	{
		
	}
	public PKItem DoLoad(FileInfo fileInfo)
	{
		return (PKItem)LoaderFunc.Invoke(null, [fileInfo]) ?? throw new InvalidOperationException();
	}
}