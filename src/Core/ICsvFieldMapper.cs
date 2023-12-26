namespace LightCsv.Core;

internal interface ICsvFieldMapper
{
    void Map(string[] origin, string[] target);
}