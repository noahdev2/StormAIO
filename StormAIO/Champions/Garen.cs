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
    internal class Garen
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
                ComboMenu.EBool,
                ComboMenu.RBool
            };

            var harassMenu = new Menu("harass", "Harass")
            {
                HarassMenu.QBool,
                HarassMenu.EBool
            };
            
            var killStealMenu = new Menu("killSteal", "KillSteal")
            {
                KillStealMenu.RBool
            };

            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.EBool,
                LaneClearMenu.QBool
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QBool,
                JungleClearMenu.EBool
            };
            

            var drawingMenu = new Menu("Drawing", "Drawing")
            {
                
                DrawingMenu.DrawE,
                DrawingMenu.DrawR
            };
            var StructureMenu = new Menu("StructureClear","Structure Clear")
            {
                StructureClearMenu.QBool,
            };
            var Misc = new Menu("Misc","Misc")
            {
                MiscMenu.WBool
            };
            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                killStealMenu,
                laneClearMenu,
                jungleClearMenu,
                StructureMenu,
                Misc,
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
            public static readonly MenuSliderButton QBool = new MenuSliderButton("comboQ", "Use Q || Q Atcive Range",1000,100,1000);
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use E");
            public static readonly MenuBool RBool = new MenuBool("comboR", "Use R Only When Killable");
        }

        public static class HarassMenu
        {
            public static readonly MenuBool QBool = new MenuBool("harassQ", "Use Q",false);
            public static readonly MenuBool EBool = new MenuBool("harassE", "Use E ");
            
        }

        public static class KillStealMenu
        {
            public static readonly MenuBool RBool = new MenuBool("killStealR","Use R");
        }

        public static class JungleClearMenu
        {
            public static readonly MenuBool QBool =
                new MenuBool("jungleClearQ", "Use Q ");
            

            public static readonly MenuBool EBool =
                new MenuBool("jungleClearE", "Use E");
        }

        public static class LaneClearMenu
        {
            
            public static readonly MenuBool EBool = new MenuBool("laneClearE", "Use E");
            public static readonly MenuBool QBool = new MenuBool("laneClearQ", "Use Q");
            
        }
        public static class StructureClearMenu
        {
            public static readonly MenuBool QBool =
                new MenuBool("structClearQ", "Use Q");
            
        }


        public static class MiscMenu
        {
          public static readonly MenuBool WBool = new MenuBool("AutoW","Auto W");
        }
        
        public static class DrawingMenu
        {
            public static readonly MenuBool DrawE = new MenuBool("DrawE", "Draw E");
            public static readonly MenuBool DrawR = new MenuBool("DrawR", "Draw R");
        }
        

        #endregion

        #region Spells 

        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E,300);
            R = new Spell(SpellSlot.R,400);
            R.SetTargetted(0.45f,float.MaxValue);
        }


        #endregion
        #region Gamestart
      
        public Garen()
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
            if (sender == null || args.Target == null) return;
            if (!MiscMenu.WBool) return;
            if (sender is AITurretClient && W.IsReady() && args.Target.IsMe)
            {
                W.Cast();
                return;
            }
            var hero = sender is AIHeroClient;
            if (!hero) return;
            if (!sender.IsEnemy || !args.Target.IsMe) return;
            if (!W.IsReady()) return;
            W.Cast();
        }

        #endregion

        #region args

        private static void OrbwalkerOnOnAction(object sender, OrbwalkerActionArgs args)
        {
            if (args.Type == OrbwalkerType.BeforeAttack && Player.HasBuff("GarenE"))
            {
                args.Process = false;
            }
            if (args.Type == OrbwalkerType.AfterAttack)
            {
                if (HasE) return;
                if (Orbwalker.ActiveMode != OrbwalkerMode.Combo)
                {
                    return;
                }

                var target = TargetSelector.GetTarget(ComboMenu.QBool.ActiveValue);
                if (target == null) return;
                if (!ComboMenu.QBool.Enabled) return;
                if (!Q.IsReady()) return;
                if (!target.IsFacing(Player)) return;
                Q.Cast();
            } 
            if (args.Type == OrbwalkerType.AfterAttack)
            {
                if (Orbwalker.ActiveMode != OrbwalkerMode.Harass)
                {
                    return;
                }

                var target = TargetSelector.GetTarget(600);
                if (target == null) return;
                if (!HarassMenu.QBool) return;
                if (!Q.IsReady()) return;
                if (!target.IsFacing(Player)) return;
                Q.Cast();
            } 
            if (args.Type == OrbwalkerType.AfterAttack && args.Target is AITurretClient && StructureClearMenu.QBool)
            {
                Q.Cast();
            } 
        }
        
        private static void Drawing_OnDraw(EventArgs args)
        {
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
            }
            KillSteal();
           
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
         if (ComboMenu.QBool.Enabled) CastQ();
         if (ComboMenu.EBool) CastE();
         if (ComboMenu.RBool) CastR();
        }

        private static void Harass()
        {
           if (HarassMenu.QBool) CastQ();
           if (HarassMenu.EBool) CastE(); 

        }

        private static void LaneClear()
        {
            var minion = GameObjects.GetMinions(Player.Position, 
                E.Range).OrderBy(x=> x.DistanceToPlayer()).FirstOrDefault();
            if (minion != null && minion.IsEnemy)
            {
                if (HasE) return;
                if (E.IsReady() && LaneClearMenu.EBool) E.Cast();
            }

            var Qminion = GameObjects.GetMinions(Player.Position, 500).Where(x => x.Health <
                                                                                  Qdmg(x)).OrderBy(x => x.MaxHealth)
                .FirstOrDefault();
            if (Qminion != null && LaneClearMenu.QBool && !HasE && !E.IsReady())
            {
                Q.Cast();
                Orbwalker.Attack(Qminion);
            }
        }

        private static void JungleClear()
        {
            var jungle = GameObjects.GetJungles(Player.Position, 
                E.Range).OrderBy(x=> x.DistanceToPlayer()).FirstOrDefault();
            if (jungle != null)
            {
                if (E.IsReady() && JungleClearMenu.EBool) E.Cast();
                if (Q.IsReady() && !Helper.CanAttackAnyMonster && JungleClearMenu.QBool && !HasE) Q.Cast();
            }
        }
        

        private static void KillSteal()
        {
           if (!KillStealMenu.RBool) return;
           var target = R.GetTarget();
           if (target == null || !R.IsReady()) return;
           if (Rdmg(target) > target.TrueHealth()) R.Cast(target);

        }
        
        #endregion

        #region Spell Stage

      
        #endregion
        #region Spell Functions

        private static void CastQ()
        {
            var target = TargetSelector.GetTarget(ComboMenu.QBool.ActiveValue);
            if (target == null || !Q.IsReady() || HasE) return;
            if (!target.IsFacing(Player) && target.MoveSpeed - 100 < Player.MoveSpeed)
                Q.Cast();
        }
        

        private static void CastE()
        {
            if (HasE) return;
            var target = TargetSelector.GetTarget(E.Range);
            if (target == null || !E.IsReady() || !target.IsFacing(Player)) return;
            E.Cast();
        }

        private static void CastR()
        {
            var target = TargetSelector.GetTarget(R.Range);
            if (target == null || !R.IsReady()) return;
            if (target.TrueHealth() < Rdmg(target)) R.Cast(target);

        }
        
        
        #endregion

        #region damage 
        // Use it if some some damages aren't available by the sdk 
        private static float Rdmg(AIBaseClient T)
        {
            if (R.Level == 0) return 0;
            double damage = 150 + 150 * (R.Level - 1)  +
                            (T.MaxHealth - T.Health) * new[] { 200, 250, 300 }[R.Level - 1] / T.MaxHealth;;
            return (float) damage;
        }

        private static float Qdmg(AIBaseClient T)
        {
            if (Q.Level == 0) return 0;
            var damage = 30 + 30 * (Q.Level - 1) + Player.TotalAttackDamage * 0.5 + Player.TotalAttackDamage;
            return (float) Player.CalculatePhysicalDamage(T,damage);
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
            if (E.IsReady()) Damage += E.GetDamage(target);
            if (R.IsReady()) Damage += Rdmg(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100) 
                Damage += (float)Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float)Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }

        private static bool HasE => Player.HasBuff("GarenE");

        #endregion
    }
}