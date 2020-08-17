﻿using System;
 using System.Drawing;
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
                var drawBackground = new Drawbackground(); // background Drawer 
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
                    case "KogMaw":
                        var kowmaw = new Kowmaw();
                        break;
                    case "Ashe":
                        new Ashe();
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
            var drawLabel = new DrawText("SpellFarm", MainMenu.Key, MainMenu.SpellFarm,Color.GreenYellow, Color.Red); // Box Drawer with text
            var testLabel = new DrawText2("Skin index",SkinChanger.SkinMeun,100); // Text Drawer 
            new Rundown();
        }
        
    }
}