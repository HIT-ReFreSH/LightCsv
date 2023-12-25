using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCsv.Core;

public class FlagMappedField :IMappedField
{
    public FlagMappedField(string name, string alias, int index, bool @default)
    {
        Name = name;
        Alias = alias;
        Default = @default?"+":"-";
        Index = index;
    }

    public string Name { get; }
    public string Alias { get; }
    public string Default { get; }
    public int Index { get; }
    public void RunMap(string statement, string[] target)
    {
        target[Index] = statement;
    }
}