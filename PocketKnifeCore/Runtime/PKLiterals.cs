namespace PocketKnifeCore;

public class PKString : PKItem<string>
{
    public PKString(string value) : base(value)
    {
    }
}

public class PKNumber : PKItem<double>
{
    public PKNumber(double value) : base(value)
    {
    }
}
