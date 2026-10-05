using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using YamlDotNet.RepresentationModel;
using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;

public partial class Patcher
{

    [JSExport]
    public static async Task<bool> PatchCeleste(bool installEverest)
    {
        try
        {
            Patcher patcher;
            if (File.Exists("/libsdl/Celeste.dll"))
            {
                patcher = new("/libsdl/Celeste.dll");
            }
            else if (File.Exists("/libsdl/Celeste.exe"))
            {
                patcher = new("/libsdl/Celeste.exe");
            }
            else
            {
                throw new Exception("Celeste.dll or Celeste.exe not found!");
            }

            patcher.installEverest = installEverest;
            patcher.patch();
            patcher.write("/libsdl/CustomCeleste.dll");

            return true;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in PatchCeleste()!");
            Console.Error.WriteLine(e);
            return false;
        }
    }

    [JSExport]
    public static async Task<bool> ExtractEverest()
    {
        try
        {
            string everestPath = "/libsdl/Celeste/Everest/";
            Directory.CreateDirectory(everestPath);
            using (ZipArchive archive = ZipFile.OpenRead("/libsdl/everest.zip"))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith("/")) continue;
                    string path = everestPath + entry.FullName.Substring(entry.FullName.IndexOf('/') + 1);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    entry.ExtractToFile(path, true);
                }
            }

            File.Delete("/libsdl/everest.zip");
            return true;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in ExtractEverest()!");
            Console.Error.WriteLine(e);
            return false;
        }
    }

    [JSExport]
    public static Task<bool> ConsumeRestartRequest()
    {

        try
        {
            if (AppDomain.CurrentDomain.GetData("EverestRestart") != null)
            {
                AppDomain.CurrentDomain.SetData("EverestRestart", null);
                return Task.FromResult(true);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: failed to read restart request: {ex.Message}");
        }
        return Task.FromResult(false);
    }

    [JSExport]
    public static Task<string> GetInstalledMods()
    {

        var mods = new List<string>();
        try
        {
            string dir = "/libsdl/Celeste/Mods";
            if (Directory.Exists(dir))
            {
                foreach (string zip in Directory.EnumerateFiles(dir, "*.zip"))
                {
                    string fileName = Path.GetFileName(zip);

                    if (ModMetaCache.TryGetValue(fileName, out var cached))
                    {
                        mods.Add(cached);
                        continue;
                    }
                    string name = "";
                    string version = "";
                    var deps = new List<string>();
                    try
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(zip))
                        {
                            var entry = archive.GetEntry("everest.yaml");
                            if (entry != null)
                            {
                                using (var reader = new StreamReader(entry.Open()))
                                {
                                    var manifest = ModManifest.Parse(reader.ReadToEnd());
                                    name = manifest.Name;
                                    version = manifest.Version;
                                    deps.AddRange(manifest.Dependencies.Select(dep =>
                                        "{\"name\":" + JsonQuote(dep.Name)
                                        + ",\"version\":" + JsonQuote(dep.Version)
                                        + ",\"optional\":" + (dep.Optional ? "true" : "false") + "}"));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Warning: failed to read mod manifest in '{zip}': {ex.Message}");
                    }
                    if (name != "")
                    {
                        var json = "{\"file\":" + JsonQuote(fileName) + ",\"name\":" + JsonQuote(name) + ",\"version\":" + JsonQuote(version) + ",\"dependencies\":[" + string.Join(",", deps) + "]}";
                        ModMetaCache[fileName] = json;
                        mods.Add(json);
                    }
                }

                var present = Directory.EnumerateFiles(dir, "*.zip")
                    .Select(Path.GetFileName)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var stale in ModMetaCache.Keys.Where(k => !present.Contains(k)).ToList())
                    ModMetaCache.Remove(stale);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: failed to list installed mods: {ex.Message}");
        }
        return Task.FromResult("[" + string.Join(",", mods) + "]");
    }

    private static readonly Dictionary<string, string> ModMetaCache =
        new(StringComparer.OrdinalIgnoreCase);

    private static string JsonQuote(string value)
    {
        var sb = new System.Text.StringBuilder(value.Length + 2);
        sb.Append('"');
        foreach (char c in value)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20)
                        sb.Append("\\u").Append(((int)c).ToString("x4"));
                    else
                        sb.Append(c);
                    break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }

    public ModuleDefinition Module;
    public ReaderParameters parameters;
    public bool installEverest = false;

    public Patcher(string path)
    {
        parameters = new(ReadingMode.Immediate) { ReadSymbols = false, InMemory = true };
        Module = ModuleDefinition.ReadModule(new MemoryStream(File.ReadAllBytes(path)), parameters);
    }

    private void RunMonoMod(ModuleDefinition module, List<ModuleReference> mods, bool coreify, Action<MonoModder> callback)
    {
        RunMonoMod(module, mods, modder =>
        {
            if (coreify)
            {
                modder.Log($"Converting {module.Name} to .NET Core");
                NETCoreifier.Coreifier.ConvertToNetCore(modder);
            }

            modder.MapDependencies();
            callback(modder);
            modder.AutoPatch();
        });
    }

    private void RunMonoMod(ModuleDefinition module, List<ModuleReference> mods, Action<MonoModder> callback)
    {
        using (MonoModder modder = new()
        {
            Module = module,
            Mods = mods,
            MissingDependencyThrow = false,
            LogVerboseEnabled = false,
        })
        {
            modder.DependencyDirs.Add("/bin");
            modder.DependencyDirs.Add("/libsdl/Celeste/Everest");

            callback(modder);
        }
    }

    public void patch()
    {
        if (installEverest)
        {
            string everestPath = "/libsdl/Celeste/Everest/Celeste.Mod.mm.dll";
            string mmhookPath = "/libsdl/Celeste/Everest/MMHOOK_Celeste.dll";

            RunMonoMod(Module, [ModuleDefinition.ReadModule(everestPath)], true, modder =>
            {
                modder.Log("Installing Everest");
            });

            RunMonoMod(Module, [], modder =>
            {
                modder.MapDependencies();

                modder.Log("Generating MMHOOK_Celeste.dll");

                HookGenerator gen = new(modder, Path.GetFileName(mmhookPath))
                {
                    HookPrivate = true,
                };

                gen.Generate();

                gen.OutputModule.Write(mmhookPath);
            });

            var everest = ModuleDefinition.ReadModule(everestPath);
            foreach (var type in everest.Types)
            {
                type.Resolve();
                foreach (var attr in type.CustomAttributes)
                {
                    var _ = attr.HasConstructorArguments;
                }
            }

        }

        ModuleDefinition wasmMod = ModuleDefinition.ReadModule("/bin/Celeste.Wasm.mm.dll");
        if (!installEverest)
        {
            var ignore = wasmMod.ImportReference(typeof(MonoMod.MonoModIgnore).GetConstructor([]));
            foreach (var type in wasmMod.GetTypes())
            {
                if (type.Namespace.StartsWith("Celeste.Mod"))
                    type.CustomAttributes.Add(new(ignore));
            }
            foreach (var type in wasmMod.GetTypes())
            {
                if (type.Namespace == "Celeste.Wasm.NonEverestOnly")
                    type.CustomAttributes.Clear();
            }
        }

        RunMonoMod(Module, [wasmMod], false, modder =>
        {
            modder.Log("Installing WASM patches");
            modder.DependencyMap[modder.Module].Add(wasmMod);
        });

        Module.AssemblyReferences.Add(wasmMod.Assembly.Name);
    }

    public void write(string path)
    {
        Module.Write(path, new WriterParameters() { WriteSymbols = false });
    }
}
