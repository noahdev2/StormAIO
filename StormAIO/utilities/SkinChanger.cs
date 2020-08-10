using System;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;

namespace MightyAio.utilities
{
    public class SkinChanger
    {
        private readonly Menu SkinSeter;
        private AIHeroClient Player => ObjectManager.Player;

        public SkinChanger()
        {
            SkinSeter = new Menu("SkinChanger", "Set Skin");
            SkinSeter.Add(new MenuSliderButton("setskin", "set skin", 
                1)).ValueChanged += OnValueChanged;
            MainMenu.Main_Menu.Add(SkinSeter);
            Game.OnNotify += delegate(GameNotifyEventArgs args)
            {
                var menu = SkinSeter.GetValue<MenuSliderButton>("setskin");
                if (args.EventId == GameEventId.OnReincarnate && menu.Enabled)
                    Player.SetSkin(menu.ActiveValue);
                // ^ to Reload Player skin Icon personally I like Skin icons :)
            };
        }


        private void OnValueChanged(object sender, EventArgs e)
        {
            var menu = SkinSeter.GetValue<MenuSliderButton>("setskin");
            if (menu.Enabled) Player.SetSkin(menu.ActiveValue);
        }
    }
}