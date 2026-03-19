using System.Diagnostics;

namespace PocketKnifeCore;

public static class BuiltinInputProviders
{
    public static Dictionary<string, Func<PKItem[], Dictionary<string,PKItem>?, IPKInputProvider>> InputProviders =
        new Dictionary<string, Func<PKItem[], Dictionary<string,PKItem>?,IPKInputProvider>>()
        {
            {
                "dir", (a,o) =>
                {
                    TraversalOrder order = TraversalOrder.ItemByItem;
                    
                    //todo: helper for getting properties from the dict
                    if (o != null && o.ContainsKey("order"))
                    {
                        //string or identifier should become tostring
                        var orderProvided = o["order"].ToString().ToLower();
                        if (orderProvided == "item")
                        {
                            order = TraversalOrder.ItemByItem;
                        }else if (orderProvided == "command")
                        {
                            order = TraversalOrder.CommandByCommand;
                        }
                    }
                    
                    //todo: make my own asserts
                    BuiltinHelpers.CheckArgumentCount(a, 1);
                    var path = BuiltinHelpers.GetArgument<PKString>(a[0], "directory path");
                    return new PKDirectoryInfo(path,order);
                }
            }
        };
}