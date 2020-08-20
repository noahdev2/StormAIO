using System;
using System.Linq;
using System.Windows.Forms;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using StormAIO.utilities;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.Champions
{
    internal class Lucian
    {
        #region Basics

        private static Spell Q, Q2, W, W2, E, R;
        private static Menu ChampMenu;
        private static bool havePassive;
        private static int lastCastTime;
        private static AIHeroClient Player => ObjectManager.Player;
        #endregion

        #region Menu

        private static void CreateMenu()
        {
            ChampMenu = new Menu(Player.CharacterName, Player.CharacterName);
            
            var comboMenu = new Menu("combo", "Combo")
            {
                ComboMenu.QBool,
                ComboMenu.QBoolExtend,
                ComboMenu.WBool,
                ComboMenu.EBool,
                ComboMenu.EBoolDash,
                ComboMenu.EBoolReset,
                ComboMenu.EBoolSafe,
                ComboMenu.EBoolWall,
                ComboMenu.RBool
            };

            var harassMenu = new Menu("harass", "Harass")
            {
                HarassMenu.QSliderBool
            };
            
            var killStealMenu = new Menu("killSteal", "KillSteal")
            {
                KillStealMenu.QBool
            };

            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.QSliderBool,
                LaneClearMenu.WSliderBool,
                LaneClearMenu.ESliderBool,
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QSliderBool,
                JungleClearMenu.WSliderBool,
                JungleClearMenu.ESliderBool
            };

            var lastHitMenu = new Menu("lastHit", "Last Hit")
            {
                LastHitMenu.QSliderBool
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
            public static readonly MenuBool QBoolExtend = new MenuBool("comboQExtend", "Use Q extended", false);
            public static readonly MenuBool WBool = new MenuBool("comboW", "Use W");
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use short E");
            public static readonly MenuBool EBoolDash = new MenuBool("comboEDash", "Use E for engage");
            public static readonly MenuBool EBoolReset = new MenuBool("comboEReset", "Utilize passive with E?");
            public static readonly MenuBool EBoolSafe = new MenuBool("comboESafe", "E Safe check");
            public static readonly MenuBool EBoolWall = new MenuBool("comboEWall", "Don't dash into walls");
            public static readonly MenuBool RBool = new MenuBool("comboR", "Use R");
        }

        public static class HarassMenu
        {
            public static readonly MenuSliderButton QSliderBool = new MenuSliderButton("harassQ", "Use extended Q | If Mana >= x%", 50);
        }

        public static class KillStealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("killStealQ", "Use Q");
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

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("laneClearW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("laneClearE", "Use E | If Mana >= x%", 50);
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
            Q = new Spell(SpellSlot.Q, 500f) {Delay = 0.25f};
            
            Q2 = new Spell(SpellSlot.Q, 1000f);
            Q2.SetSkillshot(0.25f, 25f, int.MaxValue, false, true, SkillshotType.Line);
            
            W = new Spell(SpellSlot.W, 900f);
            W.SetSkillshot(0.25f, 80f, 1600f, false, true, SkillshotType.Line);
            
            W2 = new Spell(SpellSlot.W, 900f);
            W2.SetSkillshot(0.25f, 80f, 1600f, false, true, SkillshotType.Line);
            
            E = new Spell(SpellSlot.E, 425f);
            R = new Spell(SpellSlot.R);
        }


        #endregion
        #region Gamestart
      
        public Lucian()
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
            AIBaseClient.OnPlayAnimation += OnPlayAnimation;
            Spellbook.OnCastSpell += OnCastSpell;
            AIBaseClient.OnProcessSpellCast += OnProcessSpellCast;
        }

        #endregion

        #region args
        
        private static void OnPlayAnimation(AIBaseClient sender, AIBaseClientPlayAnimationEventArgs Args)
        {
            if (!sender.IsMe || Orbwalker.ActiveMode == OrbwalkerMode.None)
            {
                return;
            }

            if (Args.Animation == "Spell1" || Args.Animation == "Spell2")
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
        }

        private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs Args)
        {
            if (sender?.Owner != null && sender.Owner.IsMe)
            {
                if (Args.Slot == SpellSlot.Q || Args.Slot == SpellSlot.W || Args.Slot == SpellSlot.E)
                {
                    havePassive = true;
                    lastCastTime = Variables.GameTimeTickCount;
                }

                if (Args.Slot == SpellSlot.E && Orbwalker.ActiveMode != OrbwalkerMode.None)
                {
                    Orbwalker.ResetAutoAttackTimer();
                }
            }
        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs Args)
        {
            if (sender.IsMe)
            {
                if (Args.Slot == SpellSlot.Q || Args.Slot == SpellSlot.W || Args.Slot == SpellSlot.E)
                {
                    havePassive = true;
                    lastCastTime = Variables.GameTimeTickCount;
                }
            }
        }
        
        private static void OrbwalkerOnOnAction(object sender, OrbwalkerActionArgs Args)
        {
            if (Args.Type != OrbwalkerType.AfterAttack) return;
            

            havePassive = false;

            if (Args.Target == null || Args.Target.IsDead || !Args.Target.IsValidTarget() || Args.Target.Health <= 0) return;
            

            switch (Args.Target.Type)
            {
                case GameObjectType.AIHeroClient:
                    {
                        if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                        {
                            var target = Args.Target as AIHeroClient;

                            if (target != null && target.IsValidTarget())
                            {
                                if (ComboMenu.EBoolReset.Enabled && E.IsReady())
                                {
                                    ResetELogic(target);
                                }
                                else if (ComboMenu.QBool.Enabled && Q.IsReady() && target.IsValidTarget(Q.Range))
                                {
                                    Q.CastOnUnit(target);
                                }
                                else if (ComboMenu.WBool.Enabled && W.IsReady())
                                {
                                    var wPred = W.GetPrediction(target);

                                    if (wPred.Hitchance >= HitChance.High)
                                    {
                                        W.Cast(wPred.UnitPosition);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case GameObjectType.AIMinionClient:
                    {
                        if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
                        {
                            var mob = (AIMinionClient)Args.Target;
                            if (mob != null && mob.IsValidTarget() && mob.GetJungleType() != JungleType.Unknown)
                            {
                                if (JungleClearMenu.ESliderBool.Enabled &&
                                    JungleClearMenu.ESliderBool.ActiveValue < Player.ManaPercent &&
                                    E.IsReady())
                                {
                                    E.Cast(Player.PreviousPosition.Extend(Game.CursorPos, 130));
                                }
                                else if (JungleClearMenu.QSliderBool.Enabled &&
                                         JungleClearMenu.QSliderBool.ActiveValue < Player.ManaPercent
                                         && Q.IsReady())
                                {
                                    Q.CastOnUnit(mob);
                                }
                                else if (JungleClearMenu.WSliderBool.Enabled &&
                                         JungleClearMenu.WSliderBool.ActiveValue < Player.ManaPercent &&
                                         W.IsReady())
                                {
                                    W2.Cast(mob.PreviousPosition);
                                }
                            }
                        }
                    }
                    break;
                case GameObjectType.AITurretClient:
                case GameObjectType.HQClient:
                case GameObjectType.Barracks:
                case GameObjectType.BarracksDampenerClient:
                case GameObjectType.BuildingClient:
                    {
                        if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
                        {
                            if (Player.CountEnemyHeroesInRange(800) == 0)
                            {
                                if (LaneClearMenu.ESliderBool.Enabled &&
                                    LaneClearMenu.ESliderBool.ActiveValue < Player.ManaPercent &&
                                    E.IsReady())
                                {
                                    E.Cast(Player.PreviousPosition.Extend(Game.CursorPos, 130));
                                }
                                else if (LaneClearMenu.WSliderBool.Enabled &&
                                         LaneClearMenu.WSliderBool.ActiveValue < Player.ManaPercent &&
                                         W.IsReady())
                                {
                                    W.Cast(Game.CursorPos);
                                }
                            }
                        }
                    }
                    break;
            }
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
                    if (!MainMenu.SpellFarm.Active) return;
                    LaneClear();
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
            if (!HarassMenu.QSliderBool.Enabled ||
                !(HarassMenu.QSliderBool.ActiveValue < Player.ManaPercent) ||
                !Q.IsReady()) return;
            var target = Q2.GetTarget();

            if (target != null && target.IsValidTarget(Q2.Range))
            {
                CastQ(target, HarassMenu.QSliderBool.Enabled);
            }
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

        private static void CastQ(AIHeroClient target, bool extendQ = true)
        {
          
            if (!Q.IsReady() || target == null || target.IsDead || target.IsInvulnerable) return;
            
            if (target.IsValidTarget(Q.Range))
            {
                Q.CastOnUnit(target);
            }
            else if (target.IsValidTarget(Q2.Range) && extendQ)
            {
                var collisions =
                    GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) && (x.IsMinion() || x.GetJungleType() != JungleType.Unknown))
                        .ToList();

                if (!collisions.Any())
                {
                    return;
                }

                foreach (var minion in collisions)
                {
                    var qPred = Q2.GetPrediction(target);
                    var qPloygon = new Geometry.Rectangle(Player.PreviousPosition, Player.PreviousPosition.Extend(minion.Position, Q2.Range), Q2.Width);

                    if (qPloygon.IsInside(qPred.UnitPosition.ToVector2()) && minion.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(minion);
                        break;
                    }
                }
            }

        }
       
        private static void CastW()
        {
          
        }

        private static void CastE()
        {
           
        }

        private static void CastR()
        {
          
        }
        
        private static void DashELogic(AIBaseClient target)
        {
            if (target.DistanceToPlayer() <= Player.GetRealAutoAttackRange(target) ||
                target.DistanceToPlayer() > Player.GetRealAutoAttackRange(target) + E.Range)
            {
                return;
            }

            var dashPos = Player.PreviousPosition.Extend(Game.CursorPos, E.Range);
            if (dashPos.IsWall() && ComboMenu.EBoolWall.Enabled)
            {
                return;
            }

            if (dashPos.CountEnemyHeroesInRange(500) >= 3 && dashPos.CountAllyHeroesInRange(400) < 3 &&
                ComboMenu.EBoolSafe.Enabled)
            {
                return;
            }

            var realRange = Player.BoundingRadius + target.BoundingRadius + Player.AttackRange;
            if (Player.PreviousPosition.DistanceToCursor() > realRange * 0.60 &&
                !target.InAutoAttackRange() &&
                target.PreviousPosition.Distance(dashPos) < realRange - 45)
            {
                E.Cast(dashPos);
            }
        }
        
        private static void ResetELogic(AIBaseClient target)
        {
            var dashRange = ComboMenu.EBool.Enabled
                ? (Player.PreviousPosition.DistanceToCursor() > Player.GetRealAutoAttackRange(target) ? E.Range : 130)
                : E.Range;
            var dashPos = Player.PreviousPosition.Extend(Game.CursorPos, dashRange);

            if (dashPos.IsWall() && ComboMenu.EBoolWall.Enabled)
            {
                return;
            }

            if (dashPos.CountEnemyHeroesInRange(500) >= 3 && dashPos.CountAllyHeroesInRange(400) < 3 &&
                ComboMenu.EBoolSafe.Enabled)
            {
                return;
            }

            E.Cast(dashPos);
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