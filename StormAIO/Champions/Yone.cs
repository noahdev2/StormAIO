﻿using System;
using System.Linq;
using System.Windows.Forms;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using SharpDX;
using StormAIO.utilities;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.Champions
{
    internal class Yone
    {
        #region Basics

        private static Spell Q, Q3, W, E, R;
        private static Menu Menu;
        private static AIHeroClient Player => ObjectManager.Player;
        private static float sheenTimer;
        #endregion

        #region Menu

        private static void CreateMenu()
        {
            Menu = new Menu("Yone", "Yone");
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("Q", "Use Q in combo"),
                new MenuBool("Q3", "Use 3Q in combo"),
                new MenuBool("QA", "Auto stack Q in combo only"),
                new MenuBool("QH", "Use Q in Harass"),
                new MenuBool("QL", "Use Q To LastHit Minions in Harass"),
                new MenuBool("QH3", "Use 3Q in Harass", false),
            };
            Menu.Add(qMenu);
            var wMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "use W in Combo"),
                new MenuBool("WH", "use W in Harass"),
            };
            Menu.Add(wMenu);
            var eMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("ED", "Use E to remove debuffs", false),
                new MenuSlider("EDR", "Remove debuff when my health is below < ", 20),
                new MenuSlider("ER", "E range +", 250, 50, 500)
            };
            Menu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuBool("R", "Use in Combo R"),
                new MenuKeyBind("RT", "Simi R Key", Keys.T, KeyBindType.Press),
                new MenuSlider("RH", " R || The target health is below %", 50),
                new MenuSliderButton("RF", "Use R in Combo || when u can hit ", 3, 1, 5)
            };
            Menu.Add(rMenu);
            var feelmenu = new Menu("Flee", "Flee")
            {
                new MenuKeyBind("FK", "Flee Key", Keys.Z, KeyBindType.Press),
                new MenuBool("Q3", "Use Stacked Q"),
                new MenuBool("QS", "Stack Q while Fleeing",false),
            };
            Menu.Add(feelmenu);
            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("FarmLogic","Farm Logic"),
                new MenuBool("Q", "Use Q"),
                new MenuBool("W", "Use W",false)
            };
            Menu.Add(laneClearMenu);
            var JungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("W", "Use W")
            };

            Menu.Add(JungleClearMenu);
            var LastHit = new Menu("LastHit", "LastHit")
            {
                new MenuBool("Q", "Use Q"),
            };

            Menu.Add(LastHit);
            var killSteal = new Menu("KS", "KillSteal")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("Q3", "Use stacked Q"),
                new MenuBool("W", "Use W"),
                new MenuBool("R", "Use R", false)
            };
            Menu.Add(killSteal);
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawW", "Draw W",false),
                new MenuBool("DrawE", "Draw E", false),
                new MenuBool("DrawR", "Draw R"),
            };
            Menu.Add(drawMenu);
            MainMenu.Main_Menu.Add(Menu);

        }

        #endregion menu

        #region Menu Helper

        private static bool UseComboQ => Menu["Q"].GetValue<MenuBool>("Q");
        private static bool UseComboQ3 => Menu["Q"].GetValue<MenuBool>("Q3");
        private static bool UseHarassQ => Menu["Q"].GetValue<MenuBool>("QH");
        private static bool UseHarassQ3 => Menu["Q"].GetValue<MenuBool>("QH3");
        private static bool QLastHit => Menu["Q"].GetValue<MenuBool>("QL");
        private static bool AutoStackQ => Menu["Q"].GetValue<MenuBool>("QA");
        private static bool UseComboW => Menu["W"].GetValue<MenuBool>("WC");
        private static bool UseHarassW => Menu["W"].GetValue<MenuBool>("WH");

        private static bool UseE => Menu["E"].GetValue<MenuBool>("EC");
        private static bool RemoveDebuff => Menu["E"].GetValue<MenuBool>("ED");
        private static int RemoveDebuffHealth => Menu["E"].GetValue<MenuSlider>("EDR").Value;
        private static int ERPlus => Menu["E"].GetValue<MenuSlider>("ER").Value;

        private static bool UseR => Menu["R"].GetValue<MenuBool>("R");
        private static int RminHealth => Menu["R"].GetValue<MenuSlider>("RH").Value;
        private static int Rnumber => Menu["R"].GetValue<MenuSliderButton>("RF").ActiveValue;
        private static bool Rmulti => Menu["R"].GetValue<MenuSliderButton>("RF").Enabled;
        private static bool FeelAtive => Menu["Flee"].GetValue<MenuKeyBind>("FK").Active;
        private static bool Q3Flee => Menu["Flee"].GetValue<MenuBool>("Q3");
        private static bool FleeStack => Menu["Flee"].GetValue<MenuBool>("QS");
        private static bool LastHitQ => Menu["LastHit"].GetValue<MenuBool>("Q");
        private static bool QKS => Menu["KS"].GetValue<MenuBool>("Q");
        private static bool Q3KS => Menu["KS"].GetValue<MenuBool>("Q3");
        private static bool WKS => Menu["KS"].GetValue<MenuBool>("W");
        private static bool RKS => Menu["KS"].GetValue<MenuBool>("R");
        
        private static MenuKeyBind SimiR => Menu["R"].GetValue<MenuKeyBind>("RT");
        

        #endregion

        #region Gamestart
        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q, 450);
            Q.SetSkillshot(0.4f, 20f, 1500f, false, SkillshotType.Line);
            Q3 = new Spell(SpellSlot.Q, 950);
            Q3.SetSkillshot(0.4f, 160f, 1500f, false, SkillshotType.Line);
            W = new Spell(SpellSlot.W, 600f);
            W.SetSkillshot(0.5f, 100f, 1500f, false, SkillshotType.Cone);
            E = new Spell(SpellSlot.E, 300f);
            E.SetSkillshot(0f, 20f, 1200f, false, SkillshotType.Line);
            R = new Spell(SpellSlot.R, 1000f);
            R.SetSkillshot(0.75f, 160f, 1500f, false, SkillshotType.Line);
        }
        
        public Yone()
        {
            InitSpell();
            test1();
            CreateMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnAction += OrbwalkerOnOnAction;
            AIBaseClient.OnBuffLose += delegate(AIBaseClient sender, AIBaseClientBuffLoseEventArgs args)
            {
                if (sender.IsMe)
                    if (args.Buff.Name == "sheen" || args.Buff.Name == "TrinityForce")
                        sheenTimer = Variables.GameTimeTickCount + 1.7f;
            };
            Drawing.OnEndScene += delegate(EventArgs args)
            {
                var t = TargetSelector.GetTarget(2000f);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
            };
            new DrawText("Simi R Key",SimiR.Key.ToString(),SimiR,Color.GreenYellow,Color.Red,123,132);
        }

        #endregion

        #region args

        private static void OrbwalkerOnOnAction(object sender, OrbwalkerActionArgs args)
        {
            if (args.Type == OrbwalkerType.AfterAttack && Orbwalker.ActiveMode == OrbwalkerMode.Combo) CastQ2();
        }
        

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Menu["Drawing"].GetValue<MenuBool>("DrawQ") && Q.IsReady())
                Drawing.DrawCircle(Player.Position, Q.Range, Color.Violet);
            if (Menu["Drawing"].GetValue<MenuBool>("DrawW") && W.IsReady())
                Drawing.DrawCircle(Player.Position, W.Range, Color.DarkCyan);
            if (Menu["Drawing"].GetValue<MenuBool>("DrawE") && E.IsReady())
                Drawing.DrawCircle(Player.Position, E.Range, Color.DarkCyan);
            if (Menu["Drawing"].GetValue<MenuBool>("DrawR") && R.IsReady())
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
                 if (MainMenu.SpellFarm.Active)   LaneClear();
                    JungleClear();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
                  
            }
            
            KillSteal();
            if (Menu["R"].GetValue<MenuKeyBind>("RT").Active) CastR2();
            if (Player.HaveImmovableBuff() && _EStage == EStage.Recast && RemoveDebuff &&
                Player.Health < RemoveDebuffHealth) E.Cast();
           if (FeelAtive) Flee();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            if (UseComboQ) CastQ();
            if (UseComboQ3) CastQ3();
            if (UseComboW) CastW();
            if (UseE) CastE();
            if (UseR) CastR();
            AutoStack();
            CastRAIO();
        }

        private static void Harass()
        {
            if (UseHarassQ) CastQ2();
            if (UseHarassQ3) CastQ3();
            if (UseHarassW) CastW();
            var minons = GameObjects.GetMinions(Player.Position, Q.Range).Where(x => x.Health <= Qdmg(x))
                .OrderByDescending(x => x.MaxHealth).FirstOrDefault();
            if (minons == null || !Menu["Q"].GetValue<MenuBool>("QL") || _QStage != QStage.First || !QLastHit) return;
            Q.Cast(minons);

        }

        private static void LaneClear()
        {
            var FarmLogic = Menu["LaneClear"].GetValue<MenuBool>("FarmLogic");
            var target = TargetSelector.GetTarget(2000);
            if (FarmLogic && target != null && target.IsVisibleOnScreen)
            {
                var Q1minons = GameObjects.GetMinions(Player.Position, Q.Range);
                var W1minons = GameObjects.GetMinions(Player.Position, W.Range);
                if (Q1minons == null || W1minons== null) return;
                if (Qdmg(Q1minons.OrderBy(x=> x.Health).First()) > Q1minons.OrderBy(x=> x.Health).First().Health && _QStage == QStage.First)
                {
                    Q.Cast(Q1minons.First().Position);
                    return;
                }
              
                var QLine = Q.GetLineFarmLocation(Q1minons,Q.Width);
                var WCone = W.GetCircularFarmLocation(W1minons,W.Width);
                if (Menu["LaneClear"].GetValue<MenuBool>("Q") && _QStage == QStage.First)
                {
                    if (QLine.MinionsHit >= 1) Q.Cast(QLine.Position);
                }
                if (Menu["LaneClear"].GetValue<MenuBool>("W") && W.IsReady())
                {
                    if (WCone.MinionsHit >= 1) W.Cast(WCone.Position);
                }
                return;
            }
            var Qminons = GameObjects.GetMinions(Player.Position,_QStage == QStage.First ? Q.Range:Q3.Range);
            var Wminons = GameObjects.GetMinions(Player.Position, W.Range);
          
            if (Qminons != null || Wminons != null)
            {
                var QLine = Q.GetLineFarmLocation(Qminons, _QStage == QStage.First ? Q.Width: Q3.Width);
                var WCone = W.GetCircularFarmLocation(Wminons,W.Width);
                if (Menu["LaneClear"].GetValue<MenuBool>("Q") && Q.IsReady())
                {
                    if (QLine.MinionsHit >= 1) Q3.Cast(QLine.Position);
                }

                if (Menu["LaneClear"].GetValue<MenuBool>("W") && W.IsReady())
                {
                    if (WCone.MinionsHit >= 1) W.Cast(WCone.Position);
                }
            }
        }

        private static void JungleClear()
        {
            var Jungle = GameObjects.GetJungles(Player.Position, W.Range).OrderByDescending(x => x.MaxHealth)
                .ThenBy(x => x.DistanceToPlayer()).FirstOrDefault();
            if (Jungle == null) return;
            if (Menu["JungleClear"].GetValue<MenuBool>("Q") && Q.IsInRange(Jungle)) Q.Cast(Jungle);
            if (Menu["JungleClear"].GetValue<MenuBool>("W")) W.Cast(Jungle);
        }

        private static void LastHit()
        {
            if (!LastHitQ) return;
            var minons = GameObjects.GetMinions(Player.Position, Q.Range).Where(x => x.Health <= Qdmg(x))
                .OrderByDescending(x => x.MaxHealth).FirstOrDefault();
            if (minons == null || _QStage != QStage.First) return;
            Q.Cast(minons);
        }

        private static void KillSteal()
        {
            var Qtarget = TargetSelector.GetTarget(Q.Range);
            var Q3target = TargetSelector.GetTarget(Q3.Range);
            var Wtarget = TargetSelector.GetTarget(W.Range);
            var Rtarget = TargetSelector.GetTarget(R.Range);
            if (Qtarget != null && Qtarget.TrueHealth() < Qdmg(Qtarget) && QKS && _QStage == QStage.First)
                Q.Cast(Qtarget);
            if (Q3target != null && Q3target.TrueHealth() < Qdmg(Q3target) && Q3KS &&
                _QStage == QStage.Second) Q3.Cast(Qtarget);
            if (Wtarget != null && Wtarget.TrueHealth() < Wdmg(Wtarget) && WKS) W.Cast(Wtarget);
            if (Rtarget != null && Rtarget.TrueHealth() < Rdmg(Rtarget) && RKS)
            {
                if (!R.IsReady()) return;
                var rpre = R.GetPrediction(Rtarget);
                if (rpre.Hitchance >= HitChance.VeryHigh) R.Cast(rpre.CastPosition);
            }

        }

        private static void Flee()
        {
            Orbwalker.Move(Game.CursorPos);
            if (Q3Flee && _QStage == QStage.Second) Q.Cast(Game.CursorPos);
            if (FleeStack) AutoStack();
        }

        #endregion

        #region Spell Stage

        private enum QStage
        {
            First,
            Second,
            Cooldown
        }

        private static QStage _QStage
        {
            get
            {
                if (!Q.IsReady()) return QStage.Cooldown;

                return Q.Name == "YoneQ" ? QStage.First : QStage.Second;
            }
        }

        private enum EStage
        {
            Cast,
            Recast,
            Cooldown
        }

        private static EStage _EStage
        {
            get
            {
                if (!E.IsReady()) return EStage.Cooldown;
                var myclone = GameObjects.AllGameObjects.FirstOrDefault(x => x.IsAlly && x.Name == "TestCubeRender10Vision");
                return myclone == null ? EStage.Cast : EStage.Recast;
            }
        }

        #endregion
        #region Spell Functions

        private static void CastQ()
        {
            var target = TargetSelector.GetTarget(Q.Range);
            if (target == null || _QStage != QStage.First) return;
            var truedelay = 0.4f * (1 - Math.Min((Player.AttackSpeedMod - 1) * 0.58f, 0.67f));
            if (Q.Delay > truedelay) Q.Delay = truedelay;
            if (target.DistanceToPlayer() > Player.GetRealAutoAttackRange()) Q.Cast(target); 
        }
        private static void CastQ2()
        {
            var target = TargetSelector.GetTarget(Q.Range);
            if (target == null || _QStage != QStage.First) return;
            var truedelay = 0.4f * (1 - Math.Min((Player.AttackSpeedMod - 1) * 0.58f, 0.67f));
            if (Q.Delay > truedelay) Q.Delay = truedelay;
            Q.Cast(target); 
        }
        private static void CastQ3()
        {
            var target = TargetSelector.GetTarget(Q3.Range - 25);
            if (target == null || _QStage != QStage.Second) return;
            var truedelay = 0.4f * (1 - Math.Min((Player.AttackSpeedMod - 1) * 0.58f, 0.67f));
            if (Q3.Delay > truedelay) Q3.Delay = truedelay;
            if (!Q.IsReady()) return;
            if (!target.IsMoving) Q3.Cast(target.Position);
            var qpre = Q3.GetPrediction(target);
            if (qpre.Hitchance >= HitChance.High) Q3.Cast(qpre.CastPosition);
        }
        private static void CastW()
        {
            var target = TargetSelector.GetTarget(W.Range - 20);
            var truedelay = 0.5f * (1 - Math.Min((Player.AttackSpeedMod - 1) * 0.58f, 0.68f));
            if (W.Delay > truedelay) W.Delay = truedelay;
            if (target == null) return;
            if (!W.IsReady()) return;
            W.Cast(target);
        }

        private static void CastE()
        {
            var target = TargetSelector.GetTarget(E.Range + ERPlus);
            if (target == null || _EStage != EStage.Cast) return;
            if (!E.IsReady()) return;
            E.Cast(target.Position);
        }

        private static void CastR()
        {
            var target = TargetSelector.GetTarget(R.Range - 50);
            if (target == null || !R.IsReady() || _QStage == QStage.Second) return;
            var rpre = R.GetPrediction(target);
            if (rpre.Hitchance >= HitChance.VeryHigh && target.HealthPercent < RminHealth) R.Cast(rpre.CastPosition);
        }

        private static void CastR2()
        {
            var target = TargetSelector.GetTarget(R.Range - 50);
            if (target == null || !R.IsReady()) return;
            var rpre = R.GetPrediction(target);
            if (rpre.Hitchance >= HitChance.High) R.Cast(rpre.CastPosition);
        }

        private static void CastRAIO()
        {
            var target = TargetSelector.GetTarget(R.Range);
            if (!R.IsReady() || target == null || !Rmulti) return;
            R.Cast(target, false, false, true, Rnumber - 1 );
        }
       

        private static void AutoStack()
        {
            var t = TargetSelector.GetTarget(Q.Range);
            if (t != null || _QStage != QStage.First || !AutoStackQ) return;
            var attackable = GameObjects.GetMinions(Player.Position,Q.Range).Where(x=> x.IsEnemy).OrderBy(x=> x.DistanceToPlayer()).FirstOrDefault();
            var attackable2 = GameObjects.GetJungles(Player.Position,Q.Range).OrderBy(x=> x.DistanceToPlayer()).FirstOrDefault();
            if (attackable != null ) Q.Cast(attackable.Position);
            if (attackable2 != null ) Q.Cast(attackable2.Position);
        }
        #endregion

        #region damage
        
        private static float Autodamage(AIBaseClient t)
        {
            var total = Player.TotalAttackDamage;
            if (Player.Crit == 0) return (float) Player.CalculatePhysicalDamage(t, total) + Sheen(t);
            var critdamage = Player.HasItem(ItemId.Infinity_Edge) ? Player.TotalAttackDamage * 1.025 : Player.TotalAttackDamage * 0.8; 
            return (float) Player.CalculatePhysicalDamage(t, total + critdamage) + Sheen(t);
        }
        private static float Qdmg(AIBaseClient t)
        {
            if (Yone.Q.Level == 0) return 0;
            var Q = 20 + 25 * (Yone.Q.Level - 1);
            var total = Q + Player.TotalAttackDamage;
            if (Player.Crit == 0) return (float) Player.CalculatePhysicalDamage(t, total) + Sheen(t);
            var critdamage = Player.HasItem(ItemId.Infinity_Edge) ? Player.TotalAttackDamage * 0.8 : Player.TotalAttackDamage * 0.6; 
            return (float) Player.CalculatePhysicalDamage(t, total + critdamage) + Sheen(t);
        }
        private static float Wdmg(AIHeroClient t) => (float) Player.CalculateMixedDamage(t, 5 + 5 * (W.Level - 1) + t.MaxHealth * (0.055 + 0.005 * (W.Level -1 )),5 + 5 * (W.Level - 1) + t.MaxHealth * (0.055 + 0.005 * (W.Level -1 )));
        private static float Rdmg(AIHeroClient t)
        {
            var a = Player.CalculatePhysicalDamage(t, 100 + 100 * (R.Level - 1) + Player.TotalAttackDamage * 0.4);
            var b = Player.CalculateMagicDamage(t, 100 + 100 * (R.Level - 1) + Player.TotalAttackDamage * 0.4);
            return (float) (a + b);
        }
            

        private static float Sheen(AIBaseClient target)
        {
            float damage = 0;

            if (Player.HasItem(ItemId.Sheen) && sheenTimer < Variables.GameTimeTickCount)
            {
                var item = new Items.Item(ItemId.Sheen, 600);
                if (item.IsReady && !Player.HasBuff("sheen"))
                    damage = (float) Player.CalculateDamage(target,
                        DamageType.Physical,
                        Player.BaseAttackDamage);
            }

            if (Player.HasItem(ItemId.Trinity_Force) && sheenTimer < Variables.GameTimeTickCount)
            {
                var item = new Items.Item(ItemId.Trinity_Force, 600);
                if (item.IsReady && !Player.HasBuff("TrinityForce"))
                    damage = (float) (Player.CalculateDamage(target,
                                          DamageType.Physical,
                                          Player.BaseAttackDamage) *
                                      2f);
            }

            return damage;
        }
        #endregion

        #region Extra functions

        private static void test1()
        {
           
            
        }
        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null)                                 return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
                             Damage += Autodamage(target) * Helper.CurrentAttackSpeed(0.625);
            if (Q.IsReady()) Damage += Qdmg(target);
            if (W.IsReady()) Damage += Wdmg(target);
            if (R.IsReady()) Damage += Rdmg(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100) 
                Damage += (float)Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float)Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }
        #endregion
    }
}