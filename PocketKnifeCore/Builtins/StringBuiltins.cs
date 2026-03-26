namespace PocketKnifeCore;

public static class StringBuiltins
{
	[Loader("text")]
	public static PKString LoadTextToString(FileInfo file)
	{
		var stream = file.OpenText();
		var content = stream.ReadToEnd();
		stream.Close();
		return new PKString(content);
	}

	[Saver("text", "txt", typeof(PKString))]
	public static void SaveStringToText(FileStream fs, PKString item)
	{
		using (var s = new StreamWriter(fs))
		{
			s.Write(item.Value);	
		}
	}

	
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

	[PipelineOperator("append", typeof(PKString))]
	public static PKItem Append(PKString item, PKItem[] args)
	{
		if (args.Length == 0)
		{
			throw new Exception("|append needs at least one argument.");
		}

		var appends = new string[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			appends[i] = args[i].ToString();
		}

		if (item.TryGetString(out string s))
		{
			for (int i = 0; i < args.Length; i++)
			{
				s += args[i];
			}

			return new PKString(s);
		}

		throw new Exception($"Cannot call |append on type {item.Type}");
	}

	[PipeInputOperator("lines", typeof(PKString))]
	public static IEnumerable<PKItem> Lines(PKString input, PKItem[] args)
	{
		if (args.Length > 0)
		{
			throw new Exception($"|>lines does not take any arguments.");

			if (args.Length == 1)
			{
				//skip-empty is one of the arguments, i think? although that feels like an 'option'.
			}
		}

		foreach (var line in input.Value.Split(Environment.NewLine))
		{
			yield return new PKString(line);
		}
	}
}