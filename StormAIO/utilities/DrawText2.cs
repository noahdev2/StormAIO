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
    public class DrawText2
    {
        private static Font Font;
      
        public DrawText2(string text,MenuSliderButton Number , int Textpos = 77)
        {
           
           
            Drawing.OnDraw += delegate(EventArgs args)
            {
              
                DrawTextWithFont(Font, text,1672 ,
                    Textpos , SharpDX.Color.White);
                DrawTextWithFont(Font, Number.ActiveValue == -1 ?  (Number.ActiveValue == 0).ToString(): Number.ActiveValue.ToString(),1855 ,
                    Textpos , SharpDX.Color.White);
            };
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
        
        private static void DrawTextWithFont(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

     
    }
}