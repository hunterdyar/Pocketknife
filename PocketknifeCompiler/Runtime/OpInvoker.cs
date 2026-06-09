using System.Linq.Expressions;
using System.Reflection;

namespace PocketknifeCore;

//todo: I don't think we will use Context context, ultimately? or if we do.... it will be a different thing. like path and env and consts and variable lookup.
//but the evaluator will pass that into these functions, they won't get them and do that stuff themselves?
public delegate object OpInvoker(object input, object[] args, Context context);

public delegate List<object> GenInvoker(object[] args, Context context);

