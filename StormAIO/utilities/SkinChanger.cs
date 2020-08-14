using System;
using System.Windows.Forms;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI.Values;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.utilities
{
    public class SkinChanger
    {
        private readonly Menu SkinSeter;
        private AIHeroClient Player => ObjectManager.Player;

        public MenuSliderButton SkinMeun => SkinSeter.GetValue<MenuSliderButton>("setskin");

        public SkinChanger()
        {
            SkinSeter = new Menu("SkinChanger", "Set Skin");
            SkinSeter.Add(new MenuSliderButton("setskin", "set skin", 
                1)).ValueChanged += OnValueChanged;
            MainMenu.UtilitiesMenu.Add(SkinSeter);
            Game.OnNotify += delegate(GameNotifyEventArgs args)
            {
                
                if (args.EventId == GameEventId.OnReincarnate && SkinMeun.Enabled)
                    Player.SetSkin(SkinMeun.ActiveValue);
                // ^ to Reload Player skin Icon personally I like Skin icons :)
            };
            Game.OnWndProc += delegate(GameWndProcEventArgs args)
            {
                if (args.Msg == 0x0100 && args.WParam == (int) Keys.Up)
                {
                    if (SkinMeun.ActiveValue == 100) SkinMeun.SetValue(0);
                    if (SkinMeun.ActiveValue != 100) SkinMeun.SetValue( SkinMeun.ActiveValue == -1? 0 + 1: SkinMeun.ActiveValue + 1);
                    Player.SetSkin(SkinMeun.ActiveValue);
                 
                }
                if (args.Msg == 0x0100 && args.WParam == (int) Keys.Down)
                {
                    if (SkinMeun.ActiveValue == 0) return;
                    if (SkinMeun.ActiveValue != 0) SkinMeun.SetValue(SkinMeun.ActiveValue - 1);
                    Player.SetSkin(SkinMeun.ActiveValue);
                }
            };
        }

       


        private void OnValueChanged(object sender, EventArgs e)
        {
            if (SkinMeun.Enabled) Player.SetSkin(SkinMeun.ActiveValue);
        }
    }
}