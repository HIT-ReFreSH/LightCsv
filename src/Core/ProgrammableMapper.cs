using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCsv.Core;

public delegate void MapProgramExecutor(string statementSequence, string[] target);

public class ProgrammableMapper : ICsvFieldMapper
{
    private readonly int _index;
    private readonly OriginFieldInfo _originFieldInfo;
    private readonly MapProgramExecutor _executor;

    public ProgrammableMapper(int index, OriginFieldInfo originFieldInfo,MapProgramExecutor executor)
    {
        _index = index;
        _originFieldInfo = originFieldInfo;
        _executor = executor;
    }

    public void Map(string[] origin, string[] target)
    {
        var originLine = origin[_index].Split(';');
        foreach (var s in originLine)
        {
            if(string.IsNullOrEmpty(s)) continue;
            _executor(_originFieldInfo.Options[s], target);
        }
        
    }
}