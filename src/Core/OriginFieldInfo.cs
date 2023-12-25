using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCsv.Core;

public record OriginFieldInfo(string Header, Dictionary<string,string>? Options);