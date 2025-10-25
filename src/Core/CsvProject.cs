using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;

namespace LightCsv.Core;

public class CsvProject
{
    private readonly string[] _headers;
    private readonly List<ICsvFieldMapper> _mappers = new();
    private readonly string _origin;

    private readonly Func<string[]> _targetFactory;

    public CsvProject(CsvProjectSetting setting)
    {
        _origin = setting.Origin;
        var originFields = setting.OriginFields;
        var sfc = originFields.Count;
        var targetFieldCount = sfc + setting.MappedFields.Count;
        _mappers.AddRange(originFields.Select((f, i) => new DirectMapper(f.Hidden, i)));
        var mapProgram = new MapProgram(setting.MappedFields.Select((f, i) => f.Create(sfc + i)));
        var headers = new List<string>();
        foreach (var (f, i) in originFields.Select((i, f) => (i, f)))
        {
            if (!f.Hidden) headers.Add(f.Header);
            if (f.Options is null) continue;
            _mappers.Add(new ProgrammableMapper(i, f, mapProgram.Executor));
        }

        headers.AddRange(setting.MappedFields.Select(f => f.Name));
        _headers = headers.ToArray();
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
        using var csvReader = new CsvReader(input, CultureInfo.InvariantCulture);
        using var output = new StreamWriter(File.OpenWrite(target));
        using var csvWriter = new CsvWriter(output, CultureInfo.InvariantCulture);

        csvReader.Read();
        csvReader.ReadHeader();

        foreach (var header in _headers)
        {
            csvWriter.WriteField(header);
        }
        csvWriter.NextRecord();

        while (csvReader.Read())
        {
            var fieldCount = csvReader.Parser.Count;
            var values = new string[fieldCount];
            for (var i = 0; i < fieldCount; i++)
            {
                values[i] = csvReader.GetField(i) ?? string.Empty;
            }

            var processedValues = ProcessLine(values);
            foreach (var value in processedValues)
            {
                csvWriter.WriteField(value);
            }
            csvWriter.NextRecord();
        }
    }

    public void ProcessFileToMarkdown(string target, string appendix, Func<string[], bool[]> hiddenSelector)
    {
        using var input = new StreamReader(File.OpenRead(_origin));
        using var csvReader = new CsvReader(input, CultureInfo.InvariantCulture);
        using var output = new StreamWriter(File.OpenWrite(target));

        var appendixLines = File.ReadAllText(appendix);
        csvReader.Read();
        csvReader.ReadHeader();

        var hidden = hiddenSelector(_headers);
        output.WriteLine($"# {_origin}\n");

        while (csvReader.Read())
        {
            var fieldCount = csvReader.Parser.Count;
            var values = new string[fieldCount];
            for (var i = 0; i < fieldCount; i++)
            {
                values[i] = csvReader.GetField(i) ?? string.Empty;
            }

            var mappedValues = ProcessLine(values);
            if (mappedValues.Length > 1 && mappedValues.Length == _headers.Length)
            {
                output.WriteLine($"## {mappedValues[0]}\n");
                for (var i = 1; i < mappedValues.Length; i++)
                    if (!hidden[i])
                        output.WriteLine($"- {_headers[i]}: {mappedValues[i]}");

                output.WriteLine();

                output.WriteLine(appendixLines);
            }
            else
            {
                throw new Exception($"Length not matched! {mappedValues}, {_headers}");
            }
        }
    }

    private string[] ProcessLine(string[] origin)
    {
        var target = _targetFactory();
        foreach (var mapper in _mappers) mapper.Map(origin, target);

        return target.Where(v => v != "(hidden)").ToArray();
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

    public static CsvProjectSetting ParseCsv
        (string fileName, Func<string[], bool[]> includeSelector, Func<string[], bool[]> hiddenSelector)
    {
        using var file = new StreamReader(File.OpenRead(fileName));
        using var csv = new CsvReader(file, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? throw new Exception("Empty CSV!");

        var included = includeSelector(headers);
        var hidden = hiddenSelector(headers);
        var fields = headers.Select((h, i) => new OriginFieldInfo(h, included[i] ? [] : null, hidden[i])).ToList();

        while (csv.Read())
        {
            var fieldCount = csv.Parser.Count;
            for (var i = 0; i < fieldCount; i++)
            {
                var value = csv.GetField(i);
                if (string.IsNullOrEmpty(value)) continue;
                var vspl = value.Split(", ");
                foreach (var v in vspl) fields[i].Options?.TryAdd(v, string.Empty);
            }
        }

        return new CsvProjectSetting(fileName, fields, new List<MappedFieldConfig>());
    }
}