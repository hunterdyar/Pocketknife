using System.Security.Cryptography;

namespace PocketKnifeCore.Engine;

public class Context
{
	public List<string> TypeHistory = new List<string>();
	public PKItem Item;
	public bool KeepProcessing = true;
	public Context? Parent;
	//todo:string should be RuntimeLabel?
	private Dictionary<string, PKItem> _assignedValues;
	private Context()
	{
	}

	public Context(PKItem item)
	{
		Item = item;
	}
	public void SetValue(string key, PKItem value, bool parentmost = true)
	{
		if (parentmost)
		{
			if (Parent != null)
			{
				Parent.SetValue(key, value, true);
				return;
			}
		}

		LazySetAssignmentDict();
		if (!_assignedValues.TryAdd(key, value))
		{
			_assignedValues[key] = value;
		}
	}

	public bool TryGetValue(string key, out PKItem value)
	{
		LazySetAssignmentDict();
		if (_assignedValues.TryGetValue(key, out  value))
		{
			return true;
		}
		else
		{
			if (Parent != null)
			{
				return Parent.TryGetValue(key, out value);
			}
			else
			{
				value = null;
				return false;
			}
		}
	}

	private void LazySetAssignmentDict()
	{
		if (_assignedValues == null)
		{
			_assignedValues = new Dictionary<string, PKItem>();
		}
	}

	public bool TryFindItemAsTypeSearchUp(string type, out PKItem item)
	{
		if (Item.Type == type)
		{
			item = Item;
			return true;
		}else if (Parent != null)
		{
			return Parent.TryFindItemAsTypeSearchUp(type, out item);
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