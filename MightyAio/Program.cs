﻿using System;
using EnsoulSharp;
using MightyAio.Champions;

 namespace MightyAio
{
    internal static class Program
    {
        
        private static void Main(string[] args)
        {
            try
            {
                switch (ObjectManager.Player.CharacterName)
                {
                    case "Fizz":
                        var fizz = new Fizz();
                        break;
                    case "Ezreal":
                        var ezreal = new Ezreal();
                        break;
                    case "Jinx":
                        var jinx = new Jinx();
                        break;
                    case "Senna":
                        var senna = new Senna();
                        break;  
                    case "Lucian":
                        var Lucian = new Lucian();
                        break;
                    case "Zac":
                        var zac = new Zac();
                        break;
                    case "Chogath":
                        var chogath = new Chogath();
                        break;
                    case "Udyr":
                        var Udyr = new Udyr();
                        break; 
                    case "Volibear":
                        var Volibear = new Volibear();
                        break;
                    case "Shen":
                        var Shen = new Shen();
                        break;
                    case "Riven":
                        var Riven = new Riven();
                        break;
                    case "Skarner":
                        var Skarner = new Skarner();
                        break;
                    case "Akali":
                        var Akali = new Akali();
                        break;
                    case "Yuumi":
                        var Yuumi = new Yuumi();
                        break;
                    case "Yorick":
                        var Yorick = new Yorick();
                        break;
                    case "Tryndamere":
                        var Tryndamere = new Tryndamere();
                        break;
                    case "Warwick":
                        var Warwick = new Warwick();
                        break;
                    case "Zed":
                        var Zed = new Zed();
                        break;
                    case "Lillia":
                        var Lillia = new Lillia();
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