using System.Globalization;

namespace LightCsv.Core;

public class NumberMappedField(string name, string alias, double @default, int index) : IMappedField
{
    public string Name { get; } = name;
    public string Alias { get; } = alias;
    public string Default { get; } = @default.ToString(CultureInfo.InvariantCulture);
    public int Index { get; } = index;

    public void RunMap(string statement, string[] target)
    {
        var @operator = statement[0];
        var operand = double.Parse(statement[1..]);
        var origin = double.Parse(target[Index]);
        target[Index] = (@operator switch
                         {
                             '=' => operand,
                             '-' => origin - operand,
                             '+' => origin + operand,
                             '*' => origin * operand,
                             '/' => origin / operand,

                             _ => throw new ArgumentOutOfRangeException(nameof(@operator))
                         }).ToString(CultureInfo.InvariantCulture);
    }
}