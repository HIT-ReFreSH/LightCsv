namespace LightCsv.Core;

public delegate void MapProgramExecutor(string statementSequence, string[] target);

public class ProgrammableMapper(int index, OriginFieldInfo originFieldInfo, MapProgramExecutor executor)
    : ICsvFieldMapper
{
    public void Map(string[] origin, string[] target)
    {
        var originLine = origin[index].Split(';');
        foreach (var s in originLine)
        {
            if (string.IsNullOrEmpty(s)) continue;
            executor(originFieldInfo.Options[s], target);
        }
    }
}