using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using System.Linq;
using DepotDownloader;
using QRCoder;

namespace Steamworks
{
    partial class SteamJS
    {
        [JSImport("GetAchievement", "SteamJS")]
        public static partial bool GetAchievement(string achievement);
        [JSImport("SetAchievement", "SteamJS")]
        public static partial void SetAchievement(string achievement);

        [JSImport("GetStat", "SteamJS")]
        public static partial int GetStat(string stat);
        [JSImport("SetStat", "SteamJS")]
        public static partial void SetStat(string stat, int value);

        [JSImport("NewQR", "SteamJS")]
        public static partial void NewQR(string dataurl);

        [JSExport]
        internal static async Task<bool> Init()
        {

            AccountSettingsStore.LoadFromFile("/libsdl/account.config");
            ContentDownloader.Config.RememberPassword = true;

            ContentDownloader.Config.CellID = 0;
            ContentDownloader.Config.UsingFileList = true;
            ContentDownloader.Config.FilesToDownload = new(["Celeste.exe"]);
            ContentDownloader.Config.FilesToDownloadRegex = [
                new Regex("Content\\/.*", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            ];

            ContentDownloader.Config.InstallDirectory = "/libsdl/";

            ContentDownloader.Config.VerifyAll = false;
            ContentDownloader.Config.MaxServers = 5;

            ContentDownloader.Config.MaxDownloads = 5;
            ContentDownloader.Config.LoginID = null;

            return true;
        }

        [JSExport]
        internal static async Task<bool> InitSteamSaved()
        {
            try
            {
                if (AccountSettingsStore.Instance.LoginTokens.Keys.Count > 0)
                {
                    string username = AccountSettingsStore.Instance.LoginTokens.Keys.First();
                    if (String.IsNullOrEmpty(username)) return false;

                    Console.WriteLine("Using saved login token for " + username);

                    if (ContentDownloader.InitializeSteam3(username, null))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return false;
        }

        [JSExport]
        internal static async Task<bool> ShutdownSteam()
        {
            try
            {
                ContentDownloader.ShutdownSteam3();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not disconnect from Steam: " + ex.Message);
                return false;
            }
        }

        [JSExport]
        internal static async Task<bool> DownloadSteamCloud()
        {
            return await ContentDownloader.steam3.DownloadSteamCloud(504230, 1000, "/remote/");
        }

        [JSExport]
        internal static async Task<bool> UploadSteamCloud()
        {
            return await ContentDownloader.steam3.UploadSteamCloud(504230, 1000, "/remote/");
        }

        [JSExport]
        internal static async Task<bool> InitSteam(string username, string password, bool qr)
        {
            try
            {
                ContentDownloader.Config.UseQrCode = qr;
                Steam3Session.qrCallback = (QRCodeData q) =>
                {
                    PngByteQRCode png = new PngByteQRCode(q);
                    byte[] bytes = png.GetGraphic(20);
                    string dataurl = "data:image/png;base64," + Convert.ToBase64String(bytes);
                    NewQR(dataurl);
                };

                if (ContentDownloader.InitializeSteam3(username, password))
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Error: InitializeSteam failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }

        [JSExport]
        internal static async Task<bool> DownloadApp()
        {
            var depotManifestIds = new List<(uint, ulong)>();
            depotManifestIds.Add((504233, 5880027853585448535));

            try
            {
                await ContentDownloader.DownloadAppAsync(504230, depotManifestIds, "public", null, null, null, false, false).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not download app: " + ex.Message);
                return false;
            }
        }

    }

    public class SteamAPI
    {
        public static void RunCallbacks()
        {
        }

        public static bool RestartAppIfNecessary(AppId_t app)
        {
            return false;
        }

        public static bool Init()
        {
            return true;
        }
    }

    public class SteamApps
    {
        public static string GetCurrentGameLanguage()
        {
            return "english";
        }
    }

    public class SteamUserStats
    {
        public static bool GetAchievement(string achievement, out bool achieved)
        {
            achieved = SteamJS.GetAchievement(achievement);
            return true;
        }
        public static bool SetAchievement(string achievement)
        {
            SteamJS.SetAchievement(achievement);
            return true;
        }

        public static bool GetStat(string stat, out int val)
        {
            val = SteamJS.GetStat(stat);
            return true;
        }
        public static bool SetStat(string stat, int val)
        {
            SteamJS.SetStat(stat, val);
            return true;
        }
        public static bool GetGlobalStat(string stat, out long val)
        {
            val = SteamJS.GetStat(stat);
            return true;
        }
        public static bool StoreStats()
        {
            return true;
        }

        public static bool RequestCurrentStats()
        {
            return true;
        }
        public static SteamAPICall_t RequestGlobalStats(int param)
        {
            return SteamAPICall_t.Invalid;
        }
    }

    [System.Serializable]
    public struct SteamAPICall_t : System.IEquatable<SteamAPICall_t>, System.IComparable<SteamAPICall_t>
    {
        public static readonly SteamAPICall_t Invalid = new SteamAPICall_t(0x0);
        public ulong m_SteamAPICall;

        public SteamAPICall_t(ulong value)
        {
            m_SteamAPICall = value;
        }

        public override string ToString()
        {
            return m_SteamAPICall.ToString();
        }

        public override bool Equals(object other)
        {
            return other is SteamAPICall_t && this == (SteamAPICall_t)other;
        }

        public override int GetHashCode()
        {
            return m_SteamAPICall.GetHashCode();
        }

        public static bool operator ==(SteamAPICall_t x, SteamAPICall_t y)
        {
            return x.m_SteamAPICall == y.m_SteamAPICall;
        }

        public static bool operator !=(SteamAPICall_t x, SteamAPICall_t y)
        {
            return !(x == y);
        }

        public static explicit operator SteamAPICall_t(ulong value)
        {
            return new SteamAPICall_t(value);
        }

        public static explicit operator ulong(SteamAPICall_t that)
        {
            return that.m_SteamAPICall;
        }

        public bool Equals(SteamAPICall_t other)
        {
            return m_SteamAPICall == other.m_SteamAPICall;
        }

        public int CompareTo(SteamAPICall_t other)
        {
            return m_SteamAPICall.CompareTo(other.m_SteamAPICall);
        }
    }

    [System.Serializable]
    public struct AppId_t : System.IEquatable<AppId_t>, System.IComparable<AppId_t>
    {
        public static readonly AppId_t Invalid = new AppId_t(0x0);
        public uint m_AppId;

        public AppId_t(uint value)
        {
            m_AppId = value;
        }

        public override string ToString()
        {
            return m_AppId.ToString();
        }

        public override bool Equals(object other)
        {
            return other is AppId_t && this == (AppId_t)other;
        }

        public override int GetHashCode()
        {
            return m_AppId.GetHashCode();
        }

        public static bool operator ==(AppId_t x, AppId_t y)
        {
            return x.m_AppId == y.m_AppId;
        }

        public static bool operator !=(AppId_t x, AppId_t y)
        {
            return !(x == y);
        }

        public static explicit operator AppId_t(uint value)
        {
            return new AppId_t(value);
        }

        public static explicit operator uint(AppId_t that)
        {
            return that.m_AppId;
        }

        public bool Equals(AppId_t other)
        {
            return m_AppId == other.m_AppId;
        }

        public int CompareTo(AppId_t other)
        {
            return m_AppId.CompareTo(other.m_AppId);
        }
    }
}
