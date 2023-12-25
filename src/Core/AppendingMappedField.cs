using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCsv.Core;

public class AppendingMappedField : IMappedField
{
    public AppendingMappedField(string name, string alias, string @default, int index)
    {
        Name = name;
        Alias = alias;
        Default = @default;
        Index = index;
    }

    public string Name { get; }
    public string Alias { get; }
    public string Default { get; }
    public int Index { get; }
    public void RunMap(string statement, string[] target)
    {
        if (string.IsNullOrEmpty(target[Index]))
        {
            target[Index] = statement;
        }
        else
        {
            target[Index] += $";{statement}";
        }
        
    }
}