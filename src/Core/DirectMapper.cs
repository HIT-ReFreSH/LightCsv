namespace LightCsv.Core;

public class DirectMapper(int index) : ICsvFieldMapper
{
    public void Map(string[] origin, string[] target)
    {
        if (index >= origin.Length) return;

        target[index] = origin[index];
    }
}