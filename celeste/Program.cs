using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("browser")]

partial class Program
{
    private static void Main()
    {
        Console.WriteLine("Hi!");
    }

    [DllImport("Emscripten")]
    public extern static int mount_opfs();
    [JSExport]
    internal static Task PreInit()
    {
        return Task.Run(() =>
        {
            try
            {
                int ret = mount_opfs();
                Console.WriteLine($"called mount_opfs: {ret}");
                if (ret != 0)
                {
                    throw new Exception("Failed to mount OPFS");
                }

                File.CreateSymbolicLink("/Content", "/libsdl/Content");
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

    [JSExport]
    internal static void Init()
    {
        try
        {
            celeste = Assembly.GetEntryAssembly();
            var Celeste = celeste.GetType("Celeste.Celeste");
            var Settings = celeste.GetType("Celeste.Settings");
            var Engine = celeste.GetType("Monocle.Engine");

            var MainThreadId = Celeste.GetField("_mainThreadId", BindingFlags.Static | BindingFlags.NonPublic);
            var AssemblyDirectory = Engine.GetField("AssemblyDirectory", BindingFlags.Static | BindingFlags.NonPublic);
            var SettingsInitialize = Settings.GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public);
            var GameConstructor = Celeste.GetConstructor([]);

            Hooks.Initialize(celeste);
            MainThreadId.SetValue(null, Thread.CurrentThread.ManagedThreadId);
            AssemblyDirectory.SetValue(null, "/");

            SettingsInitialize.Invoke(null, []);

            game = (Game)GameConstructor.Invoke([]);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in Init()!");
            Console.Error.WriteLine(e);
            throw;
        }
    }

    [JSExport]
    internal static void Cleanup()
    {
        try
        {
            celeste.GetType("Celeste.RunThread").GetMethod("WaitAll").Invoke(null, []);
            game.Dispose();
            celeste.GetType("Celeste.Audio").GetMethod("Unload").Invoke(null, []);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in Cleanup()!");
            Console.Error.WriteLine(e);
            throw;
        }
    }

    [JSExport]
    internal static bool MainLoop()
    {
        try
        {
            game.RunOneFrame();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in MainLoop()!");
            Console.Error.WriteLine(e);
            throw;
        }
        return game.RunApplication;
    }
}
