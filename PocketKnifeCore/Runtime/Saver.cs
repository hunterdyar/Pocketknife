using System.Management.Automation;

namespace PocketKnifeCore.Engine;

public class Saver
{
	public bool Overwrite = true;
	public Action<FileStream, PKItem> Writer;
	public string DefaultExtension;
	public Type? OnlyValidOn;
	public string Name;

	public void Execute(Context context, PKItem[] args)
	{
		var item = context.Item;
		//arg 0 will be the type of the saver (csv, text, etc)
		//args 1....^1 should be a combined directory (optional)
		//the last argument should be the file name to save as. Extension is optional, if not provided, will use defaultExtension.
		
		FileInfo file = null;
		if (args.Length >= 3)
		{
			//0...length-2 = dir
			//length-1 == filename
			if (!args[^1].TryGetString(out var filename))
			{
				throw new Exception($"invalid argument on save {Name} command. Should be a string or filename to be converted into a file path.");

			}
			string[] dirs = new string[args.Length - 2];
			for (int i = 1; i<args.Length-1; i++)
			{
				if(args[i].TryGetString(out var s))
				{
					dirs[i - 1] = s;
				}
				else
				{
					throw new Exception($"invalid argument on save {Name} command. Argument {i} ({args[i].ToString()}) cannot be made into a path");
				}
			}

			var baseDir = Environment.CurrentDirectory;
			
			var path = Path.Combine(dirs);
			var dir = new DirectoryInfo(path);
			if (!dir.Exists)
			{
				dir = new DirectoryInfo(Path.Combine(baseDir, path));
				if (!dir.Exists)
				{
					dir.Create();
				}

				// if (!dir.Exists)
				// {
				// 	throw new Exception(
				// 		"invalid save location, directory does not exists."); //createDir=true should be an option.
				// }
			}
			
			
		
			file = new FileInfo(dir.FullName+filename);
		}else if (args.Length == 2)
		{
			if (args[1].TryGetString(out var s))
			{
				if (s.EndsWith("/"))
				{
					//directory. get the original filename from an input filetype, if possible.
					if (context.TryFindItemAsTypeSearchUp<PKFileInfo>(out var pkFileInfo))
					{
						var originalInfo = pkFileInfo.Value;
						//okay, we have our fileInfo, so we can get the name.
						var dir = originalInfo.Directory;
						file = new FileInfo(dir + s + DefaultExtension);
					}
					
				}
				else
				{
					file = new FileInfo(s);
				}
			}
			else
			{
				throw new Exception($"invalid argument on save {Name}. when given one argument, it should be a string, convertable to a path");
			}
		}else if (args.Length == 1)
		{
			//we have been given nothing! uh oh.
			if (context.TryFindItemAsTypeSearchUp<PKFileInfo>(out var pkFileInfo))
			{
				var originalInfo = pkFileInfo.Value;
				//okay, we have our fileInfo, so we can get the name.
				var originalExt = originalInfo.Extension;
				var name = originalInfo.Name.Substring(originalInfo.Name.Length - originalExt.Length,
					originalExt.Length);
				//next, the directory... the original one!
				var dir = originalInfo.Directory;
				file = new FileInfo(dir + name + DefaultExtension);
			}
		}

		if (file != null)
		{
			if (Overwrite && file.Exists)
			{
				using (var fs = file.OpenWrite())
				{
					Writer.Invoke(fs, item);
				}
				return;
			}else if (!file.Exists)
			{
				using (var fs = file.Create())
				{
					Writer.Invoke(fs, item);
				}
			}
			else
			{
				//!file.exists and !overwrite
				throw new Exception($"cannot overwrite file {file} during save command.");
			}

		
		}
		else
		{
			throw new Exception("unexpected error during |save commands. invalid arguments?");
		}
	}
}