using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using System.Collections.Generic;
using System.IO.Compression;
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
                patcher.installEverest = installEverest;
            }
            else
            {
                throw new Exception("Celeste.dll or Celeste.exe not found!");
            }

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

            var mmhook = ModuleDefinition.ReadModule(mmhookPath);
            RunMonoMod(mmhook, [everest], false, modder =>
            {
                modder.Log("Patching MMHOOK_Celeste.dll");
            });
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
