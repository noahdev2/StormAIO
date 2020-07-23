﻿using System;
 using System.Collections.Generic;
 using System.Linq;
using System.Windows.Forms;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
 using EnsoulSharp.SDK.Utility;
 using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace MightyAio.Champions
{
    internal class Lillia
    {
        #region Starter

        private static AIHeroClient Player => ObjectManager.Player;

        private static Menu Menu, alliles, Emotes;

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            Menu = new Menu("Lillia", "Lillia", true);

            // Q
            var QMenu = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use W in Combo"),
                new MenuBool("QH", "Use W in Harass"),
                new MenuSlider("QMana", "Mana for using Q in harass", 30),
                new MenuBool("QM","Q managnet")
            };
            Menu.Add(QMenu);

            // W
            var WMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuBool("WH", "Use W in Harass"),
                new MenuSlider("WMana", "Mana for using W in harass", 50),
            };
            Menu.Add(WMenu);
            // E
            var EMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("EC2", "Use E long range in Combo"),
                new MenuBool("EF", "Use E in Harass"),
                new MenuSlider("EM","Use E when Mana >",70)
            };
            Menu.Add(EMenu);
            // R
            var RMenu = new Menu("R", "R")
            {
                new MenuBool("R", "Use R in Combo"),
                new MenuSlider("RC","Use R When you hit",3,1,5)
            };
            Menu.Add(RMenu);
            // lane clear
            var laneclear = new Menu("laneclear", "Lane Clear")
            {
                new MenuKeyBind("SpellFarm","Spell Farm Key",Keys.M,KeyBindType.Toggle),
                new MenuBool("Q", "Use Q for Lane Clear"),
                new MenuSlider("QMana", "Mana for using Q in LaneClear", 40),
                new MenuBool("W", "Use W for Lane Clear",false),
                new MenuSlider("WMana", "Mana for using W in LaneClear", 80),
            };
            Menu.Add(laneclear);
            var Jungleclear = new Menu("JungleClear", "Jungle Clear")
            {
                new MenuBool("Q", "Use Q" ),
                new MenuBool("W", "Use W" ),
                new MenuBool("E", "Use E" ),
            };
            Menu.Add(Jungleclear);

            // kill steal
            var killsteal = new Menu("KillSteal", "Kill Steal")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("E", "E"),
                new MenuBool("E2", "Use E Max Range"),
                new MenuBool("W", "Use W"),
            };
            Menu.Add(killsteal);

            // Misc
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 4, 0, 55),
                new MenuBool("autolevel", "Auto Level")
            };

            // use emotes
            Emotes = new Menu("Emotes", "Emotes")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Center", "East", "West", "South", "North", "Mastery"}),
                new MenuBool("Kill", "Use on kill")
            };
            miscMenu.Add(Emotes);
            Menu.Add(miscMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawW", "Draw W"),
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("DrawSpell", "Draw Spell status"),
                new MenuBool("Drawkillabeabilities", "Draw kill able abilities")
            };
            Menu.Add(drawMenu);

            Menu.Attach();
        }

        #endregion Menu

        #region Spells

        private static Font Berlinfont;

        private static int mykills = 0 + Player.ChampionsKilled;
        private static Spell Q, W, E, E2 , R;
        private static int[] SpellLevels;
        
        #endregion Spells

        #region GameLoad

        public Lillia()
        {
            SpellLevels = new[] {1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3};
            CreateMenu();
            Q = new Spell(SpellSlot.Q, 485);
            W = new Spell(SpellSlot.W, 500);
            W.SetSkillshot(0.1f, 250, 1200f, true, SkillshotType.Circle);
            E = new Spell(SpellSlot.E, 750);
            E.SetSkillshot(0.4f,150,1000f,false,SkillshotType.Line);
            E2 = new Spell(SpellSlot.E, 20000f);
            E2.SetSkillshot(0.4f,150,1000f,true,SkillshotType.Line);
            R = new Spell(SpellSlot.R, 20000f);
            R.SetSkillshot(1f, 320f, 20000f, false, SkillshotType.Line);

            Berlinfont = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Berlin San FB Demi",
                    Height = 23,
                    Weight = FontWeight.DemiBold,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearTypeNatural
                });

            Game.OnUpdate += GameOnOnUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
            Game.OnNotify += delegate(GameNotifyEventArgs args)
            {
                if (args.EventId == GameEventId.OnReincarnate && Menu["Misc"].GetValue<MenuBool>("UseSkin"))
                    Player.SetSkin(Menu["Misc"].GetValue<MenuSlider>("setskin").Value);
            };
        }
        private static bool spellfarmkey => Menu["laneclear"].GetValue<MenuKeyBind>("SpellFarm").Active;
        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Menu["Drawing"].GetValue<MenuBool>("DrawQ");
            var drawW = Menu["Drawing"].GetValue<MenuBool>("DrawW");
            var drawE = Menu["Drawing"].GetValue<MenuBool>("DrawE");
            var drawKill = Menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities");
            var drawS = Menu["Drawing"].GetValue<MenuBool>("DrawSpell");
            var p = Player.Position;

            if (drawQ && (Q.IsReady()))
                Drawing.DrawCircle(p, Q.Range, Color.Purple);
            if (drawE && (E.IsReady() )) Drawing.DrawCircle(p, E.Range, Color.Red);
            if (drawW && (W.IsReady() )) Drawing.DrawCircle(p, W.Range, Color.DarkCyan);

            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget(2000)))
                if (enemyVisible.IsValidTarget())
                {
                    var autodmg = Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage) +
                                  Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage) *
                                  Player.Crit;
                    var aa = string.Format("AA Left:" + (int) (enemyVisible.Health / autodmg));
                    if (drawKill)
                    {
                        if (Q.GetDamage(enemyVisible) > enemyVisible.Health)
                            DrawText(Berlinfont, "Killable Skills (Q):",
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                        else if (Q.GetDamage(enemyVisible) + Q.GetDamage(enemyVisible) > enemyVisible.Health)
                            DrawText(Berlinfont, "Killable Skills (Q + outrange true dmg):",
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                        else if (Q.GetDamage(enemyVisible) + Q.GetDamage(enemyVisible) + W.GetDamage(enemyVisible,DamageStage.Empowered) > enemyVisible.Health)
                            DrawText(Berlinfont, "Killable Skills (Q + outrange true dmg + W):",
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                        else if (Q.GetDamage(enemyVisible) + Q.GetDamage(enemyVisible) + W.GetDamage(enemyVisible,DamageStage.Empowered) + E.GetDamage(enemyVisible) > enemyVisible.Health)
                            DrawText(Berlinfont, "Killable Skills (Q + outrange true dmg + W  + E):",
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                        else
                            DrawText(Berlinfont, aa, (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                    }
                }
            if (drawS)
            {
                if (spellfarmkey)
                    DrawText(Berlinfont, "Spell Farm On",
                        (int) Drawing.WorldToScreen(Player.Position)[0] - 58,
                        (int) Drawing.WorldToScreen(Player.Position)[1] + 30, SharpDX.Color.White);
                if (!spellfarmkey)
                    DrawText(Berlinfont, "Spell Farm Off",
                        (int) Drawing.WorldToScreen(Player.Position)[0] - 58,
                        (int) Drawing.WorldToScreen(Player.Position)[1] + 30, SharpDX.Color.White);
            }
        }

        #endregion GameLoad

        #region Update

        private static void GameOnOnUpdate(EventArgs args)
        {
            if (Player.ChampionsKilled > mykills && Emotes.GetValue<MenuBool>("Kill"))
            {
                mykills = Player.ChampionsKilled;
                Emote();
            }

            var getskin = Menu["Misc"].GetValue<MenuSlider>("setskin").Value;
            var skin = Menu["Misc"].GetValue<MenuBool>("UseSkin");
            if (skin && Player.SkinID != getskin) Player.SetSkin(getskin);

            if (!Player.CanCast) return;
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Harass:
                    Harass();
                    break;

                case OrbwalkerMode.Combo:
                    Combo();
                    break;

                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;

                case OrbwalkerMode.LastHit:
                   
                    break;
            }

            
       

          
            Killsteal();
            if (Menu["Misc"].GetValue<MenuBool>("autolevel")) Levelup();
        }

        #endregion Update


        #region Orbwalker mod

        private static void LaneClear()
        {
            if (!spellfarmkey ) return;
            var minions = GameObjects.GetMinions(Player.Position, Q.Range).OrderByDescending(x => x.MaxHealth).ThenBy(
                x=> x.DistanceToPlayer()).ToList();
            if (!minions.Any()) return;
            foreach (var moster in minions.Where(x=> x.IsValid))
            {
                if (Menu["laneclear"].GetValue<MenuBool>("Q") && Player.ManaPercent >= Menu["laneclear"].GetValue<MenuSlider>("QMana").Value )
                { 
                
                    if (IsInInnerRange(moster)) Orbwalker.Move(Player.Position.Extend(moster.Position, -Q.Range));
                    Q.Cast();
                     
            
                }
                if (Menu["laneclear"].GetValue<MenuBool>("W") && Player.ManaPercent >= Menu["laneclear"].GetValue<MenuSlider>("WMana").Value)
                {
                    var wiminion = GameObjects.GetMinions(Player.Position, W.Range).OrderByDescending(x=> x.MaxHealth).ToList();
                    if (!wiminion.Any()) return;
                    var aa=   W.GetCircularFarmLocation(wiminion, W.Width);
                    if (aa.MinionsHit >= 1)
                    {
                        W.Cast(aa.Position);
                    }
                }
            
            }
        }

        private static void JungleClear()
        {
            var Jgl = GameObjects.GetJungles(Player.Position, Q.Range).OrderByDescending(x => x.MaxHealth).ThenBy(
                x=> x.DistanceToPlayer()).ToList();
            if (!Jgl.Any()) return;
            foreach (var moster in Jgl.Where(x=> x.IsValid))
            {
                if (Menu["JungleClear"].GetValue<MenuBool>("Q"))
                { 
                
                     if (IsInInnerRange(moster)) Orbwalker.Move(Player.Position.Extend(moster.Position, -Q.Range));
                     Q.Cast();
                     
            
                }
                if (Menu["JungleClear"].GetValue<MenuBool>("W"))
                {
                    var wiminion = GameObjects.GetJungles(Player.Position, W.Range).OrderByDescending(x=> x.MaxHealth).ToList();
                    if (!wiminion.Any()) return;
                    var aa=   W.GetCircularFarmLocation(wiminion, W.Width);
                    if (aa.MinionsHit >= 1)
                    {
                        W.Cast(aa.Position);
                    }
                }
                if (Menu["JungleClear"].GetValue<MenuBool>("E"))
                {
                    E.Cast(moster);
                }
            }
        }

        private static void Combo()
        {
            if (Menu["Q"].GetValue<MenuBool>("QC"))castQ();
            if (Menu["W"].GetValue<MenuBool>("WC"))castW();
            if (Menu["E"].GetValue<MenuBool>("EC"))castE();
            if (Menu["E"].GetValue<MenuBool>("EC2"))castE2();
            castR();
        }


        private static void Harass()
        {
            if (Menu["Q"].GetValue<MenuBool>("QH") && Menu["Q"].GetValue<MenuBool>("QMana") >= Player.ManaPercent)castQ();
            if (Menu["W"].GetValue<MenuBool>("WH") && Menu["W"].GetValue<MenuBool>("WMana") >= Player.ManaPercent)castW();
            if (Menu["E"].GetValue<MenuBool>("EF") && Menu["E"].GetValue<MenuBool>("EM")    >= Player.ManaPercent)castE();
        }

        #endregion
        
        #region spell Functions

        private static void castQ()
        {
            if (!Q.IsReady()) return;
            var target = TargetSelector.GetTarget(Q.Range);
            if (target == null) return;
            if (IsInInnerRange(target) && Menu["Q"].GetValue<MenuBool>("QM") )
            {
                Orbwalker.Move(Player.Position.Extend(target.Position, -Q.Range + 100));
                DelayAction.Add(250,()=> Q.Cast() );
            }
            else
            {
                Q.Cast();
            }
        }

        private static void castW()
        {
            var target = TargetSelector.GetTarget(W.Range);
            if (target == null) return;
            var wpre=  W.GetPrediction(target);
            if (wpre.Hitchance >= HitChance.High)
            {
              if (wpre.CastPosition.IsWall()) return;
              W.Cast(wpre.UnitPosition);
            }
        }

        private static void castE()
        {
            if (!E.IsReady()) return;
            var target = TargetSelector.GetTarget(E.Range);
            if (target == null) return;
            var epre = E.GetPrediction(target);
            if (epre.Hitchance >= HitChance.High) E.Cast(epre.CastPosition);
        }

        private static void castE2()
        {
            if (!E.IsReady()) return;
            var target = TargetSelector.GetTarget(20000);
            if (target == null) return;
            var from = Player.Position.ToVector2();
            var to = target.Position.ToVector2();
            var direction = (from - to).Normalized();
            var distance = from.Distance(to);

            for (var d = 0; d < distance; d = d + 20)
            {
                var point = from + d * direction;
                var flags = NavMesh.GetCollisionFlags(point.ToVector3());

                if (flags.HasFlag(CollisionFlags.Building) || flags.HasFlag(CollisionFlags.Wall))
                {
                    return;
                }
            }
            var epre = E2.GetPrediction(target,true);
            if (epre.Hitchance >= HitChance.VeryHigh)
            {
                E2.Cast(epre.CastPosition);
            }
        }
        private static void castR()
        {
            if (!R.IsReady() || !Menu["R"].GetValue<MenuBool>("R") ) return;
            var target = GameObjects.EnemyHeroes.Count(x => x.HasBuff("LilliaPDoT"));
            if (target >= Menu["R"].GetValue<MenuSlider>("RC").Value)
            {
                R.Cast();
            }
        }

        #endregion
        #region Extra functions

       

       

        private static void Levelup()
        {
            var qLevel = Q.Level;
            var wLevel = W.Level;
            var eLevel = E.Level;
            var rLevel = R.Level;

            if (qLevel + wLevel + eLevel + rLevel >= ObjectManager.Player.Level || Player.Level > 18) return;

            var level = new[] {0, 0, 0, 0};
            for (var i = 0; i < ObjectManager.Player.Level; i++)
                level[SpellLevels[i] - 1] = level[SpellLevels[i] - 1] + 1;

            if (qLevel < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);

            if (wLevel < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);

            if (eLevel < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            if (rLevel < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
        }

        private static bool IsInInnerRange(AIBaseClient Target)
        {
            return Target.DistanceToPlayer() <= 225;
        }
   
        private static void Emote()
        {
            var b = Emotes.GetValue<MenuList>("selectitem").SelectedValue;
            switch (b)
            {
                case "Mastery":
                    Game.SendSummonerEmote(SummonerEmoteSlot.Mastery);
                    break;

                case "Center":
                    Game.SendSummonerEmote(SummonerEmoteSlot.Center);
                    break;

                case "South":
                    Game.SendSummonerEmote(SummonerEmoteSlot.South);
                    break;

                case "West":
                    Game.SendSummonerEmote(SummonerEmoteSlot.West);
                    break;

                case "East":
                    Game.SendSummonerEmote(SummonerEmoteSlot.East);
                    break;

                case "North":
                    Game.SendSummonerEmote(SummonerEmoteSlot.North);
                    break;
            }
        }

        private static void Killsteal()
        {
            var target = TargetSelector.GetTarget(20000);
            if (target==null) return;
            if (Q.GetDamage(target) + Q.GetDamage(target) >= target.Health)
            {
                if (!Menu["KillSteal"].GetValue<MenuBool>("Q")) return;
                castQ();
            }

            if (W.GetDamage(target,DamageStage.Empowered) >= target.Health)
            {
                if (!Menu["KillSteal"].GetValue<MenuBool>("W")) return;
                castW();
            }
            if (E.GetDamage(target) >= target.Health)
            {
                if (Menu["KillSteal"].GetValue<MenuBool>("E")) castE();
                if (Menu["KillSteal"].GetValue<MenuBool>("E2")) castE2();
            }
        }

     
        #endregion
    }
}