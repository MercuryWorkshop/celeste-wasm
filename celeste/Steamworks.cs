using System;

namespace Steamworks
{
    class SteamAPI
    {
        public static void RunCallbacks()
        {
        }

        public static bool RestartAppIfNecessary(AppId_t app)
        {
            Console.WriteLine($"Steamworks polyfill: RestartAppIfNecessary {app}");
            return false;
        }

        public static bool Init()
        {
            Console.WriteLine("Steamworks polyfill: Init");
            return true;
        }
    }

    class SteamApps
    {
        public static string GetCurrentGameLanguage()
        {
            Console.WriteLine("Steamworks polyfill: GetCurrentGameLanguage");
            return "english";
        }
    }

    class SteamUserStats
    {
        public static bool GetAchievement(string achievement, out bool achieved)
        {
            Console.WriteLine($"Steamworks polyfill: GetAchievement {achievement}");
            achieved = false;
            return true;
        }
        public static void SetAchievement(string achievement)
        {
            Console.WriteLine($"Steamworks polyfill: SetAchievement {achievement}");
        }

        public static bool GetStat(string stat, out int val)
        {
            Console.WriteLine($"Steamworks polyfill: GetStat {stat}");
            val = 0;
            return true;
        }
        public static bool SetStat(string stat, int val)
        {
            Console.WriteLine($"Steamworks polyfill: SetStat {stat} {val}");
            return true;
        }
        public static bool GetGlobalStat(string stat, out long val)
        {
            Console.WriteLine($"Steamworks polyfill: GetGlobalStat {stat}");
            val = 0;
            return true;
        }
        public static bool StoreStats()
        {
            Console.WriteLine("Steamworks polyfill: StoreStats");
            return true;
        }

        public static bool RequestCurrentStats()
        {
            Console.WriteLine("Steamworks polyfill: RequestCurrentStats");
            return true;
        }
        public static bool RequestGlobalStats(int param)
        {
            Console.WriteLine($"Steamworks polyfill: RequestGlobalStats {param}");
            return true;
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
