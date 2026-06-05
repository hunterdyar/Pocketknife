using System.Linq.Expressions;
using System.Reflection;

namespace PocketknifeCore;

public delegate PKValue OpInvoker(PKValue input, ReadOnlySpan<PKValue> args, Context context);

