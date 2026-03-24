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

	[FilterOperator("non-empty", typeof(PKString))]
	public static bool NonEmptyStringFilter(PKString a, PKItem[] arguments)
	{
		return !string.IsNullOrEmpty(a.Value);
	}

	[FilterOperator("empty", typeof(PKString))]
	public static bool EmptyStringFilter(PKString a, PKItem[] arguments)
	{
		return string.IsNullOrEmpty(a.Value);
	}

	[FilterOperator("length", typeof(PKString))]
	public static bool LengthMatchesFilter(PKString a, PKItem[] arguments)
	{
		BuiltinHelpers.CheckArgumentCount(arguments, 1);
		var length = BuiltinHelpers.GetArgument<PKNumber>(arguments,0, "length");
		return (int)a.Value.Length == (int)length.Value;
	}
}