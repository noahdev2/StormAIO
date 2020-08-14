using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using SharpDX;

namespace StormAIO
{
    
    public class MainMenu
    {
        public static Menu Main_Menu,UtilitiesMenu,Emotes,Level;
        public MainMenu()
        {
            CreateMenu();
            CreateUtilitiesMenu();
        }
        
        #region Functions

        public static void CreateMenu()
        {
            Main_Menu = new Menu("StormAIO", "StormAIO", true);
            Main_Menu.Attach();
        }
        public static void CreateUtilitiesMenu()
        {
            UtilitiesMenu = new Menu("StormAIOUtilities", "StormAIO-Utilities", true);
            Emotes = new Menu("Emotes", "Emotes")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Center", "East", "West", "South", "North", "Mastery"}),
                new MenuBool("Kill", "Use on kill")
            };
            UtilitiesMenu.Add(Emotes);
            Level = new Menu("AutoLevel","Auto Level Up")
            {
                new MenuBool("autolevel", "Auto Level")
            };
            UtilitiesMenu.Add(Level);
            var Indicator = new Menu("Indicator","Indicator")
            {
                new MenuBool("Indicator", "Indicator"),
                new MenuColor("SetColor","SetColor",new ColorBGRA(253,197,45,232))
            };
            UtilitiesMenu.Add(Indicator);
            UtilitiesMenu.Attach();
        }
        #endregion
    }
}