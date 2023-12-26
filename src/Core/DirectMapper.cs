using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCsv.Core;

public class DirectMapper(int index) : ICsvFieldMapper
{
    public void Map(string[] origin, string[] target)
    {
        if (index >= origin.Length)
        {
            return;
        }

        target[index] = origin[index];
    }
}