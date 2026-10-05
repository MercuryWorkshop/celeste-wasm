using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Xna.Framework;
using MonoMod.Core.Platforms;
using MonoMod.Logs;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("browser")]

public static partial class CelesteLoader
{
    private static void Main()
    {
        Console.WriteLine(":3");
    }

    [DllImport("Emscripten")]
    public extern static void wasm_func_viil(Int32 x, Int32 y, Int64 l);

	[JSImport("XXHash64_Fast", "interop.js")]
	public static partial Task<JSObject> XXHash64_Fast(string file);

    internal static void CallPinvokeFixers()
    {
        wasm_func_viil(0, 0, 0);
        MonoMod.CustomBankLoader.PinvokeFix();
    }

    [JSExport]
    internal static Task PreInit()
    {
        return Task.Run(() =>
        {
            try
            {
                CallPinvokeFixers();

                Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL3");
                Environment.SetEnvironmentVariable("MONOMOD_DEPENDENCY_REMOVE_PATCH", "0");
                Environment.SetEnvironmentVariable("MONOMOD_WASM_ENABLE_MAGIC_OVERWRITE", "0");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error in PreInit()!");
                Console.Error.WriteLine(e);
                throw;
            }
        });
    }

    static Game game;
    static Assembly celeste;
    static FieldInfo RunApplication;

    [JSExport]
    internal static Task Init(bool tailcalls)
    {
        try
        {
            // Subscribe to Warning and Error logs from MonoMod
            DebugLog.Subscribe(LogLevelFilter.Warning | LogLevelFilter.Error, (source, time, level, msg) =>
            {
                Console.WriteLine($"[{source}] {level}: {msg}");
            });

            File.Delete("/bin/Celeste.exe");
            File.Delete("/bin/Celeste.dll");
            File.CreateSymbolicLink("/bin/Celeste.exe", "/libsdl/CustomCeleste.dll");
            File.CreateSymbolicLink("/bin/Celeste.dll", "/libsdl/CustomCeleste.dll");
            if (Directory.Exists("/libsdl/Celeste/Everest") &&
                File.Exists("/libsdl/Celeste/Everest/Celeste.Mod.mm.dll") &&
                File.Exists("/libsdl/Celeste/Everest/MMHOOK_Celeste.dll"))
            {
                File.CreateSymbolicLink("/bin/Celeste.Mod.mm.dll", "/libsdl/Celeste.Mod.mm.dll");
                File.CreateSymbolicLink("/bin/MMHOOK_Celeste.dll", "/libsdl/MMHOOK_Celeste.dll");

                File.Copy("/libsdl/Celeste/Everest/Celeste.Mod.mm.dll", "/libsdl/Celeste.Mod.mm.dll", true);
                File.Copy("/libsdl/Celeste/Everest/MMHOOK_Celeste.dll", "/libsdl/MMHOOK_Celeste.dll", true);
            }
            else
            {
                File.Delete("/libsdl/Celeste.Mod.mm.dll");
                File.Delete("/libsdl/MMHOOK_Celeste.dll");
                File.Delete("/bin/Celeste.Mod.mm.dll");
                File.Delete("/bin/MMHOOK_Celeste.dll");
            }

            celeste = Assembly.LoadFrom("/libsdl/CustomCeleste.dll");

            WasmDetourFactory.EnableTailCallDetours = tailcalls;
            WasmDetourFactory.Blacklist.Add("System.Private.CoreLib");
            WasmDetourFactory.Blacklist.Add("System.Collections");
            WasmDetourFactory.Blacklist.Add("System.Collections.Concurrent");
            WasmDetourFactory.Blacklist.Add("System.IO.Compression");
            WasmDetourFactory.Blacklist.Add("System.IO.Compression.FileSystem");
            WasmDetourFactory.Blacklist.Add("System.IO.Compression.ZipFile");
            WasmDetourFactory.Blacklist.Add("System.IO.Hashing");
            WasmDetourFactory.Blacklist.Add("mscorlib");
            WasmDetourFactory.Blacklist.Add("netstandard");
            WasmDetourFactory.Blacklist.Add("FNA");
            WasmDetourFactory.Blacklist.Add("Mono.Cecil");
            WasmDetourFactory.Blacklist.Add("Mono.Cecil.Mdb");
            WasmDetourFactory.Blacklist.Add("Mono.Cecil.Pdb");
            WasmDetourFactory.Blacklist.Add("Mono.Cecil.Rocks");
            WasmDetourFactory.Blacklist.Add("MonoMod.Core");
            WasmDetourFactory.Blacklist.Add("MonoMod.Patcher");
            WasmDetourFactory.Blacklist.Add("MonoMod.ILHelpers");
            WasmDetourFactory.Blacklist.Add("MonoMod.RuntimeDetour");
            WasmDetourFactory.Blacklist.Add("MonoMod.RuntimeDetour.HookGen");

            AssemblyLoadContext.Default.ResolvingUnmanagedDll += (assembly, name) =>
            {
                if (name == "SDL2") name = "SDL3";
                return NativeLibrary.Load(name, assembly, null);
            };
            AssemblyLoadContext.Default.Resolving += (ctx, name) =>
            {
                try
                {
                    Assembly asm;
                    if (name.Name == "Celeste") asm = ctx.LoadFromAssemblyPath($"/libsdl/CustomCeleste.dll");
                    else if (name.Name == "Celeste.Mod.mm") asm = ctx.LoadFromAssemblyPath($"/libsdl/Celeste.Mod.mm.dll");
                    else asm = ctx.LoadFromAssemblyPath($"/libsdl/Celeste/Everest/{name.Name}.dll");
                    return asm;
                }
                catch
                {
                    return null;
                }
            };

            JsSplash.Init(celeste);

            var Celeste = celeste.GetType("Celeste.Celeste");
            var Settings = celeste.GetType("Celeste.Settings");
            var Engine = celeste.GetType("Monocle.Engine");

            var MainThreadId = Celeste.GetField("_mainThreadId", BindingFlags.Static | BindingFlags.NonPublic);
            var AssemblyDirectory = Engine.GetField("AssemblyDirectory", BindingFlags.Static | BindingFlags.NonPublic);
            var SettingsInitialize = Settings.GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public);
            var GameConstructor = Celeste.GetConstructor([]);

            MainThreadId.SetValue(null, Thread.CurrentThread.ManagedThreadId);
            AssemblyDirectory.SetValue(null, "/");

            SettingsInitialize.Invoke(null, []);

            var Everest = celeste.GetType("Celeste.Mod.Everest");
            if (Everest != null)
            {
                var ParseArgs = Everest.GetMethod("ParseArgs", BindingFlags.Static | BindingFlags.NonPublic);
                ParseArgs.Invoke(null, [new string[] { }]);

				var XXH64_Fast = Everest.GetField("CelesteWasm_XXHash64Fast", BindingFlags.Static | BindingFlags.Public);
				if (XXH64_Fast != null) {
					Func<string, byte[]> XXH64 = (path) => {
						try {
							var ret = XXHash64_Fast(path).Result;
							return ret.GetPropertyAsByteArray("ret");
						} catch {
							return null;
						}
					};
					XXH64_Fast.SetValue(null, XXH64);
				}
            }

            Console.WriteLine($"CelesteWasm on {RuntimeInformation.FrameworkDescription}");
            game = (Game)GameConstructor.Invoke([]);
            RunApplication = Celeste.GetField("RunApplication", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in Init()!");
            Console.Error.WriteLine(e);
            return Task.FromException(e);
        }
        return Task.Delay(0);
    }

