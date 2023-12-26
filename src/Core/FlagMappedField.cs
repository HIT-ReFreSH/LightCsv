using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCsv.Core;

public class FlagMappedField(string name, string alias, int index, bool @default) : IMappedField
{
    public string Name { get; } = name;
    public string Alias { get; } = alias;
    public string Default { get; } = @default ? "+" : "-";
    public int Index { get; } = index;
    public void RunMap(string statement, string[] target) { target[Index] = statement; }
}