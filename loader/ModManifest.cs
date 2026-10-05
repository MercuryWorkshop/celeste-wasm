#nullable enable

using System;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

public sealed class ModManifest
{
    public string Name { get; private set; } = "";
    public string Version { get; private set; } = "";
    public List<ModDependency> Dependencies { get; } = new();

    public static ModManifest Parse(string yaml)
    {
        var result = new ModManifest();
        var stream = new YamlStream();
        using var reader = new System.IO.StringReader(yaml);
        stream.Load(reader);

        foreach (var document in stream.Documents)
        {
            switch (document.RootNode)
            {
                case YamlSequenceNode entries:
                    foreach (var entry in entries.Children)
                    {
                        if (entry is YamlMappingNode mapping)
                            result.ReadMod(mapping);
                    }
                    break;
                case YamlMappingNode mapping:
                    result.ReadMod(mapping);
                    break;
            }
        }
        return result;
    }

    private void ReadMod(YamlMappingNode mapping)
    {
        var name = ScalarValue(mapping, "Name");
        var version = ScalarValue(mapping, "Version");
        if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(Name))
            Name = name.Trim();
        if (!string.IsNullOrWhiteSpace(version) && string.IsNullOrWhiteSpace(Version))
            Version = version.Trim();

        ReadDependencies(mapping, "Dependencies", optional: false);
        ReadDependencies(mapping, "OptionalDependencies", optional: true);
    }

    private void ReadDependencies(YamlMappingNode mapping, string key, bool optional)
    {
        if (TryGet(mapping, key) is not YamlSequenceNode dependencies)
            return;
        foreach (var dependency in dependencies.Children)
        {
            if (dependency is not YamlMappingNode dependencyMapping) continue;
            var depName = ScalarValue(dependencyMapping, "Name");
            if (string.IsNullOrWhiteSpace(depName)) continue;
            Dependencies.Add(new ModDependency
            {
                Name = depName.Trim(),
                Version = ScalarValue(dependencyMapping, "Version").Trim(),
                Optional = optional,
            });
        }
    }

    private static YamlNode? TryGet(YamlMappingNode mapping, string key)
    {
        foreach (var pair in mapping.Children)
        {
            if (pair.Key is YamlScalarNode scalar
                && string.Equals(scalar.Value, key, StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        }
        return null;
    }

    private static string ScalarValue(YamlMappingNode mapping, string key)
    {
        return TryGet(mapping, key) is YamlScalarNode scalar ? scalar.Value ?? "" : "";
    }
}

public sealed class ModDependency
{
    public string Name { get; init; } = "";
    public string Version { get; init; } = "";
    public bool Optional { get; init; }
}
