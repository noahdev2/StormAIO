﻿using System;
 using EnsoulSharp;
 using StormAIO.Champions;
 using StormAIO.utilities;

 namespace StormAIO
{
    internal static class Program
    {
        
        private static void Main(string[] args)
        {
         
            try
            {
                // if (!Checker.ServerStatus() || !Checker.IsUpdatetoDate()) return; // disable for now We need to add a file location to our checker.cs
                var LoadMenu      = new MainMenu();
                var LoadEmote     = new Emote();
                var LoadSkinSeter = new SkinChanger();
                var LoadAutoLevel = new AutoLeveler();
                var BuyItem       = new StarterItem();
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
                }
            }
            catch (Exception error)
            {
                Game.Print("Failed to load reload or Check ur Console");
                Console.WriteLine(@"Failed To load: " + error);
            }
        }
    }
}