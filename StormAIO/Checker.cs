using System;
using System.Data;
using System.Net;
using System.Net.Cache;
using EnsoulSharp;


namespace StormAIO
{
    public class Checker
    {
        private static readonly string ScriptVersion = "1.9";

        public static bool ServerStatus()
        {
            
         
            var Wc = new WebClient();
            try
            {
                Wc.CachePolicy =
                    new RequestCachePolicy(RequestCacheLevel.BypassCache);
                var Isonline =
                    Wc.DownloadString("https://raw.githubusercontent.com/noahdev2/MightyAio/master/ServerStatus.txt");
                if (!Isonline.Contains("On"))
                {
                    Game.Print("Script Failed to Load Check Your Console");
                    Console.WriteLine("The Script is Disabled By Owner");
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

        public static bool IsUpdatetoDate()
        {
            var Wc = new WebClient();
            try
            {
                Wc.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                var OnlineV =
                    Wc.DownloadString("https://raw.githubusercontent.com/noahdev2/MightyAio/master/CurrentVersion.txt")
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