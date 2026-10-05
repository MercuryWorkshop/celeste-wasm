using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.IO;
using System.Linq;

struct Dll
{
    public string RealName;
    public string MappedName;
}

public static partial class CelesteBootstrap
{
    [DllImport("Emscripten")]
    public extern static int mount_opfs();
    [DllImport("Emscripten")]
    public extern static int mount_fetch(string srcdir, string dstdir);
    [DllImport("Emscripten")]
    public extern static int mount_fetch_file(string path);

    private static void TryCreateDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private static void MountDlls(string root, string[] rawDlls)
    {
        IEnumerable<Dll> dlls = rawDlls.Select(x =>
        {
            var split = x.Split('|');
            return new Dll() { RealName = split[0], MappedName = split[1] };
        });

        // mono.cecil searches in /bin for some dlls
        Directory.CreateDirectory("/bin");
		mount_fetch(root + "_framework/", "/fetchdlls/");
        foreach (var dll in dlls)
        {
            int ret = mount_fetch_file($"/fetchdlls/{dll.RealName}");
            if (ret != 0)
            {
                throw new Exception($"Failed to mount {dll.RealName}: error code {ret}");
            }
			File.CreateSymbolicLink($"/bin/{dll.MappedName}", $"/fetchdlls/{dll.RealName}");
        }
    }

    [JSExport]
    public static async Task MountFilesystems(string root, string[] rawDlls)
    {
        try
        {
            int ret = mount_opfs();
            if (ret != 0)
            {
                throw new Exception($"Failed to mount OPFS: error code {ret}");
            }

            TryCreateDirectory("/libsdl/Celeste");
            TryCreateDirectory("/libsdl/Celeste/Mods");
            TryCreateDirectory("/libsdl/Celeste/Saves");
            TryCreateDirectory("/remote/");
            File.CreateSymbolicLink("/Content", "/libsdl/Content");
            File.CreateSymbolicLink("/Saves", "/libsdl/Celeste/Saves");
            File.CreateSymbolicLink("/remote/%GameInstall%Saves", "/libsdl/Celeste/Saves");
            MountDlls(root, rawDlls);
        }
        catch (Exception err)
        {
            Console.Error.WriteLine("Error in MountFilesystems()!");
            Console.Error.WriteLine(err);
            throw;
        }
    }
}
