using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCsv.Core;

internal interface ICsvFieldMapper
{
    void Map(string[] origin, string[] target);
}