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
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		startInfo.FileName = "cmd.exe";
		startInfo.Arguments = "/C copy /b Image1.jpg + Archive.rar Image2.jpg";
		process.StartInfo = startInfo;
		process.Start();
	}
}
