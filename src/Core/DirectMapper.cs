namespace LightCsv.Core;

public class DirectMapper(bool hidden, int index) : ICsvFieldMapper
{
    public void Map(string[] origin, string[] target)
    {
        if (index >= origin.Length) return;

        target[index] = hidden ? "(hidden)" : origin[index];
    }
}