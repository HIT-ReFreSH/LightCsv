using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCsv.Core;

public interface IMappedField
{
    string Name { get; }
    string Alias { get; }
    string Default { get; }
    int Index { get; }
    void RunMap(string statement, string[] target);
}

public record MappedFieldConfig(string Name, string Alias, string Type, string Default)
{
    public IMappedField Create(int index)
    {
        return Type switch
        {
            "bul" => new FlagMappedField(Name, Alias, index, Default == "+"),
            "num" => new NumberMappedField(Name, Alias, double.Parse(Default), index),
            "seq" => new AppendingMappedField(Name, Alias, Default, index),
            "str" => new SettingMappedField(Name, Alias, Default, index),
            _=>throw new ArgumentOutOfRangeException(nameof(Type)),
        };
    }
}
