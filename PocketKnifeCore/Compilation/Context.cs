using System.Security.Cryptography;

namespace PocketKnifeCore.Engine;

public class Context
{
	public List<string> TypeHistory = new List<string>();
	public PKItem Item;
	public bool KeepProcessing = true;
	public Context? Parent;
	//todo:string should be RuntimeLabel?
	private Dictionary<string, Context> _namedBranches;
	private Context()
	{
	}

	public Context(PKItem item)
	{
		Item = item;
	}
	public void SetNamedBranch(string key, Context value)
	{
		LazySetNamedBranchesDict();
		if (!_namedBranches.TryAdd(key, value))
		{
			_namedBranches[key] = value;
		}
	}

	public bool TryGetNamedBranch(string key, out PKItem value)
	{
		LazySetNamedBranchesDict();
		if (_namedBranches.TryGetValue(key, out var context))
		{
			value = context.Item;
			return true;
		}
		else
		{
			if (Parent != null)
			{
				return Parent.TryGetNamedBranch(key, out value);
			}
			else
			{
				value = null;
				return false;
			}
		}
	}

	private void LazySetNamedBranchesDict()
	{
		if (_namedBranches == null)
		{
			_namedBranches = new Dictionary<string, Context>();
		}
	}

	/// <summary>
	/// Each branch (not each pipeline) keeps the context. We can find the context from a parent class by searching for the type of object there.
	/// At the root will be the original input branch.
	/// </summary>
	public bool TryFindItemAsTypeSearchUp<T>(out T item) where T : PKItem
	{
		if (Item.GetType() == typeof(T))
		{
			item = Item as T ?? throw new InvalidOperationException();
			return true;
		}else if (Parent != null)
		{
			return Parent.TryFindItemAsTypeSearchUp<T>(out item);
		}

		item = null;
		return false;
	}
	public Context PushDuplicate()
	{
		var newTop = new Context()
		{
			Item = this.Item,
			KeepProcessing = this.KeepProcessing,
			Parent = this
		};
		return newTop;
	}

	public override string ToString()
	{
		return "Ctx-top: "+this.Item.ToString();
	}
}