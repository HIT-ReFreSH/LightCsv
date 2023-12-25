using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCsv.Core;

internal class SettingMappedField:IMappedField 
{
    public SettingMappedField(string name, string alias, string @default, int index)
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
        target[Index] = statement;
    }
}