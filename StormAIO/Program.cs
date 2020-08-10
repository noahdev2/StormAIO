﻿using System;
 using System.Reflection;
 using System.Threading;
 using System.Threading.Tasks;
 using EnsoulSharp;
using MightyAio.Champions;
 using MightyAio.utilities;

 namespace MightyAio
{
    internal static class Program
    {
        
        private static void Main(string[] args)
        {
         
            try
            {
                if (!Checker.ServerStatus() || !Checker.IsUpdatetoDate()) return;
                var LoadMenu      = new MainMenu();
                var LoadEmote     = new Emote();
                var LoadSkinSeter = new SkinChanger();
                var LoadAutoLevel = new AutoLeveler();
                switch (ObjectManager.Player.CharacterName)
                {
                    case "Yone":
                        var Yone = new Yone();
                        break;
                   
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(@"Failed To load: " + error);
            }
        }
    }
}