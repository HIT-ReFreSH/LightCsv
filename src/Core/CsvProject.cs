using System.Text.Encodings.Web;
using System.Text.Json;

namespace LightCsv.Core;

public class CsvProject
{
    private readonly string _header;
    private readonly List<ICsvFieldMapper> _mappers = new();
    private readonly string _origin;

    private readonly Func<string[]> _targetFactory;

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
            for (var i = sfc; i < targetFieldCount; i++) r[i] = setting.MappedFields[i - sfc].Default;

            return r;
        };
    }

    public void ProcessFile(string target)
    {
        using var input = new StreamReader(File.OpenRead(_origin));
        using var output = new StreamWriter(File.OpenWrite(target));
        _ = input.ReadLine();
        output.WriteLine(_header);
        for (;;)
        {
            var line = input.ReadLine();
            if (line is null) break;
            var values = line.Split(",");
            output.WriteLine(string.Join(",", ProcessLine(values)));
        }

        output.Close();
        input.Close();
    }

    public void ProcessFileToMarkdown(string target, string appendix)
    {
        using var input = new StreamReader(File.OpenRead(_origin));
        using var output = new StreamWriter(File.OpenWrite(target));
        var appendixLines = File.ReadAllText(appendix);
        _ = input.ReadLine();
        var headers = _header.Split(", ");
        output.WriteLine($"# {_origin}\n");
        for (;;)
        {
            var line = input.ReadLine();
            if (line is null) break;
            var values = line.Split(",");
            var mappedValues = ProcessLine(values);
            if (mappedValues.Length > 1 && mappedValues.Length == headers.Length)
            {
                output.WriteLine($"## {mappedValues[0]}\n");
                for (var i = 1; i < mappedValues.Length; i++) output.WriteLine($"- {headers[i]}: {mappedValues[i]}");

                output.WriteLine();

                output.WriteLine(appendixLines);
            }
            else
            {
                throw new Exception($"Length not matched! {mappedValues}, {headers}");
            }
        }

        output.Close();
        input.Close();
    }

    private string[] ProcessLine(string[] origin)
    {
        var target = _targetFactory();
        foreach (var mapper in _mappers) mapper.Map(origin, target);

        return target;
    }
}

public record CsvProjectSetting(string Origin, List<OriginFieldInfo> OriginFields, List<MappedFieldConfig> MappedFields)
{
    public CsvProject Build() { return new CsvProject(this); }

    public void ToFile(string path)
    {
        File.WriteAllText
        (
            path,
            JsonSerializer.Serialize
            (
                this,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }
            )
        );
    }

    public static CsvProjectSetting FromFile(string fileName)
    {
        return JsonSerializer.Deserialize<CsvProjectSetting>
        (
            File.ReadAllText(fileName),
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }
        )!;
    }

    public static CsvProjectSetting ParseCsv(string fileName, Func<string[], bool[]> includeSelector)
    {
        using var file = new StreamReader(File.OpenRead(fileName));
        var header = file.ReadLine();
        if (header is null) throw new Exception("Empty CSV!");
        var headers = header.Split(',');
        var included = includeSelector(headers);
        var fields = headers.Select((h, i) => new OriginFieldInfo(h, included[i] ? [] : null)).ToList();
        ;
        for (;;)
        {
            var line = file.ReadLine();
            if (line is null) break;
            var values = line.Split(",");
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (string.IsNullOrEmpty(value)) continue;
                var vspl = value.Split(';');
                foreach (var v in vspl) fields[i].Options?.TryAdd(v, string.Empty);
            }
        }

        file.Close();
        return new CsvProjectSetting(fileName, fields, new List<MappedFieldConfig>());
    }
}