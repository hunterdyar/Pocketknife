namespace PocketKnifeCore;

class Program
{
    static void Main(string[] args)
    {
        var p = Blender.GetBlenderPath();
        Console.WriteLine(p);
    }
}