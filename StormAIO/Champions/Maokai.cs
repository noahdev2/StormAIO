using System;
using System.Linq;
using System.Windows.Forms;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using EnsoulSharp.SDK.Utility;
using SharpDX;
using StormAIO.utilities;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.Champions
{
    internal class Maokai
    {
        #region Basics

        private static Spell Q, W, E, R;
        private static Menu ChampMenu;
        private static AIHeroClient Player => ObjectManager.Player;
        private static Vector3 InsecPos = Vector3.Zero;
        private static Vector3 startPos = Vector3.Zero;
        private static Vector3 endPos   = Vector3.Zero;
        #endregion

        #region Menu

        private static void CreateMenu()
        {
            ChampMenu = new Menu(Player.CharacterName, Player.CharacterName);
            
            var comboMenu = new Menu("combo", "Combo")
            {
                ComboMenu.QBool,
                ComboMenu.WBool,
                ComboMenu.EBool,
                ComboMenu.RBool
            };

            var harassMenu = new Menu("harass", "Harass")
            {
                HarassMenu.QSliderBool,
                HarassMenu.WSliderBool,
                HarassMenu.ESliderBool
            };
            
            var killStealMenu = new Menu("killSteal", "KillSteal")
            {
                KillStealMenu.QBool,
                KillStealMenu.WBool,
                KillStealMenu.EBool
            };

            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.QSliderBool,
                LaneClearMenu.WSliderBool,
                LaneClearMenu.ESliderBool,
                new Menu("customization", "Customization")
                {
                    LaneClearMenu.QCountSliderBool,
                    LaneClearMenu.WCountSliderBool,
                    LaneClearMenu.ECountSliderBool
                }
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QSliderBool,
                JungleClearMenu.WSliderBool,
                JungleClearMenu.ESliderBool
            };

            var lastHitMenu = new Menu("lastHit", "Last Hit")
            {
                LastHitMenu.QSliderBool,
                LastHitMenu.WSliderBool,
                LastHitMenu.ESliderBool
            };

            var drawingMenu = new Menu("Drawing", "Drawing")
            {

                DrawingMenu.DrawQ,
                DrawingMenu.DrawW,
                DrawingMenu.DrawE,
                DrawingMenu.DrawR
            };
            var StructureMenu = new Menu("StructureClear","Structure Clear")
            {
                StructureClearMenu.QSliderBool,
                StructureClearMenu.WSliderBool,
                StructureClearMenu.ESliderBool
            };
            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                killStealMenu,
                laneClearMenu,
                jungleClearMenu,
                StructureMenu,
                lastHitMenu,
                drawingMenu
            };

            foreach (var menu in menuList)
            {
                ChampMenu.Add(menu);
            }
            MainMenu.Main_Menu.Add(ChampMenu);
        }
        #endregion

        #region MenuHelper

        public static class ComboMenu
        {
            public static readonly MenuBool QBool = new MenuBool("comboQ", "Use Q");
            public static readonly MenuBool WBool = new MenuBool("comboW", "Use W");
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use E");
            public static readonly MenuBool RBool = new MenuBool("comboR", "Use R");
        }

        public static class HarassMenu
        {
            public static readonly MenuSliderButton QSliderBool = new MenuSliderButton("harassQ", "Use Q | If Mana >= x%", 50);
            public static readonly MenuSliderButton WSliderBool = new MenuSliderButton("harassW", "Use W | If Mana >= x%", 50);
            public static readonly MenuSliderButton ESliderBool = new MenuSliderButton("harassE", "Use E | If Mana >= x%", 50);
            
        }

        public static class KillStealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("killStealQ", "Use Q");
            public static readonly MenuBool WBool = new MenuBool("killStealW", "Use W");
            public static readonly MenuBool EBool = new MenuBool("killStealE", "Use E");
        }

        public static class JungleClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("jungleClearQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("jungleClearW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("jungleClearE", "Use E | If Mana >= x%", 50);
        }

        public static class LaneClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("laneClearQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSlider QCountSliderBool =
                new MenuSlider("laneClearQCount", "Use Q if hittable minions >= x", 3, 1, 5);

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("laneClearW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSlider WCountSliderBool =
                new MenuSlider("laneClearWCount", "Use W if hittable minions >= x", 3, 1, 5);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("laneClearE", "Use E | If Mana >= x%", 50);

            public static readonly MenuSlider ECountSliderBool =
                new MenuSlider("laneClearECount", "Use E if hittable minions >= x", 3, 1, 5);
        }
        public static class StructureClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("structClearQ", "Use Q | If Mana >= x%", 50);
            
            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("structClearW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("structClearE", "Use E | If Mana >= x%", 50);
            
        }


        public static class LastHitMenu
        {
            public static readonly MenuBool QSliderBool = new MenuBool("lastHitQ", "Use Q");
            public static readonly MenuBool WSliderBool = new MenuBool("lastHitW", "Use W");
            public static readonly MenuBool ESliderBool = new MenuBool("lastHitE", "Use E");
        }
        
        public static class DrawingMenu
        {
            public static readonly MenuBool DrawQ = new MenuBool("DrawQ", "Draw Q");
            public static readonly MenuBool DrawW = new MenuBool("DrawW", "Draw W");
            public static readonly MenuBool DrawE = new MenuBool("DrawE", "Draw E");
            public static readonly MenuBool DrawR = new MenuBool("DrawR", "Draw R");
        }
        

        #endregion

        #region Spells 

        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 525);
            E = new Spell(SpellSlot.E, 1100f);
            R = new Spell(SpellSlot.R, 3000f);

            Q.SetSkillshot(0.35f, 300f, 1800f, false,SkillshotType.Line);
            E.SetSkillshot(1.5f, 100f, 1400f,false ,SkillshotType.Circle);
            
            R = new Spell(SpellSlot.R);
            R.Delay = 0.5f;
        }


        #endregion
        #region Gamestart
      
        public Maokai()
        {
            InitSpell();
            CreateMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnAction += OrbwalkerOnOnAction;
            Drawing.OnEndScene += delegate(EventArgs args)
            {
                var t = TargetSelector.GetTarget(2000f);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
            };
            AIBaseClient.OnProcessSpellCast += AIBaseClientOnOnProcessSpellCast;
            
        }

        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.Slot == SpellSlot.W)
            {
                InsecPos =
                    GameObjects.AllyHeroes.
                        FirstOrDefault(x => x.IsValidTarget(1500) &&
                                            !x.IsMe) !=
                    null
                        ? GameObjects.AllyHeroes.
                            FirstOrDefault(x => x.IsValidTarget(1500) &&
                                                x != Player).
                            Position
                        : GameObjects.AllyTurrets.Where(x =>
                                  x.IsValid &&
                                  x.DistanceToPlayer() <= 1500
                              ).
                              OrderBy(x => x.DistanceToPlayer()).
                              FirstOrDefault() !=
                          null
                            ? GameObjects.AllyTurrets.Where(x =>
                                    x.IsValid &&
                                    x.DistanceToPlayer() <= 1500).
                                OrderBy(x => x.DistanceToPlayer()).
                                FirstOrDefault().
                                Position
                            : Vector3.Zero;
            }
        }

        #endregion

        #region args

        private static void OrbwalkerOnOnAction(object sender, OrbwalkerActionArgs args)
        {
           
        }
        
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingMenu.DrawQ && Q.IsReady())
                Drawing.DrawCircle(Player.Position, Q.Range, Color.Violet);
            if (DrawingMenu.DrawW && W.IsReady())
                Drawing.DrawCircle(Player.Position, W.Range, Color.DarkCyan);
            if (DrawingMenu.DrawE && E.IsReady())
                Drawing.DrawCircle(Player.Position, E.Range, Color.DarkCyan);
            if (DrawingMenu.DrawR && R.IsReady())
                Drawing.DrawCircle(Player.Position, R.Range, Color.Violet);

        }

        #endregion

        #region gameupdate

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Helper.Checker()) return; 
            

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                  if(MainMenu.SpellFarm.Active) LaneClear();
                    JungleClear();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }

            KillSteal();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
           
        }

        private static void Harass()
        {
           

        }

        private static void LaneClear()
        {
            
        }

        private static void JungleClear()
        {
           
        }

        private static void LastHit()
        {
          
        }

        private static void KillSteal()
        {
           

        }
        
        #endregion

        #region Spell Stage

      
        #endregion
        #region Spell Functions

        private static void CastQ()
        {
            if (!Q.IsReady())
            {
                return;
            }

            var target = Q.GetTarget();
            if (target == null)
            {
                return;
            }

            if (target.DistanceToPlayer() <= 10)
            {
                return;
            }

            if (Player.IsDashing())
            {
                return;
            }

            if (target.DistanceToPlayer() <= 200 && ComboMenu.EBool.Enabled)
            {
                var pos = Q.GetPrediction(target).CastPosition.Extend(Player.Position, -300);
                if (target.HaveImmovableBuff())
                {
                    if (E.Cast(pos))
                    {
                        Q.Cast(pos);
                    }
                }
                else
                {
                    if (Q.Cast(pos))
                    {
                        E.Cast(pos);
                    }
                }
            }
            else
            {
                Q.Cast(target);
            }
        }
       
        private static void CastW()
        {

            var target = W.GetTarget();
            if (target == null || !W.IsReady())
            {
                return;
            }


            W.CastOnUnit(target);
        }

        private static void CastE()
        {
            var target = E.GetTarget();
            if (target == null || !E.IsReady())
            {
                return;
            }


            // if (HarassMenu.EUsage.Value == 0)
            // {
            //     E.Cast(E.GetPrediction(target).CastPosition);
            // }
            //
            // if (HarassMenu.EUsage.Value != 1)
            // {
            //     return;
            // }

            if (!target.HaveImmovableBuff())
            {
                return;
            }

            E.Cast(target);
        }

        private static void CastR()
        {
            var target = TargetSelector.GetTarget(1000);
            if (target == null || !R.IsReady()) return;
            if (!target.HaveImmovableBuff() || target.HaveSpellShield()) return;
            R.Cast(target);
        }
        
        
        #endregion

        #region damage 
        // Use it if some some damages aren't available by the sdk 
        private static float Qdmg(AIBaseClient t)
        {
            var damage = 0;
            return damage;
        }
        private static float Wdmg(AIBaseClient t)
        {
            var damage = 0;
            return damage;
        }
        private static float Edmg(AIBaseClient t)
        {
            var damage = 0;
            return damage;
        }
        private static float Rdmg(AIHeroClient t)
        {
            var damage = 0;
            return damage;
        }
        
        #endregion

        #region Extra functions

        public static void ExecuteInsec()
        {
            // if (!ComboMenu.Insec.Enabled)
            // {
            //     return;
            // }

            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (!Q.IsReady() || Player.Mana < 130)
            {
                return;
            }

            var target = W.GetTarget();
            if (target == null)
            {
                return;
            }

            W.CastOnUnit(target);
            if (InsecPos == Vector3.Zero)
            {
                return;
            }

            var pos = target.Position.Extend(InsecPos, 300);

            startPos = target.Position;
            endPos = pos;

            if (target.DistanceToPlayer() <= 200)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, target.Position.Extend(InsecPos, -300));
                Orbwalker.Move(target.Position.Extend(InsecPos, -300));
                if (!target.IsDashing())
                {
                    E.Cast(pos);
                    DelayAction.Add(500, () => RunInsec(InsecPos));

                }
            }
        }

        private static void RunInsec(Vector3 insecpos)
        {
            Q.Cast(InsecPos);
            InsecPos = Vector3.Zero;
            startPos = Vector3.Zero;
            endPos = Vector3.Zero;
        }
        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null)                                 return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
                             Damage += (float) Player.GetAutoAttackDamage(target);
            if (Q.IsReady()) Damage += Qdmg(target);
            if (W.IsReady()) Damage += Wdmg(target);
            if (E.IsReady()) Damage += Edmg(target);
            if (R.IsReady()) Damage += Rdmg(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100) 
                Damage += (float)Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float)Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }
        #endregion
    }
}