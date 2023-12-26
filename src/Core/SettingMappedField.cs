using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCsv.Core;

internal class SettingMappedField(string name, string alias, string @default, int index) : IMappedField
{
    public string Name { get; } = name;
    public string Alias { get; } = alias;
    public string Default { get; } = @default;
    public int Index { get; } = index;
    public void RunMap(string statement, string[] target) { target[Index] = statement; }
}