using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Utility;

namespace StormAIO.utilities
{
    public class StarterItem
    {
        private static AIHeroClient Player => ObjectManager.Player;
        public StarterItem()
        {
            CreateMenu();
            DelayAction.Add(200, () => BuyItem());
        }

        private static void BuyItem()
        {
            var gold = Player.Gold;
            var time = Game.Time / 60;
            var item = MainMenu.Main_Menu.GetValue<MenuList>("selectitem").SelectedValue;
            
            if (item != "none" && item != null && Game.MapId == GameMapId.SummonersRift)
                switch (item)
                {
                    case "Dorans Blade":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 500 && !Player.HasItem(ItemId.Dorans_Blade))
                                Player.BuyItem(ItemId.Dorans_Blade);
                            if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                                Player.BuyItem(ItemId.Health_Potion);
                        }

                        break;
                    }
                }
        }

        private static void CreateMenu()
        {
            var Champ = Player.CharacterName;
            switch (Champ)
            {
                case "Yone":
                    MainMenu.Main_Menu.Add(new MenuList("selectitem", "StarterItem",
                        new[] {"Dorans Blade", "none"}));
                    break;
            }
        }
    }
}