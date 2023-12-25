using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCsv.Core;

public class DirectMapper:ICsvFieldMapper
{
    private readonly int _index;
    public DirectMapper(int index)
    {
        _index = index;
    }

    public void Map(string[] origin, string[] target)
    {
        if (_index >= origin.Length)
        {
            return;}
        target[_index] = origin[_index];
    }
}