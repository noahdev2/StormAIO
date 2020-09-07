using System;
using System.Drawing;
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
            DelayAction.Add((int) delay, BootStrap);
        }

        private static void BootStrap()
        {
            try
            {
                if (!Checker.ServerStatus() || !Checker.IsUpdatetoDate())
                    return;
                // ReSharper disable once ObjectCreationAsStatement
                new MainMenu();
                // ReSharper disable once ObjectCreationAsStatement
                new Drawbackground();
                switch (ObjectManager.Player.CharacterName)
                {
                    case "Yone":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Yone();
                        break;
                    case "Warwick":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Warwick();
                        break;
                    case "Akali":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Akali();
                        break;
                    case "Yorick":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Yorick();
                        break;
                    case "KogMaw":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Kowmaw();
                        break;
                    case "DrMundo":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Drmundo();
                        break;
                    case "Rengar":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Rengar();
                        break;
                    case "Garen":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Garen();
                        break;
                    case "Ashe":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Ashe();
                        break;
                    case "Urgot":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Urgot();
                        break;
                    case "Lucian":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Lucian();
                        break;
                    case "Chogath":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Chogath();
                        break;
                    case "Zed":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Zed();
                        break;
                    case "Maokai":
                        // ReSharper disable once ObjectCreationAsStatement
                        new Maokai();
                        break;
                    // case "Nunu":
                    //     // ReSharper disable once ObjectCreationAsStatement    
                    //     new Nunu();
                    //     break;
                    // case "Bard":
                    //     // ReSharper disable once ObjectCreationAsStatement    
                    //     new Bard();
                    //     break;
                    case "Vladimir":
                        // ReSharper disable once ObjectCreationAsStatement    
                        new Vladimir();
                        break;
                    case "Twitch":
                        // ReSharper disable once ObjectCreationAsStatement    
                        new Twitch();
                        break;
                    case "Twitch" :
                        var Twitch = new Twitch();
                        break;
                }
            }
            catch (Exception error)
            {
                Game.Print("Failed to load reload or Check ur Console");
                Console.WriteLine(@"Failed To load: " + error);
            }
            // ReSharper disable once ObjectCreationAsStatement
            new Emote();
            // ReSharper disable once ObjectCreationAsStatement
            new SkinChanger();
            // ReSharper disable once ObjectCreationAsStatement
            new AutoLeveler();
            // ReSharper disable once ObjectCreationAsStatement
            new StarterItem();
            // ReSharper disable once ObjectCreationAsStatement
            new ArrowDrawer();
            // ReSharper disable once ObjectCreationAsStatement
                new DrawText("SpellFarm", MainMenu.Key, MainMenu.SpellFarm, Color.GreenYellow,
                    Color.Red); // Box Drawer with text
            // ReSharper disable once ObjectCreationAsStatement
            new DrawText2("Skin index", SkinChanger.SkinMeun, 100); // Text Drawer
            // ReSharper disable once ObjectCreationAsStatement
            new Rundown();
        }
    }
}