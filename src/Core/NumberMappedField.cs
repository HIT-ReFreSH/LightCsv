using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCsv.Core;

public class NumberMappedField:IMappedField
{
    public NumberMappedField(string name, string alias, double @default, int index)
    {
        Name = name;
        Alias = alias;
        Default = @default.ToString(CultureInfo.InvariantCulture);
        Index = index;
    }

    public string Name { get; }
    public string Alias { get; }
    public string Default { get; }
    public int Index { get; }
    public void RunMap(string statement, string[] target)
    {
        var @operator = statement[0];
        var operand= double.Parse( statement[1..]);
        var origin = double.Parse(target[Index]);
        target[Index] = (@operator switch
        {
            '='=>operand,
            '-'=>origin-operand,
            '+' => origin + operand,
            '*' => origin * operand,
            '/' => origin / operand,

            _ => throw new ArgumentOutOfRangeException(nameof(@operator))
        }).ToString(CultureInfo.InvariantCulture);
    }
}