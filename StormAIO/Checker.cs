using System;
using System.Net;
using System.Threading;
using EnsoulSharp;

namespace StormAIO
{
    public class Checker
    {
        private static string ScriptVersion = "1.8";
        public static bool ServerStatus()
        {
            var Wc = new WebClient();
            Wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
            var Isonline = Wc.DownloadString("https://raw.githubusercontent.com/noahdev2/StormAIO/master/ServerStatus.txt");
            if (!Isonline.Contains("On"))
            {
                Game.Print("Script Failed to Load Check Your Console");
                Console.WriteLine("The Script is Disabled By Owner");
                return false;
            }

            return true;
        }

        public static bool IsUpdatetoDate()
        {   var Wc = new WebClient();
            try
            {
                Wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                string OnlineV =
                    Wc.DownloadString("https://raw.githubusercontent.com/noahdev2/StormAIO/master/CurrentVersion.txt")
                        .Substring(0, 3);
                if (OnlineV != ScriptVersion)
                {
                    Game.Print("Script Failed to Load Check Your Console");
                    Console.WriteLine("The Script is outdated Please Update");
                    return false;
                }
            }
            catch
            {
                Game.Print("script failed to load Please Reload Reload Key {F5}");
                return false;
            }
            
            return true;
        }
        
    }
}