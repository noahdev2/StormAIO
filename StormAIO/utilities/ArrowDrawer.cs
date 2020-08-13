using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Utility;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace StormAIO.utilities
{
    public class ArrowDrawer
    {
        private static AIHeroClient Player => ObjectManager.Player;
    
    

        private static Menu ArrowMenu;
        private static int SP1 => ArrowMenu["Heros"].GetValue<MenuSlider>("startp1").Value;
        private static int SP2 => ArrowMenu["Heros"].GetValue<MenuSlider>("startp2").Value;
        private static int EP1 => ArrowMenu["Heros"].GetValue<MenuSlider>("EndPoint1").Value;
        private static int EP2 => ArrowMenu["Heros"].GetValue<MenuSlider>("EndPoint2").Value;
        #region MenuHelper

        private static bool DrawOnPlayer => ArrowMenu["Heros"].GetValue<MenuBool>("DrawPlayer");
        private static MenuColor PlayerColor => ArrowMenu["Heros"].GetValue<MenuColor>("PlayerColor");
        private static bool DrawOnAlly   => ArrowMenu["Heros"].GetValue<MenuBool>("DrawAlly");
        private static MenuColor AllyColor => ArrowMenu["Heros"].GetValue<MenuColor>("AllyColor");
        private static bool DrawOnEnemy  => ArrowMenu["Heros"].GetValue<MenuBool>("DrawEnemy");
        private static MenuColor EnemyColor => ArrowMenu["Heros"].GetValue<MenuColor>("EnemyColor");
        private static bool DrawOnTarget => ArrowMenu["Heros"].GetValue<MenuBool>("DrawTarget");
        private static MenuColor TargetColor => ArrowMenu["Heros"].GetValue<MenuColor>("TargetColor");
        private static int  HeroLine     => ArrowMenu["Heros"].GetValue<MenuSlider>("thinkness").Value;
        
        

        #endregion
        public ArrowDrawer()
        {
            ArrowMenu = new Menu("ArrowHelper","ArrowHelper")
            {
                new Menu("Heros","Heros")
                {
                    new MenuBool("DrawPlayer","Draw On Me"),
                    new MenuColor("PlayerColor","My Arrow Color",new ColorBGRA(255,255,255,198)),
                    new MenuBool("DrawAlly","Draw On Ally"),
                    new MenuColor("AllyColor","Ally Arrow Color",new ColorBGRA(255,255,255,198)),
                    new MenuBool("DrawEnemy","Draw On Enemy"),
                    new MenuColor("EnemyColor","Enemy Arrow Color",new ColorBGRA(255,255,255,198)),
                    new MenuBool("DrawTarget","Draw On Selected Target"),
                    new MenuColor("TargetColor","Target Arrow Color",new ColorBGRA(255,255,255,198)),
                    new MenuSlider("thinkness","Line thinkness",13,1,30),
                    new MenuSlider("startp1","HeaderRotate",-35,-200,200),
                    new MenuSlider("startp2","HeaderHeight",-94,-200,200),
                    new MenuSlider("EndPoint1","BottomRotate",3,-200,200),
                    new MenuSlider("EndPoint2","BottomHeight",-54,-200,200),
                },
                new Menu("minions","minions")
                {
                    new MenuBool("DrawMinion","Draw On Minion"),
                    new MenuColor("MinionColor","My Arrow Color",new ColorBGRA(253,197,45,232)),
                    new MenuSlider("thinkness","Line thinkness",15,1,30),
                    new MenuSlider("startp1","HeaderRotate",0,-200,200),
                    new MenuSlider("startp2","HeaderHeight",0,-200,200),
                    new MenuSlider("EndPoint1","BottomRotate",0,-200,200),
                    new MenuSlider("EndPoint2","BottomHeight",0,-200,200),
                }
            };
        

            MainMenu.Main_Menu.Add(ArrowMenu);
            
            Drawing.OnDraw += DrawingOnOnEndScene;
        
        }

        private void DrawingOnOnEndScene(EventArgs args)
        {
           
           
          if (DrawOnPlayer) DrawPlayerArrow();
          if (DrawOnEnemy) DrawEnemyArrow();


        }

        private static void DrawPlayerArrow()
        {
            if (Player.IsDead) return;
            var hpBar = Player.HPBarPosition;
            var startPoint = new Vector2(hpBar.X + SP1  , hpBar.Y + SP2);
            var endPoint = new Vector2(hpBar.X + EP1, hpBar.Y + EP2);
            var startPoint12 = new Vector2(hpBar.X - SP1  , hpBar.Y + SP2);
            var endPoint2 = new Vector2(hpBar.X - EP1, hpBar.Y + EP2);
            Drawing.DrawLine(startPoint, endPoint, HeroLine, Color.FromArgb(PlayerColor.ColorA,
                PlayerColor.ColorR, PlayerColor.ColorG, PlayerColor.ColorB));
            Drawing.DrawLine(startPoint12,endPoint2,HeroLine,Color.FromArgb(PlayerColor.ColorA,
                PlayerColor.ColorR, PlayerColor.ColorG, PlayerColor.ColorB));
        }
        private static void DrawSelectedtargetArrow()
        {
            var target = TargetSelector.SelectedTarget;
            if (target == null || !target.IsVisibleOnScreen) return;
            var hpBar = target.HPBarPosition;
            var startPoint = new Vector2(hpBar.X + SP1  , hpBar.Y + SP2);
            var endPoint = new Vector2(hpBar.X + EP1, hpBar.Y + EP2);
            var startPoint12 = new Vector2(hpBar.X - SP1  , hpBar.Y + SP2);
            var endPoint2 = new Vector2(hpBar.X - EP1, hpBar.Y + EP2);
            Drawing.DrawLine(startPoint,endPoint,HeroLine,Color.White);
            Drawing.DrawLine(startPoint12,endPoint2,HeroLine,Color.White);
        }
        private static void DrawAllyArrow()
        {
            var hpBars = GameObjects.AllyHeroes.Where(x=> !x.IsMe && !x.IsDead && x.IsVisibleOnScreen && x.DistanceToPlayer() < 2000).ToList();
            foreach (var hpBar in hpBars)
            {
               
                var startPoint = new Vector2(hpBar.HPBarPosition.X + SP1  , hpBar.HPBarPosition.Y + SP2);
                var endPoint = new Vector2(hpBar.HPBarPosition.X + EP1, hpBar.HPBarPosition.Y + EP2);
                var startPoint12 = new Vector2(hpBar.HPBarPosition.X - SP1  , hpBar.HPBarPosition.Y + SP2);
                var endPoint2 = new Vector2(hpBar.HPBarPosition.X - EP1, hpBar.HPBarPosition.Y + EP2);
                Drawing.DrawLine(startPoint,endPoint,HeroLine,Color.FromArgb(EnemyColor.ColorA,
                    EnemyColor.ColorR, EnemyColor.ColorG, EnemyColor.ColorB));
                Drawing.DrawLine(startPoint12,endPoint2,HeroLine,Color.FromArgb(EnemyColor.ColorA,
                    EnemyColor.ColorR, EnemyColor.ColorG, EnemyColor.ColorB));
            }

        }
        private static void DrawEnemyArrow()
        {
            var hpBars = GameObjects.EnemyHeroes.Where(x=>   !x.IsDead && x.IsVisibleOnScreen && x.DistanceToPlayer() < 2000).ToList();
            foreach (var hpBar in hpBars)
            {
               
                var startPoint = new Vector2(hpBar.HPBarPosition.X + SP1  , hpBar.HPBarPosition.Y + SP2);
                var endPoint = new Vector2(hpBar.HPBarPosition.X + EP1, hpBar.HPBarPosition.Y + EP2);
                var startPoint12 = new Vector2(hpBar.HPBarPosition.X - SP1  , hpBar.HPBarPosition.Y + SP2);
                var endPoint2 = new Vector2(hpBar.HPBarPosition.X - EP1, hpBar.HPBarPosition.Y + EP2);
                Drawing.DrawLine(startPoint,endPoint,HeroLine,Color.FromArgb(EnemyColor.ColorA,
                    EnemyColor.ColorR, EnemyColor.ColorG, EnemyColor.ColorB));
                Drawing.DrawLine(startPoint12,endPoint2,HeroLine,Color.FromArgb(EnemyColor.ColorA,
                    EnemyColor.ColorR, EnemyColor.ColorG, EnemyColor.ColorB));
            }

        }
        private static void DrawMinionArrow()
        {
            var minioms = GameObjects.GetMinions(Player.Position, 2000).Where(x=> Player.GetAutoAttackDamage(x) >= x.Health).OrderBy(x=> x.DistanceToPlayer()).FirstOrDefault();
            if (minioms != null)
            {

                if (minioms.IsVisibleOnScreen)
                {
                    var hpBar1 = minioms.HPBarPosition;

    
                    var mstartPoint = new Vector2(hpBar1.X + SP1, hpBar1.Y + SP2);
                    var mendPoint = new Vector2(hpBar1.X + EP1, hpBar1.Y + EP2);
                    var mstartPoint12 = new Vector2(hpBar1.X - SP1, hpBar1.Y + SP2);
                    var mendPoint2 = new Vector2(hpBar1.X - EP1, hpBar1.Y + EP2);
                    Drawing.DrawLine(mstartPoint, mendPoint, 15, Color.White);
                    Drawing.DrawLine(mstartPoint12, mendPoint2, 15, Color.White);
                }
            }
        }

       
    }
}