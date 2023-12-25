using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace LightCsv.Core;


public class CsvProject
{
    private readonly List<ICsvFieldMapper> _mappers = new();
    private readonly string _header;
    private readonly string _origin;
    public CsvProject(CsvProjectSetting setting)
    {
        _origin = setting.Origin;
        var originFields = setting.OriginFields;
        var sfc = originFields.Count;
        var targetFieldCount = sfc + setting.MappedFields.Count;
        _mappers.AddRange(originFields.Select((_, i) => new DirectMapper(i)));
        var mapProgram = new MapProgram(setting.MappedFields.Select((f, i) => f.Create(sfc + i)));
        var headers = new List<string>();
        foreach (var (f, i) in originFields.Select((i, f) => (i, f)))
        {
            headers.Add(f.Header);
            if (f.Options is null) continue;
            _mappers.Add(new ProgrammableMapper(i, f, mapProgram.Executor));
        }
        headers.AddRange(setting.MappedFields.Select(f => f.Name));
        _header = string.Join(", ", headers);
        _targetFactory = () =>
        {
            var r = new string[targetFieldCount];
            for (var i = sfc; i < targetFieldCount; i++)
            {
                r[i] = setting.MappedFields[i - sfc].Default;
            }
            return r;
        };
    }

    public void ProcessFile(string target)
    {
        using var input = new StreamReader(File.OpenRead(_origin));
        using var output = new StreamWriter(File.OpenWrite(target));
        _ = input.ReadLine();
        output.WriteLine(_header);
        for (; ; )
        {
            var line = input.ReadLine();
            if (line is null) break;
            var values = line.Split(",");
            output.WriteLine(string.Join(",", ProcessLine(values)));
        }
        output.Close();
        input.Close();
    }

    private Func<string[]> _targetFactory;
    private string[] ProcessLine(string[] origin)
    {
        var target = _targetFactory();
        foreach (var mapper in _mappers)
        {
            mapper.Map(origin, target);
        }
        return target;
    }
}
public record CsvProjectSetting(string Origin, List<OriginFieldInfo> OriginFields, List<MappedFieldConfig> MappedFields)
{
    public CsvProject Build() => new(this);
    public void ToFile(string path) => File.WriteAllText(path, JsonSerializer.Serialize(this,options: new(JsonSerializerDefaults.Web)
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    }));

    public static CsvProjectSetting FromFile(string fileName)
        => JsonSerializer.Deserialize<CsvProjectSetting>(File.ReadAllText(fileName), options: new(JsonSerializerDefaults.Web)
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        })!;
    public static CsvProjectSetting ParseCsv(string fileName, Func<string[], bool[]> includeSelector)
    {
        using var file = new StreamReader(File.OpenRead(fileName));
        var header = file.ReadLine();
        if (header is null) throw new Exception("Empty CSV!");
        var headers = header.Split(',');
        var included = includeSelector(headers);
        var fields = headers.Select((h, i) => new OriginFieldInfo(h, included[i] ? new() : null)).ToList();
        ;
        for (; ; )
        {
            var line = file.ReadLine();
            if (line is null) break;
            var values = line.Split(",");
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if(string.IsNullOrEmpty(value)) continue;
                var vspl=value.Split(';');
                foreach (var v in vspl)
                {
                    fields[i].Options?.TryAdd(v, string.Empty);
                }
            }
        }

        file.Close();
        return new(fileName, fields, new());
    }
}