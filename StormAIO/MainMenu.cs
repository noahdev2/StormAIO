using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using SharpDX;

namespace StormAIO
{
    
    public class MainMenu
    {
        public static Menu Main_Menu,Emotes,Level;
        public MainMenu()
        {
            CreateMenu();
        }
        
        #region Functions

        public static void CreateMenu()
        {
            Main_Menu = new Menu("StormAIO", "StormAIO", true);
            Emotes = new Menu("Emotes", "Emotes")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Center", "East", "West", "South", "North", "Mastery"}),
                new MenuBool("Kill", "Use on kill")
            };
            Main_Menu.Add(Emotes);
            Level = new Menu("AutoLevel","Auto Level Up")
            {
                new MenuBool("autolevel", "Auto Level")
            };
            Main_Menu.Add(Level);
            var Indicator = new Menu("Indicator","Indicator")
            {
                new MenuBool("Indicator", "Indicator"),
                new MenuColor("SetColor","SetColor",new ColorBGRA(253,197,45,232))
            };
            Main_Menu.Add(Indicator);
            Main_Menu.Attach();
        }
        #endregion
    }
}