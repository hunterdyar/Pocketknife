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

	
	[PipelineOperator("to-upper")]
	public static PKString ToUpperCasePipe(PKString a, PKItem[] arguments)
	{
			a.Value = a.Value.ToUpperInvariant();
			return a;
	}

	[PipelineOperator("to-lower")]
	public static PKString ToLowerCasePipe(PKString a, PKItem[] arguments)
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

	[PipelineOperator("append")]
	public static PKString Append(PKString item, PKItem[] args)
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

	[PipelineOperator("append-line")]
	public static PKString AppendLine(PKString item, PKItem[] args)
	{
		var x = Append(item, args);
		x.Value = x.Value + Environment.NewLine;
		return x;
	}

	[PipelineOperator("trim")]
	public static PKString Trim(PKString item, PKItem[] args)
	{
		item.Value = item.Value.Trim();
		return item;
	}

	[PipelineOperator("trim-start")]
	public static PKString TrimStart(PKString item, PKItem[] args)
	{
		item.Value = item.Value.TrimStart();
		return item;
	}

	[PipelineOperator("trim-end")]
	public static PKString TrimEnd(PKString item, PKItem[] args)
	{
		item.Value = item.Value.TrimEnd();
		return item;
	}

	[PipelineOperator("length")]
	public static PKNumber Length(PKString item, PKItem[] args)
	{
		return new PKNumber(item.Value.Length);
	}
	
	// [SignalOperator("echo")]
	// public static void Echo(PKString input, PKItem[] args)
	// {
	// 	Console.WriteLine(input);
	// 	if (args.Length > 0)
	// 	{
	// 		Console.Write(" ");
	// 		foreach (var item in args)
	// 		{
	// 			if (item.TryGetString(out var s))
	// 			{
	// 				Console.Write(s);
	// 				Console.Write(" ");
	// 			}
	// 		}
	// 	}
	// }
}