    [JSExport]
    internal static Task Cleanup()
    {
        try
        {
            celeste.GetType("Celeste.RunThread").GetMethod("WaitAll").Invoke(null, []);
            celeste.GetType("Celeste.Audio").GetMethod("Unload").Invoke(null, []);
            game.Dispose();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in Cleanup()!");
            Console.Error.WriteLine(e);
            return Task.FromException(e);
        }
        return Task.Delay(0);
    }

    [JSExport]
    internal static Task<bool> RunOneFrame()
    {
        try
        {
            game.RunOneFrame();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in RunOneFrame()!");
            Console.Error.WriteLine(e);
            return (Task<bool>)Task.FromException(e);
        }
        return Task.FromResult((bool)RunApplication.GetValue(game));
    }

    [JSExport]
    internal static Task MainLoop()
    {
        try
        {
            game.Run();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in MainLoop()!");
            Console.Error.WriteLine(e);
            return Task.FromException(e);
        }
        return Task.Delay(0);
    }

    [JSExport]
    internal static Task WatchMemoryUsage([JSMarshalAs<JSType.Function<JSType.Number, JSType.Boolean>>] Func<double, bool> callback)
    {
        return Task.Run(async () =>
        {
            while (true)
            {
                bool stop = callback((double)GC.GetTotalMemory(false) / (1024 * 1024));
				if (stop) break;
                await Task.Delay(1000 * 30);
            }
        });
    }
}
