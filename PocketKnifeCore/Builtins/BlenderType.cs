using System.Diagnostics;
using System.Management.Automation;

namespace PocketKnifeCore;

public class Blender : PKItem<FileInfo>
{
	public override string Type => "blender";

	public Blender(FileInfo blendFile) : base(blendFile)
	{
		if (blendFile.Extension != ".blend")
		{
			throw new Exception("invalid filetype to create blend file.");
		}
	}

	public static void RenderFrame()
	{
		GetBlenderPath();
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		startInfo.FileName = "cmd.exe";
		startInfo.Arguments = "/C copy /b Image1.jpg + Archive.rar Image2.jpg";
		process.StartInfo = startInfo;
		process.Start();
	}

	public static PSObject GetBlenderPath()
	{
		var ps = PowerShell.Create();
		ps.AddScript("""
		              function Get-BlenderPath {
		                  try {
		                      # 1. Try reading from registry (64-bit and 32-bit locations)
		                      $regPaths = @(
		                          "HKLM:\SOFTWARE\BlenderFoundation\Blender",
		                          "HKLM:\SOFTWARE\WOW6432Node\BlenderFoundation\Blender"
		                      )

		                      foreach ($regPath in $regPaths) {
		                          if (Test-Path $regPath) {
		                              $installPath = (Get-ItemProperty -Path $regPath).Install_Dir
		                              if ($installPath -and (Test-Path $installPath)) {
		                                  return $installPath
		                              }
		                          }
		                      }

		                      # 2. Check common install directories
		                      $commonPaths = @(
		                          "$Env:ProgramFiles\Blender Foundation\Blender",
		                          "$Env:ProgramFiles\Blender Foundation\Blender 5.0",
		                          "$Env:ProgramFiles(x86)\Blender Foundation\Blender"
		                      )

		                      foreach ($path in $commonPaths) {
		                          if (Test-Path $path) {
		                              return $path
		                          }
		                      }

		                      # 3. Try finding blender.exe in PATH
		                      $blenderExe = Get-Command blender.exe -ErrorAction SilentlyContinue
		                      if ($blenderExe) {
		                          return Split-Path $blenderExe.Source -Parent
		                      }

		                      return $null
		                  }
		                  catch {
		                      Write-Error "Error while searching for Blender path: $_"
		                      return $null
		                  }
		              }

		              # Run the function
		              $blenderPath = Get-BlenderPath

		              if ($blenderPath) {
		                  Write-Host "Blender installation path: $blenderPath"
		              } else {
		                  Write-Host "Blender not found on this system."
		              }
		              return $blenderPath
		              """);
		var result = ps.Invoke();
		foreach (var info in ps.Streams.Information)
		{
			Debug.WriteLine(info.ToString());	
		}
		return result.First();
	}
}
