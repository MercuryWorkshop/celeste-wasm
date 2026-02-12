using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("browser")]

public static partial class StardewLoader
{
    private static void Main()
    {
        Console.WriteLine("Stardew Valley WASM Loader Initialized");
    }

    [DllImport("Emscripten")]
    public extern static void wasm_func_viil(Int32 x, Int32 y, Int64 l);

    internal static void CallPinvokeFixers()
    {
        wasm_func_viil(0, 0, 0);
        // MonoMod CustomBankLoader not available in this version
        // This is a WASM-specific utility that may not be needed
    }

    [JSExport]
    internal static Task PreInit()
    {
        return Task.Run(() =>
        {
            try
            {
                CallPinvokeFixers();

                // Setup FNA platform for WASM
                Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL3");
                Environment.SetEnvironmentVariable("MONOMOD_DEPENDENCY_REMOVE_PATCH", "0");
                
                // Setup Stardew specific paths
                Environment.SetEnvironmentVariable("StardewValleyPath", "/libsdl/");
                Environment.SetEnvironmentVariable("STARDEW_GAME_PATH", "/libsdl/");

                Console.WriteLine("[PreInit] FNA Platform set to SDL3");
                Console.WriteLine("[PreInit] Game paths configured");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error in PreInit()!");
                Console.Error.WriteLine(e);
                throw;
            }
        });
    }

    static object game;
    static Assembly stardew;
    static FieldInfo RunApplication;

    [JSExport]
    internal static Task Init()
    {
        try
        {
            // Create symbolic links for DLLs (if they don't exist)
            try { File.CreateSymbolicLink("/bin/StardewValley.exe", "/libsdl/CustomStardewValley.dll"); } catch { }
            try { File.CreateSymbolicLink("/bin/StardewValley.dll", "/libsdl/CustomStardewValley.dll"); } catch { }

            // Load the patched Stardew Valley assembly - try multiple candidate paths
            string[] candidates = new string[] {
                "/libsdl/CustomStardewValley.dll",
                "/libsdl/StardewValley.dll",
                "/libsdl/StardewValley.exe",
                "/bin/StardewValley.dll",
                "/bin/StardewValley.exe"
            };
            string loadedPath = null;
            foreach (var c in candidates)
            {
                try
                {
                    if (File.Exists(c))
                    {
                        stardew = Assembly.LoadFrom(c);
                        loadedPath = c;
                        break;
                    }
                }
                catch { }
            }

            // If not found yet, try scanning /libsdl for a matching dll
            if (stardew == null)
            {
                try
                {
                    if (Directory.Exists("/libsdl"))
                    {
                        foreach (var f in Directory.EnumerateFiles("/libsdl", "*.dll"))
                        {
                            if (Path.GetFileName(f).IndexOf("Stardew", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                try { stardew = Assembly.LoadFrom(f); loadedPath = f; break; } catch { }
                            }
                        }
                    }
                }
                catch { }
            }

            if (stardew == null)
            {
                var msg = "Could not find StardewValley assembly in /libsdl. Please put CustomStardewValley.dll or StardewValley.dll into the frontend/public/libsdl/ directory.";
                Console.Error.WriteLine(msg);
                throw new FileNotFoundException(msg);
            }
            Console.WriteLine($"[Init] Loaded StardewValley assembly from {loadedPath}");

            // Setup assembly resolution with patched detour behavior
            // MonoMod.Core.Platforms.WasmDetourFactory not available in this version
            // Assembly resolution will rely on AssemblyLoadContext

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
                    if (name.Name == "StardewValley")
                        asm = ctx.LoadFromAssemblyPath($"/libsdl/CustomStardewValley.dll");
                    else
                        asm = ctx.LoadFromAssemblyPath($"/libsdl/{name.Name}.dll");
                    return asm;
                }
                catch
                {
                    return null;
                }
            };

            Console.WriteLine("[Init] Assembly loading configured");

            // Get game class and initialize
            var StardewValleyClass = stardew.GetType("StardewValley.Program");
            var GameType = stardew.GetType("StardewValley.Game") ?? stardew.GetType("Monocle.Engine");
            var Settings = stardew.GetType("StardewValley.GameData");
            var Engine = stardew.GetType("Monocle.Engine");

            if (StardewValleyClass == null && GameType == null)
            {
                Console.Error.WriteLine("[Init] Could not find StardewValley.Program or Game class");
                throw new Exception("StardewValley assembly does not have expected structure");
            }

            var MainThreadId = StardewValleyClass?.GetField("_mainThreadId", BindingFlags.Static | BindingFlags.NonPublic);
            var AssemblyDirectory = Engine?.GetField("AssemblyDirectory", BindingFlags.Static | BindingFlags.NonPublic);

            if (MainThreadId != null)
                MainThreadId.SetValue(null, Thread.CurrentThread.ManagedThreadId);

            if (AssemblyDirectory != null)
                AssemblyDirectory.SetValue(null, "/");

            Console.WriteLine("[Init] Game main thread configured");

            // Create game instance
            var GameConstructor = GameType?.GetConstructor(Type.EmptyTypes);
            if (GameConstructor == null)
            {
                Console.Error.WriteLine("[Init] Could not find Game constructor");
                throw new Exception("Cannot construct game instance");
            }

            game = GameConstructor.Invoke(new object[] { });
            RunApplication = GameType?.GetField("RunApplication", BindingFlags.NonPublic | BindingFlags.Instance);

            Console.WriteLine("[Init] Game instance created successfully");
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
            if (game != null)
            {
                var disposeMethod = game.GetType()?.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance);
                if (disposeMethod != null)
                    disposeMethod.Invoke(game, null);

                if (game is IDisposable disposable)
                    disposable.Dispose();
            }

            Console.WriteLine("[Cleanup] Game disposed");
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
            if (game == null)
                return Task.FromResult(false);

            // Call RunOneFrame via reflection
            var method = game.GetType()?.GetMethod("RunOneFrame", BindingFlags.Public | BindingFlags.Instance);
            if (method != null)
                method.Invoke(game, null);
            else
                Console.Error.WriteLine("[RunOneFrame] Could not find RunOneFrame method");

            if (RunApplication != null)
                return Task.FromResult((bool)RunApplication.GetValue(game));
            
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in RunOneFrame()!");
            Console.Error.WriteLine(e);
            return Task.FromException<bool>(e);
        }
    }

    [JSExport]
    internal static Task MainLoop()
    {
        try
        {
            if (game != null)
            {
                var runMethod = game.GetType()?.GetMethod("Run", BindingFlags.Public | BindingFlags.Instance);
                if (runMethod != null)
                    runMethod.Invoke(game, null);
                else
                    Console.Error.WriteLine("[MainLoop] Could not find Run method");
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in MainLoop()!");
            Console.Error.WriteLine(e);
            return Task.FromException(e);
        }
        return Task.Delay(0);
    }
}
