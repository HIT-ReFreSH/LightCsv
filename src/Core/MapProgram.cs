namespace LightCsv.Core;

public class MapProgram
{
    private readonly IReadOnlyDictionary<string, IMappedField> _mappedFields;

    //private List<string,string> _unmappedFields=new();
    public MapProgram(IEnumerable<IMappedField> mappedFields)
    {
        _mappedFields = mappedFields.ToDictionary(f => f.Alias);

        Executor = ExecutorInternal;
    }

    public MapProgramExecutor Executor { get; }

    private void ExecutorInternal(string statementSequence, string[] target)
    {
        if (string.IsNullOrWhiteSpace(statementSequence)) return;
        var statements = statementSequence.Split(';');
        foreach (var statement in statements.Where(s => !string.IsNullOrEmpty(s)))
        {
            var statementSpl = statement.Split(":", 2);
            var field = statementSpl[0];
            var operation = statementSpl[1];
            _mappedFields[field].RunMap(operation, target);
        }
    }
}