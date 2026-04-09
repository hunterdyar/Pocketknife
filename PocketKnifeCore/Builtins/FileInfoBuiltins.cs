using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PocketKnifeCore;

public static class FileInfoBuiltins
{
	[PipelineOperator("filename")]
	public static PKString Filename(PKFileInfo info, PKItem[] args)
	{
		//todo: figure out the options thing.
		bool ext = true;
		
		if (ext)
		{
			return new PKString(info.Value.Name);
		}
		else
		{
			//no extension
			return new PKString(info.Value.Name.Replace(info.Value.Extension, ""));
		}
	}
	

	[FilterOperator("exists", typeof(PKFileInfo))]
	public static bool Exists(PKFileInfo fileInfo, PKItem[] args)
	{
		return fileInfo.Value.Exists;
	}

	[PipelineOperator("full-name")]
	public static PKString ToPKString(PKFileInfo info, PKItem[] args)
	{
		return new PKString(info.Value.FullName);
	}

	[PipelineOperator("extension")]
	public static PKString Extension(PKFileInfo info, PKItem[] args)
	{
		return new PKString(info.Value.Extension);
	}

	[PipelineOperator("creation-time")]
	public static PKDateTime CreationTime(PKFileInfo info, PKItem[] args)
	{
		return new PKDateTime(info.Value.CreationTime);
	}

	[PipelineOperator("last-access-time")]
	public static PKDateTime LastAccessTime(PKFileInfo info, PKItem[] args)
	{
		return new PKDateTime(info.Value.LastAccessTime);
	}

	[PipelineOperator("last-write-time")]
	public static PKDateTime LastWriteTime(PKFileInfo info, PKItem[] args)
	{
		return new PKDateTime(info.Value.LastWriteTime);
	}

	public static bool HasExtension(PKFileInfo info, PKItem[] args)
	{
		BuiltinHelpers.CheckArgumentCount(args, 1);
		//todo: make my own asserts
		var extension = BuiltinHelpers.GetArgument<PKString>(args[0], "file extension");
		if (!extension.Value.StartsWith('.'))
		{
			extension.Value = "." + extension.Value;
		}

		extension.Value = extension.Value.ToLowerInvariant();
		return info.Value.Extension.ToLowerInvariant() == extension.Value;
	}

	#region DirectoryInfo

	[FilterOperator("exists", typeof(PKDirectoryInfo))]
	public static bool Exists(PKDirectoryInfo dirInfo, PKItem[] args)
	{
		return dirInfo.Value.Exists;
	}

	[PipelineOperator("create-if-needed")]
	public static PKDirectoryInfo CreateIfNoExist(PKDirectoryInfo dirInfo, PKItem[] args)
	{
		if (!dirInfo.Value.Exists)
		{
			dirInfo.Value.Create();
		}
		return dirInfo;
	}

	[PipelineOperator("extract")]
	public static PKDirectoryInfo ExpandArchive(PKFileInfo info, PKItem[] args)
	{
		if (!info.Value.Exists)
		{
			throw new Exception($"Cannot expand archive. Cannot find {info.Value.FullName}.");
		}

		DirectoryInfo outputDir = null;
		if (args.Length == 1)
		{
			//get output directory from first argument.
		}

		if (args.Length == 0)
		{
			var containingDir = info.Value.Directory;
			var name = Path.GetFileNameWithoutExtension(info.Value.Name);
			outputDir = new DirectoryInfo(Path.Combine(containingDir.FullName, name));
		}

		try
		{
			using (var ps = PowerShell.Create())
			{
				ps.AddScript("Set-ExecutionPolicy -ExecutionPolicy ByPass -Scope Process");
				ps.AddScript($"Import-Module Microsoft.PowerShell.Archive; Expand-Archive -Path {info.Value.FullName} -DestinationPath {outputDir.FullName+"/"}");
				
				// Command cmd = new Command("Expand-Archive");
				// cmd.Parameters.Add("Path", info.Value.FullName);
				// cmd.Parameters.Add("DestinationPath", outputDir.FullName);
				// ps.Commands.AddCommand(cmd);
				var res = ps.Invoke();

				
			}
		}
		catch (Exception e)
		{
			Console.Error.WriteLine("Error during executing powershell cmdlet Extract-Archive.");
			throw;
		}
		
		return new PKDirectoryInfo(outputDir);
	}


	[PipelineOperator("nav-up")]
	public static PKDirectoryInfo NavigateUp(PKDirectoryInfo dirInfo, PKItem[] args)
	{
		if (dirInfo.Value.Parent != null)
		{
			return new PKDirectoryInfo(dirInfo.Value.Parent);
		}

		return dirInfo;
	}
	

	#endregion
}