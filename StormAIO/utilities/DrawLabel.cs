using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Windows.Forms;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI.Values;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.utilities
{
    public class DrawLabel
    {
        private static Menu Menu;
        private static Font Font;
        private static string Key => Menu.GetValue<MenuKeyBind>("Test").Key.ToString();
        public static bool SpellFarm => Menu.GetValue<MenuKeyBind>("Test").Active;
        public DrawLabel(string text,Color coloron,Color coloroff, int Textpos,int keypos)
        {
           
           
            Drawing.OnDraw += delegate(EventArgs args)
            {
                Drawing.DrawLine(2000, 120, 1650, 120, 120, Color.FromArgb(45,Color.Black));
                Drawing.DrawLine(1913 , 120, 1661 , 120, (float) (120 * 0.85), Color.FromArgb(120,Color.Black));
                Drawing.DrawLine(1819 , 84, 1907 , 84, 22, Color.FromArgb(230, SpellFarm ? Color.YellowGreen : Color.Red));
                DrawText(Font, text,1672 ,
                    77 , SharpDX.Color.White);
                DrawText(Font, Key,1855 ,
                    77 , !SpellFarm ? SharpDX.Color.White : SharpDX.Color.Black);
            };
            Menu = new Menu("L","TestMenu")
            {
                new MenuKeyBind("Test","keyBind",Keys.M,KeyBindType.Toggle)
            };
            MainMenu.UtilitiesMenu.Add(Menu);
            Font = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Comic Sans MS, Verdana",
                    Height = 16,
                    Weight = FontWeight.ExtraBold,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearTypeNatural
                });
            
        }
        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }
      
    }
}