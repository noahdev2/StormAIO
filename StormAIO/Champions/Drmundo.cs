using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using StormAIO.utilities;

namespace StormAIO.Champions
{
    public class Drmundo
    {
       #region Basics

        private static Spell Q, W, E, R;
        private static Menu ChampMenu;
        private static AIHeroClient Player => ObjectManager.Player;
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
            };

            var harassMenu = new Menu("harass", "Harass")
            {
                HarassMenu.QSliderBool,
                HarassMenu.WSliderBool,
                HarassMenu.EBool
            };
            
            var killStealMenu = new Menu("killSteal", "KillSteal")
            {
                KillStealMenu.QBool,
            };

            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.QSliderBool,
                LaneClearMenu.WSliderBool,
                LaneClearMenu.EBool,
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QSliderBool,
                JungleClearMenu.WSliderBool,
                JungleClearMenu.EBool
            };

            var lastHitMenu = new Menu("lastHit", "Last Hit")
            {
                LastHitMenu.QBool,
            };
            var GapCloserMenu = new Menu("GapCloserMenu","GapCloser")
            {
                Drmundo.GapCloserMenu.QBool
            };
            var drawingMenu = new Menu("Drawing", "Drawing")
            {

                DrawingMenu.DrawQ,
                DrawingMenu.DrawW,
            };
            var StructureMenu = new Menu("StructureClear","Structure Clear")
            {
                StructureClearMenu.EBool
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
                GapCloserMenu,
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
        }

        public static class HarassMenu
        {
            public static readonly MenuSliderButton QSliderBool = new MenuSliderButton("harassQ", "Use Q | If Health >= x%", 30);
            public static readonly MenuSliderButton WSliderBool = new MenuSliderButton("harassW", "Use W | If Health >= x%", 30);
            public static readonly MenuBool EBool = new MenuBool("harassE", "Use E");
            
        }

        public static class KillStealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("killStealQ", "Use Q");
        }
        public static class GapCloserMenu
        {
            public static readonly MenuBool QBool = new MenuBool("enabled", "Use Q on gapcloser");
        }
        public static class JungleClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("jungleClearQ", "Use Q | If Health >= x%", 10);

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("jungleClearW", "Use W | If Health >= x%", 10);

            public static readonly MenuBool EBool =
                new MenuBool("jungleClearE", "Use E");
        }

        public static class LaneClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("laneClearQ", "Use Q | If Health >= x%", 50);
            

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("laneClearW", "Use W | If Health >= x%", 50);
            

            public static readonly MenuBool EBool =
                new MenuBool("laneClearE", "Use E");
            
        }
        public static class StructureClearMenu
        {

            public static readonly MenuBool EBool =
                new MenuBool("structClearE", "Use E ");
            
        }


        public static class LastHitMenu
        {
            public static readonly MenuBool QBool = new MenuBool("lastHitQ", "Use Q");
            public static readonly MenuBool EBool = new MenuBool("lastHitE", "Use E");
        }
        
        public static class DrawingMenu
        {
            public static readonly MenuBool DrawQ = new MenuBool("DrawQ", "Draw Q");
            public static readonly MenuBool DrawW = new MenuBool("DrawW", "Draw W");
        }
        

        #endregion

        #region Spells 

        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q,975);
            Q.SetSkillshot(0.25f,75f,1500,true,SkillshotType.Line,HitChance.VeryHigh);
            W = new Spell(SpellSlot.W,325);
            W.Delay = 0f;
            E = new Spell(SpellSlot.E);
            E.Delay = 0f;
            R = new Spell(SpellSlot.R);
            R.Delay = 0f;
        }


        #endregion
        #region Gamestart
      
        public Drmundo()
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
            Gapcloser.OnGapcloser += GapcloserOnOnGapcloser;
            AIBaseClient.OnProcessSpellCast += AIBaseClientOnOnProcessSpellCast;
        }

        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.Slot == SpellSlot.E)
            {
                Orbwalker.ResetAutoAttackTimer();
            }
        }

        #endregion

        #region args
        private void GapcloserOnOnGapcloser(AIHeroClient Sender, Gapcloser.GapcloserArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            var sender = Sender;
            if (sender == null || !sender.IsValid || !sender.IsEnemy)
            {
                return;
            }

            if (E.IsReady() && !sender.HasBuffOfType(BuffType.SpellShield))
            {
                if (!GapCloserMenu.QBool.Enabled)
                {
                    return;
                }
                    
                switch (args.Type)
                {
                    case Gapcloser.SpellType.Targeted:
                        if (args.StartPosition == Player.Position)
                        {
                            Q.Cast(args.StartPosition);
                        }

                        break;

                    case Gapcloser.SpellType.SkillShot:
                        if (args.EndPosition.DistanceToPlayer() <= Q.Range)
                        {
                            Q.Cast(args.EndPosition);
                        }

                        break;
                }
                Q.Cast(sender);
            }
        }
        private static void OrbwalkerOnOnAction(object sender, OrbwalkerActionArgs args)
        {
            
            if (args.Type != OrbwalkerType.AfterAttack || !E.IsReady()) return;
            if (args.Target.IsEnemy && args.Target is AIHeroClient && (Orbwalker.ActiveMode == OrbwalkerMode.Combo))
            {
                if (ComboMenu.EBool) CastE();
            }
            if (args.Target.IsEnemy && args.Target is AIHeroClient && (Orbwalker.ActiveMode == OrbwalkerMode.Harass))
            {
                if(HarassMenu.EBool.Enabled) CastE();
            }
           
            if (args.Target.IsEnemy && args.Target.IsJungle() && (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear))
            {
                if (JungleClearMenu.EBool.Enabled)
                {
                    var Jungle = GameObjects.GetJungles(Player.Position, Player.GetRealAutoAttackRange() + 25)
                        .OrderBy(x => x.DistanceToPlayer()).FirstOrDefault();
                    if (Jungle == null) return;
                    E.Cast();
                }
            }

            if (args.Target is AITurretClient && StructureClearMenu.EBool)
            {
                E.Cast();
            }
            
        }
        
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingMenu.DrawQ && Q.IsReady())
                Drawing.DrawCircle(Player.Position, Q.Range, Color.Violet);
            if (DrawingMenu.DrawW && W.IsReady())
                Drawing.DrawCircle(Player.Position, W.Range, Color.DarkCyan);

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
            if (ComboMenu.QBool.Enabled)
            {
                CastQ();
            }

            if (HarassMenu.WSliderBool.Enabled)
            {
                CastW();
            }
        }

        private static void Harass()
        {
            if (HarassMenu.QSliderBool.Enabled && Player.HealthPercent > HarassMenu.QSliderBool.ActiveValue)
            {
                CastQ();
            }

            if (HarassMenu.WSliderBool.Enabled && Player.HealthPercent > HarassMenu.WSliderBool.ActiveValue)
            {
                CastW();
            }
        }

        private static void LaneClear()
        {
            if (LaneClearMenu.QSliderBool.Enabled && Q.IsReady() && LaneClearMenu.QSliderBool.ActiveValue < Player.HealthPercent)
            {
                var minions = GameObjects.GetMinions(Player.Position, Q.Range).Where(
                        x => Q.GetDamage(x) > x.Health).OrderByDescending(x => x.MaxHealth)
                    .ThenBy(x => x.DistanceToPlayer()).FirstOrDefault();
                if (minions == null) return;
                if (Player.GetAutoAttackDamage(minions) > minions.Health &&
                    minions.DistanceToPlayer() < Player.GetRealAutoAttackRange() && Orbwalker.CanAttack()) return;
                Q.Cast(minions);
            }
            if (LaneClearMenu.WSliderBool.Enabled && W.IsReady() && LaneClearMenu.WSliderBool.ActiveValue < Player.HealthPercent)
            {
                var minions = GameObjects.GetMinions(Player.Position, W.Range).OrderByDescending(x => x.MaxHealth)
                    .ThenBy(x => x.DistanceToPlayer()).FirstOrDefault();
                if (minions == null && W.ToggleState == 2)
                {
                    W.Cast();
                    return;
                }
                if (minions != null && W.ToggleState == 1) W.Cast();
            }
            if (LaneClearMenu.EBool.Enabled && E.IsReady())
            {
                var minions = GameObjects.GetMinions(Player.Position, Player.GetRealAutoAttackRange()).OrderBy(x => x.DistanceToPlayer())
                    .FirstOrDefault();
                if (minions == null) return;
                if ( E.GetDamage(minions) > minions.Health)
                {
                    E.Cast();
                    Orbwalker.Attack(minions);

                }
            }
            
        }

        private static void JungleClear()
        {
            if (JungleClearMenu.QSliderBool.Enabled && Q.IsReady() && JungleClearMenu.QSliderBool.ActiveValue < Player.HealthPercent)
            {
                var Jungles = GameObjects.GetJungles(Player.Position, Q.Range).OrderByDescending(x => x.MaxHealth)
                    .ThenBy(x => x.DistanceToPlayer()).FirstOrDefault();
                if (Jungles == null) return;
                Q.Cast(Jungles);
            }
            if (JungleClearMenu.WSliderBool.Enabled && W.IsReady() && JungleClearMenu.WSliderBool.ActiveValue < Player.HealthPercent)
            {
                var Jungles = GameObjects.GetJungles(Player.Position, W.Range).OrderByDescending(x => x.MaxHealth)
                    .ThenBy(x => x.DistanceToPlayer()).FirstOrDefault();
                if (Jungles == null && W.ToggleState == 2)
                {
                    W.Cast();
                    return;
                }
                if (Jungles != null && W.ToggleState == 1) W.Cast();
            }
            
        }

        private static void LastHit()
        {
            if (!LastHitMenu.QBool || !Q.IsReady()) return;
            var minions = GameObjects.GetMinions(Player.Position, Q.Range).Where(
                x => Q.GetDamage(x) > x.Health).OrderByDescending(x=> x.MaxHealth).
                ThenBy(x=> x.DistanceToPlayer()).FirstOrDefault();
            if (minions == null) return;
            if (Player.GetAutoAttackDamage(minions) > minions.Health && minions.DistanceToPlayer() < Player.GetRealAutoAttackRange() && Orbwalker.CanAttack()) return;
            Q.Cast(minions);
        }

        private static void KillSteal()
        {
            var qtarget = Q.GetTarget();
            if (qtarget == null || !KillStealMenu.QBool) return;
            if (qtarget.TrueHealth() < Q.GetDamage(qtarget)) Q.Cast(qtarget);

        }
        
        #endregion

        #region Spell Stage

      
        #endregion
        #region Spell Functions

        private static void CastQ()
        {
            var target = Q.GetTarget();
            if (target == null || !Q.IsReady()) return;

            var qpre = Q.GetPrediction(target);
            if (qpre.Hitchance == HitChance.High)
            {
                Q.Cast(qpre.CastPosition);
            }
        }
       
        private static void CastW()
        {
            var target = W.GetTarget();
            if (!W.IsReady()) return;
            if (target == null && W.ToggleState == 2)
            {
                W.Cast();
                return;
            }
            if (target != null && W.ToggleState == 1)
            {
                W.Cast();
            }
        }

        private static void CastE()
        {
            var target = TargetSelector.GetTarget(Player.GetRealAutoAttackRange() + 25f);
            if (target == null) return;
            E.Cast();
        }
        
        
        #endregion

      
        #region Extra functions

        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null)                                 return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
                             Damage += (float) Player.GetAutoAttackDamage(target);
            if (Q.IsReady()) Damage += Q.GetDamage(target);
            if (W.IsReady()) Damage += W.GetDamage(target);
            if (E.IsReady()) Damage += E.GetDamage(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100) 
                Damage += (float)Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float)Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }
        #endregion
    }
}