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
                    case "Hunters Machete":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 500 && !Player.HasItem(ItemId.Hunters_Machete))
                                Player.BuyItem(ItemId.Hunters_Machete);
                            if (gold >= 150 && !Player.HasItem(ItemId.Refillable_Potion))
                                Player.BuyItem(ItemId.Refillable_Potion);
                        }

                        break;
                    }
                    case "Dorans Ring":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 500 && !Player.HasItem(ItemId.Dorans_Ring)) Player.BuyItem(ItemId.Dorans_Ring);
                            if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                                Player.BuyItem(ItemId.Health_Potion);
                        }

                        break;
                    }
                    case "Dorans Shield":
                    {
                        if (time < 1 && Player.InShop())
                            if (gold >= 500 && !Player.HasItem(ItemId.Long_Sword))
                                Player.BuyItem(ItemId.Long_Sword);
                        if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                            Player.BuyItem(ItemId.Health_Potion);
                        break;
                    }
                    case "Long Sword":
                    {
                        if (time < 1 && Player.InShop())
                            if (gold >= 500 && !Player.HasItem(ItemId.Dorans_Shield))
                                Player.BuyItem(ItemId.Dorans_Shield);
                        if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                            Player.BuyItem(ItemId.Health_Potion);
                        break;
                    }
                    case "Corrupting Potion":
                    {
                        if (time < 1 && Player.InShop())
                            if (gold >= 500 && !Player.HasItem(ItemId.Corrupting_Potion))
                                Player.BuyItem(ItemId.Corrupting_Potion);
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
                    MainMenu.UtilitiesMenu.Add(new MenuList("selectitem", "StarterItem",
                        new[] {"Dorans Blade", "none"}));
                    break;
                case "Warwick":
                    MainMenu.UtilitiesMenu.Add(new MenuList("selectitem", "Select Item",
                        new[] {"Hunters Machete", "none"}));
                    break;
                case "Akali":
                    MainMenu.UtilitiesMenu.Add(new MenuList("selectitem", "Select Item",
                        new[] {"Dorans Ring", "Dorans Shield", "Long Sword", "none"}));
                    break;
                case "Yorick":
                    MainMenu.UtilitiesMenu.Add(new MenuList("selectitem", "Select Item",
                        new[] {"Dorans Blade", "Corrupting Potion", "none"}));
                    break;
            }
        }
    }
}