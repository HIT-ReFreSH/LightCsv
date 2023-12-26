using HitRefresh.MobileSuit;
using HitRefresh.MobileSuit.UI;
using LightCsv.Core;

namespace LightCsv;

[SuitInfo("LightCSV")]
public class Driver(IIOHub iO)
{
    [SuitAlias("c")]
    public void CreateProject(string csv, string projectFile)
    {
        var proj = CsvProjectSetting.ParseCsv
        (
            csv,
            headers =>
            {
                var selected = iO.CuiSelectItemsFrom("Select fields to Include as programmable", headers).ToHashSet();
                return headers.Select(h => selected.Contains(h)).ToArray();
            }
        );
        iO.WriteLineAsync("Input mapped fields: bul|num|seq|str name alias[ default], end with blank line.");
        for (;;)
        {
            var line = iO.ReadLine();
            if (string.IsNullOrEmpty(line)) break;
            var spl = line.Split(' ');
            if (spl.Length is < 3 or > 4)
            {
                iO.WriteLine("Invalid def: Too short or too long", OutputType.Warning);
                continue;
            }

            proj.MappedFields.Add(new MappedFieldConfig(spl[1], spl[2], spl[0], spl.Length == 3 ? "" : spl[3]));
        }

        proj.ToFile(projectFile);
    }

    [SuitAlias("x")]
    public void RunProject(string projectFile, string newCsv)
    {
        CsvProjectSetting.FromFile(projectFile).Build().ProcessFile(newCsv);
    }

    [SuitAlias("mdx")]
    public void RunProjectToMd(string projectFile, string newMd, string appendix)
    {
        CsvProjectSetting.FromFile(projectFile).Build().ProcessFileToMarkdown(newMd);
    }

    //[SuitAlias("code"), SuitAlias("$")]
    //public void RunVsCode(string projectFile)
    //{
    //    Process.Start("code", new[] { projectFile });
    //}
    [SuitAlias("cd")] public void ChangeDir(string dir) { Directory.SetCurrentDirectory(dir); }
}