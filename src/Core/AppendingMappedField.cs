namespace LightCsv.Core;

public class AppendingMappedField(string name, string alias, string @default, int index) : IMappedField
{
    public string Name { get; } = name;
    public string Alias { get; } = alias;
    public string Default { get; } = @default;
    public int Index { get; } = index;

    public void RunMap(string statement, string[] target)
    {
        if (string.IsNullOrEmpty(target[Index]))
            target[Index] = statement;
        else
            target[Index] += $";{statement}";
    }
}