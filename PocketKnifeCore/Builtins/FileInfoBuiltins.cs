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
}