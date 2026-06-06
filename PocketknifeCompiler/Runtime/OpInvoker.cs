using System.Linq.Expressions;
using System.Reflection;

namespace PocketknifeCore;

//todo: I don't think we will use Context context, ultimately? or if we do.... it will be a different thing. like path and env and consts and variable lookup.
//but the evaluator will pass that into these functions, they won't get them and do that stuff themselves?
public delegate PKValue OpInvoker(PKValue input, PKValue[] args, Context context);

//todo: i guess we should a more flexible thing than a list. if we want to manage parallel and such? but not sure how to "transform an IEnumerable"... IList?
public delegate List<PKValue> GenInvoker(PKValue[] args, Context context);

