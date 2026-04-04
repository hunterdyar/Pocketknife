namespace PocketKnifeCore.Engine;

public class PathAssembler
{
	private string[] _pieces;
	private PathSegment[] _segments;
	public PathAssembler(params string[] pieces)
	{
		_pieces = pieces;
		_segments = CreateSegments(pieces);
	}

	private PathSegment[] CreateSegments(string[] pieces)
	{
		return pieces.Select(x => new PathSegment(x)).ToArray();
	}

	private string Assemble()
	{
		//if we know that the file should exist, we can get better answers (dir or file, etc)
			//with File.GetAttributes. we can declare for sure if it's a dir or file or so on.
				//so we check that, whythefucknot.
		
		
		//our goal is to provide reasonable assists to this function.
			//insert trailing or preceding directory seperator characters if neccesary.
				//it's assumed (what we know that Path.combine does not):
					//that our pieces MUST be directory gaps except for the last 2
					//the last 1 is a dir, or filename, or just extension.
					//if las (could be just) extension, the second to last is a dir or filename
						//.git (dir), .gitignore (file) bro nobody knows anything.
		
		return Path.Combine(_pieces);
	}
	
	
	
	
}

public struct PathSegment
{
	public string Piece;
	public bool HasExt;
	public string Ext;
	public string Filename;
	public PathSegment(string piece)
	{
		if (piece.StartsWith("."))
		{
			if (Piece.Count(x => x == '.') == 1)
			{
				//this is a 'hidden directory' or an extension.
				//wait, extensions can have multiple dots too. .tar.gz, .xaml.cs
				//and so cna directories.
				
				
				
				//man, fuck computers.
			}
		}
		Piece = Path.TrimEndingDirectorySeparator(piece);
		HasExt = Path.HasExtension(Piece);
		Ext = Path.GetExtension(Piece);
		Filename = Path.GetFileNameWithoutExtension(Piece);
	}
}