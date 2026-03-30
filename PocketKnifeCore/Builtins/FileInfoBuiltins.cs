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
	

		var output = new PKDirectoryInfo(TraversalOrder.ItemByItem);
		output.Value = outputDir;
		return output;
	}
}