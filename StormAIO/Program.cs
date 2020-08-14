﻿using System;
 using System.Threading;
 using System.Threading.Tasks;
 using EnsoulSharp;
 using EnsoulSharp.SDK;
 using EnsoulSharp.SDK.Utility;
 using StormAIO.Champions;
 using StormAIO.utilities;

 namespace StormAIO
{
    internal static class Program
    {
        
        private static void Main(string[] args)
        {
         GameEvent.OnGameLoad += GameEventOnOnGameLoad;
        }

        private static void GameEventOnOnGameLoad()
        {
           var delay = Game.Time > 7 ? 0f : 1750f;
           DelayAction.Add((int)delay, () => BootStrap());
        }

        private static void BootStrap()
        {
            try
            {
              //  if (!Checker.ServerStatus() || !Checker.IsUpdatetoDate()) return; // disable for now We need to add a file location to our checker.cs
                var LoadMenu      = new MainMenu();
                
                switch (ObjectManager.Player.CharacterName)
                {
                    case "Yone":
                        var Yone = new Yone();
                        break;
                    case "Warwick":
                        var Warwick = new Warwick();
                        break;
                    case "Akali":
                        var Akali = new Akali();
                        break;
                    case "Yorick":
                        var Yorick = new Yorick();
                        break;
                }
            }
            catch (Exception error)
            {
                Game.Print("Failed to load reload or Check ur Console");
                Console.WriteLine(@"Failed To load: " + error);
            }
            var LoadEmote     = new Emote();
            var LoadSkinSeter = new SkinChanger();
            var LoadAutoLevel = new AutoLeveler();
            var BuyItem       = new StarterItem();
            var farmHelper    = new ArrowDrawer();
        }
        
    }
}