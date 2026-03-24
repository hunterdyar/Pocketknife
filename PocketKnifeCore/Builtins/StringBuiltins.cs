namespace PocketKnifeCore;

public static class StringBuiltins
{
	[PipelineOperator("to-upper", typeof(PKString))]
	public static PKItem ToUpperCasePipe(PKString a, PKItem[] arguments)
	{
			a.Value = a.Value.ToUpperInvariant();
			return a;
	}

	[PipelineOperator("to-lower", typeof(PKString))]
	public static PKItem ToLowerCasePipe(PKString a, PKItem[] arguments)
	{
		a.Value = a.Value.ToLowerInvariant();
		return a;
	}
}