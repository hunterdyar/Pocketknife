namespace PocketKnifeCore;

public class FileInfoBuiltins
{
	[PipelineOperator("filename", typeof(PKFileInfo))]
	public PKString Filename(PKFileInfo info)
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
	public bool Exists(PKFileInfo fileInfo)
	{
		return fileInfo.Value.Exists;
	}
}