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
    internal class Senna
    {
        #region Starter

        private static AIHeroClient Player => ObjectManager.Player;

        private static Menu Menu, alliles, baseutl, Emotes;
        private static double recallFinishTime;
        private static double recallstart;
        private static string sendername;
        private static bool Chatsent;
        private static bool SpellFarm => Menu["laneclear"].GetValue<MenuKeyBind>("Key").Active;

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            Menu = new Menu("MightySenna", "Mighty Senna", true);

            // Q
            var QMenu = new Menu("Q", "Q")
            {
                new MenuSlider("HealSet", "Auto Heal When Ally is below", 50),
                new MenuBool("QHeal", "Auto Heal"),
                new MenuSlider("QHealM", "Mana for Q heal", 50),
                new MenuKeyBind("QSimi", "Heal simikey", Keys.A, KeyBindType.Press),
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass"),
                new MenuSlider("QMana", "Mana for Q in harass", 40),
                new MenuBool("QE", "Use Extended Q ")
            };
            var ally = from hero in ObjectManager.Get<AIHeroClient>()
                where hero.IsAlly
                select hero;
            alliles = new Menu("alliles", "Use Q on");
            foreach (var hero in ally.Where(x => x.CharacterName != "Senna"))
                alliles.Add(new MenuBool(hero.CharacterName, "Use Q on " + hero.CharacterName));
            QMenu.Add(alliles);
            Menu.Add(QMenu);

            // W
            var WMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuBool("WH", "Use W in Harass"),
                new MenuSlider("WMana", "Mana for W in harass", 40),
                new MenuBool("WGap", "Use W in gapclose "),
                new MenuBool("WI", "Use W in Interrupter", false)
            };
            Menu.Add(WMenu);
            // E
            var EMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("EF", "Use E in Feel"),
                new MenuKeyBind("FeelKey", "Feel Key", Keys.Z, KeyBindType.Press)
            };
            Menu.Add(EMenu);
            // R
            var RMenu = new Menu("R", "R")
            {
                new MenuBool("RC", "Use R in Combo Only When Target is out of range and killable"),
                new MenuKeyBind("RS", "Rsimikey", Keys.H, KeyBindType.Press)
            };
            // BaseUlt
            baseutl = new Menu("BaseUlt", "Base Ult")
            {
                new MenuBool("BaseUlt", "Base Ult"),
                new MenuBool("BaseBm", "Base Bm"),
                new MenuBool("BasePing", "game ping")
            };
            RMenu.Add(baseutl);
            Menu.Add(RMenu);
            // lane clear
            var laneclear = new Menu("laneclear", "Lane Clear")
            {
                new MenuKeyBind("Key", "Spell Farm Key", Keys.M, KeyBindType.Toggle),
                new MenuBool("Q", "Use Q for Lane Clear", false),
                new MenuSlider("QMana", "Mana for Q in LaneClear", 40),
                new MenuSlider("Qcount", "Only use Q if it can hit ", 2, 0, 5),
                new MenuSeparator("Soul", "X Simi collecting Souls key")
            };
            Menu.Add(laneclear);
            // kill steal
            var killsteal = new Menu("KillSteal", "Kill Steal")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("WQ", "UseWardQ"),
                new MenuBool("D", "Use Ignite"),
                new MenuBool("W", "Use W"),
                new MenuBool("R", "Use R", false)
            };
            Menu.Add(killsteal);

            // Misc
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("SupportMode", "SupportMode"),
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 10, 0, 55),
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
                new MenuBool("DrawQ2", "Draw Q max range"),
                new MenuBool("DrawSpell", "Draw Farm Spell Status"),
                new MenuBool("PermaShow", "Perma Show"),
                new MenuBool("Drawkillabeabilities", "Draw kill abe abilities")
            };
            Menu.Add(drawMenu);

            Menu.Attach();
        }

        #endregion Menu

        #region Spells

        private static Font Berlinfont;

        private static int mykills = 0 + Player.ChampionsKilled;
        private static Spell Q, Q2, W, E, R;
        private static int[] SpellLevels;

        #endregion Spells

        #region GameLoad

        public Senna()
        {
            SpellLevels = new[] {1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3};
            CreateMenu();
            Q = new Spell(SpellSlot.Q, Player.GetRealAutoAttackRange());
            Q.SetTargetted(0.3f, 1200);
            Q2 = new Spell(SpellSlot.Q,
                Math.Min(Player.GetRealAutoAttackRange() + Math.Abs(Player.GetRealAutoAttackRange() - 1300), 1300));
            Q2.SetSkillshot(0.3f, 100f, 1200f, false, SkillshotType.Line);
            W = new Spell(SpellSlot.W, 1175f);
            W.SetSkillshot(0.25f, 60f, 1200f, true, SkillshotType.Line);
            E = new Spell(SpellSlot.E, 400f) {Delay = 1f};
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
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Drawing.OnDraw += DrawingOnOnDraw;
            Teleport.OnTeleport += OnTeleportEvent;
            Interrupter.OnInterrupterSpell += OnInterruptible;
            Orbwalker.OnAction += OrbwalkerOnOnAction;
            Game.OnNotify += delegate(GameNotifyEventArgs args)
            {
                if (args.EventId == GameEventId.OnReincarnate && Menu["Misc"].GetValue<MenuBool>("UseSkin"))
                    Player.SetSkin(Menu["Misc"].GetValue<MenuSlider>("setskin").Value);
            };
        }

        private void OrbwalkerOnOnAction(object sender, OrbwalkerActionArgs args)
        {
            if (args.Type == OrbwalkerType.BeforeAttack)
            {
                if (!Menu["Misc"].GetValue<MenuBool>("SupportMode")) return;
                if (Orbwalker.ActiveMode.Equals(OrbwalkerMode.LastHit) ||
                    Orbwalker.ActiveMode.Equals(OrbwalkerMode.LaneClear) ||
                    Orbwalker.ActiveMode.Equals(OrbwalkerMode.Harass))
                {
                    var aa = GameObjects.AttackableUnits.Where(x => x.Name == "Barrel")
                        .OrderBy(x => x.DistanceToPlayer())
                        .ToList();
                    if (args.Target.Type == GameObjectType.AIMinionClient &&
                        args.Target.Type != GameObjectType.NeutralMinionCampClient &&
                        GameObjects.AllyHeroes.Any(x => x.Distance(Player) < 1000 && !x.IsMe) && !aa.Any())
                        args.Process = false;
                }
            }
        }

        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Menu["Drawing"].GetValue<MenuBool>("DrawQ");
            var drawQM = Menu["Drawing"].GetValue<MenuBool>("DrawQ2");
            var drawW = Menu["Drawing"].GetValue<MenuBool>("DrawW");
            var drawE = Menu["Drawing"].GetValue<MenuBool>("DrawE");
            var drawKill = Menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities");
            var drawS = Menu["Drawing"].GetValue<MenuBool>("DrawSpell");
            var PermaShow = Menu["Drawing"].GetValue<MenuBool>("PermaShow");
            var p = Player.Position;

            if (drawQ && (Q.IsReady() || PermaShow))
                Drawing.DrawCircle(p, Player.GetRealAutoAttackRange(), Color.Purple);
            if (drawQM && (Q.IsReady() || PermaShow)) Drawing.DrawCircle(p, Q2.Range, Color.OrangeRed);
            if (drawE && (E.IsReady() || PermaShow)) Drawing.DrawCircle(p, E.Range, Color.Red);
            if (drawW && (W.IsReady() || PermaShow)) Drawing.DrawCircle(p, Q2.Range, Color.DarkCyan);

            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget(2000)))
                if (enemyVisible.IsValidTarget())
                {
                    var autodmg = Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage) +
                                  Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage) *
                                  Player.Crit;
                    var aa = string.Format("AA Left:" + (int) (enemyVisible.Health / autodmg));
                    if (!drawKill) continue;
                    DrawText(Berlinfont, Qdamage(enemyVisible) > enemyVisible.Health ? "Killable Skills (Q):" : aa,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                }

            if (drawS)
            {
                if (SpellFarm)
                    DrawText(Berlinfont, "Spell Farm On",
                        (int) Drawing.WorldToScreen(Player.Position)[0] - 58,
                        (int) Drawing.WorldToScreen(Player.Position)[1] + 30, SharpDX.Color.White);
                if (!SpellFarm)
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
                    break;

                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }

            if (Menu["Q"].GetValue<MenuKeyBind>("QSimi").Active) QHeal1();
            if (Menu["Q"].GetValue<MenuBool>("QHeal")) QHeal();
            if (Menu["E"].GetValue<MenuKeyBind>("FeelKey").Active) Feel();
            if (Menu["R"].GetValue<MenuKeyBind>("RS").Active && R.IsReady()) Rsimi();
            Killsteal();
            Baseutl();
            if (Menu["Misc"].GetValue<MenuBool>("autolevel")) Levelup();
        }

        #endregion Update


        #region Orbwalker mod

        private static void LastHit()
        {
            AutoCollect();
        }

        private static void Feel()
        {
            var UseEtofeel = Menu["E"].GetValue<MenuBool>("EF");
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (E.IsReady() && UseEtofeel) E.Cast();
        }

        private static void LaneClear()
        {
            AutoCollect();
            var allMinions = GameObjects.GetMinions(ObjectManager.Player.Position, Q.Range)
                .OrderBy(x => x.DistanceToPlayer()).Where(x => x.DistanceToPlayer() < Player.AttackRange);
            var allJgl = GameObjects.GetJungles(ObjectManager.Player.Position, Q.Range)
                .OrderBy(x => x.DistanceToPlayer()).Where(x => x.DistanceToPlayer() < Player.AttackRange);
            var useq = Menu["laneclear"].GetValue<MenuBool>("Q");
            var count = Menu["laneclear"].GetValue<MenuSlider>("Qcount").Value;

            if (Player.ManaPercent < Menu["laneclear"].GetValue<MenuSlider>("QMana").Value || !SpellFarm) return;

            foreach (var minion in allMinions)
            {
                var laneE = GameObjects.GetMinions(ObjectManager.Player.Position, Q.Range + Q2.Width);
                var Efarmpos = Q.GetLineFarmLocation(laneE, Q2.Width);

                if (minion.IsValidTarget(Q.Range) && Q.IsReady() &&
                    Efarmpos.MinionsHit >= count && laneE.Count >= count && useq)
                    Q.Cast(minion);
            }

            foreach (var jgl in allJgl)
            {
                var laneJ = GameObjects.GetJungles(ObjectManager.Player.Position, Q.Range + Q2.Width);
                var jfarmpos = Q.GetLineFarmLocation(laneJ, Q2.Width);

                if (jgl.IsValidTarget(Q.Range) && Q.IsReady() &&
                    jfarmpos.MinionsHit >= 1 && laneJ.Count >= 1)
                    Q.Cast(jgl);
            }
        }

        private static void Rsimi()
        {
            var rtarget = TargetSelector.GetTarget(R.Range);
            var Rdmg = R.GetDamage(rtarget);
            if (rtarget.IsValidTarget() && !rtarget.IsDead)
                if (R.IsReady() && rtarget.Health < Rdmg)
                {
                    var rPred = R.GetPrediction(rtarget);
                    if (rPred.Hitchance >= HitChance.High) R.Cast(rPred.CastPosition);
                }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(4000);

            var QA = Menu["Q"].GetValue<MenuBool>("QC");
            var QE = Menu["Q"].GetValue<MenuBool>("QE");
            var WA = Menu["W"].GetValue<MenuBool>("WC");
            var UE = Menu["E"].GetValue<MenuBool>("EC");
            var RC = Menu["R"].GetValue<MenuBool>("RC");
            if (!target.IsValidTarget()) return;
            var Rmana = R.Level > 1 && R.IsReady() ? 100 : 0;
            if (QA && Q.IsReady() && Player.Mana - new[] {70, 80, 90, 100, 110}[Q.Level - 1] > Rmana)
                Qspell(target, QE);
            if (WA && W.IsReady() && W.IsInRange(target) &&
                Player.Mana - new[] {50, 55, 60, 65, 70}[W.Level - 1] > Rmana)
            {
                var wpre = W.GetPrediction(target);
                if (wpre.Hitchance >= HitChance.High) W.Cast(wpre.CastPosition);
            }

            if (UE && E.IsReady() && Player.Distance(target) < Q2.Range && Player.Mana - 70 > Rmana) E.Cast();

            if (RC && R.IsReady() && target.DistanceToPlayer() > Q.Range + 150 &&
                !Player.IsUnderEnemyTurret())
            {
                var Rpre = R.GetPrediction(target);
                var rd = R.GetDamage(target);
                if (Rpre.Hitchance >= HitChance.High && rd > target.Health + target.AllShield && !target.IsDead &&
                    !target.HasBuffOfType(BuffType.Invulnerability) &&
                    !target.HasBuffOfType(BuffType.SpellShield)) R.Cast(Rpre.UnitPosition);
            }
        }


        private static void Harass()
        {
            var target = TargetSelector.GetTarget(2000);

            var QA = Menu["Q"].GetValue<MenuBool>("QH");
            var QE = Menu["Q"].GetValue<MenuBool>("QE");
            var WA = Menu["W"].GetValue<MenuBool>("WH");

            var settedmana = Menu["Q"].GetValue<MenuSlider>("QMana").Value;
            var playermana = Math.Floor(Player.ManaPercent);

            if (playermana < settedmana) return;
            if (!target.IsValidTarget() || target == null) return;

            if (QA && Q.IsReady()) Qspell(target, QE);
            if (WA && W.IsReady() && W.IsInRange(target) && playermana > Menu["W"].GetValue<MenuSlider>("WMana").Value)
            {
                var wpre = W.GetPrediction(target);
                if (wpre.Hitchance >= HitChance.High) W.Cast(wpre.CastPosition);
            }
        }

        #endregion

        #region Args

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserArgs Args)
        {
            if (!Menu["W"].GetValue<MenuBool>("WGap")) return;

            if (W.IsReady() && sender != null && sender.IsValidTarget(W.Range)) W.Cast(Args.EndPosition);
        }

        private void OnInterruptible(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (!Menu["W"].GetValue<MenuBool>("WI")) return;

            if (W.IsReady() && sender != null && sender.IsValidTarget(W.Range))
            {
                var pred = W.GetPrediction(sender);

                if (pred != null && pred.Hitchance >= HitChance.High) W.Cast(pred.UnitPosition);
            }
        }

        private static void OnTeleportEvent(AIBaseClient Sender, Teleport.TeleportEventArgs args)
        {
            if (args != null && Sender.IsValid && Sender.IsEnemy && Sender is AIHeroClient)
            {
                var damage = R.GetDamage(args.Source);
                if (args.Status == Teleport.TeleportStatus.Start &&
                    args.Source.Health + args.Source.AllShield < damage)
                {
                    if (args.Source.HasBuff("willrevive") || args.Source.HasBuff("bansheesveil") ||
                        args.Source.HasBuff("itemmagekillerveil")) return;
                    Chatsent = false;
                    sendername = Sender.CharacterName;
                    recallFinishTime = args.Duration;
                    recallstart = Variables.GameTimeTickCount;
                }

                if (args.Status == Teleport.TeleportStatus.Abort)
                {
                    Chatsent = true;
                    sendername = null;
                    recallFinishTime = 0;
                    recallstart = 0;
                }

                if (args.Status != Teleport.TeleportStatus.Start)
                {
                    Chatsent = true;
                    sendername = null;
                    recallFinishTime = 0;
                    recallstart = 0;
                }
            }
        }

        #endregion Args

        #region Extra functions

        private static int Qdamage(AIHeroClient target)
        {
            var q = 0;
            q = 40 + 30 * (Q.Level - 1);
            var bouns = Player.TotalAttackDamage - Player.BaseAttackDamage;

            var totalqdamage =
                Player.CalculatePhysicalDamage(target, q + bouns * 0.40 + Player.TotalAttackDamage * 0.20);

            if (target.HasBuff("sennapassivemarker"))
            {
                var level = Math.Min(Player.Level, 11);

                switch (level)
                {
                    case 7:
                        level = 8;
                        break;

                    case 8:
                        level = 10;
                        break;

                    case 9:
                        level = 12;
                        break;

                    case 10:
                        level = 14;
                        break;

                    case 11:
                        level = 16;
                        break;
                }

                var passivedmg = Player.CalculatePhysicalDamage(target, level * target.Health / 100);
                totalqdamage = totalqdamage + passivedmg;
            }

            return (int) totalqdamage;
        }

        private static void Qspell(AIHeroClient target, bool useExtendQ = true)
        {
            if (!Q.IsReady() || target == null || target.IsDead ||
                target.HasBuffOfType(BuffType.Invulnerability)) return;
            if (target.IsValidTarget(Q.Range) )
            {
                Q.CastOnUnit(target);
            }
            else if (target.IsValidTarget(Q2.Range) && useExtendQ )
            {
                var collisions =
                    GameObjects.AllGameObjects.Where(x => Q2.IsInRange(x) && x.IsValid)
                        .ToList();

                if (!collisions.Any()) return;
                foreach (var vailds in collisions)
                {
                    var qPred = Q2.GetPrediction(target);
                    var Line = new Geometry.Rectangle(Player.PreviousPosition,
                        Player.PreviousPosition.Extend(vailds.Position, Q2.Range), Q2.Width);
                    if (Line.IsInside(qPred.UnitPosition.ToVector2()) && Q.IsInRange(vailds) )
                    {
                        Q.CastOnUnit(vailds);
                        break;
                    }
                }
            }
        }

        private static void QHeal1()
        {
            var allies = GameObjects.AllyHeroes.ToList().OrderByDescending(x => x.HealthPercent);
            var target = allies.FirstOrDefault(x => !x.InFountain() && x.DistanceToPlayer() <
                                                    Player.AttackRange && x.CharacterName != "Senna" &&
                                                    !x.IsRecalling());
            if (!Q.IsReady() || target == null) return;
            if (Q.IsInRange(target)) Q.Cast(target);
        }

        private static void QHeal()
        {
            var allies = GameObjects.AllyHeroes.ToList().OrderBy(x => x.DistanceToPlayer() < Q2.Range);
            var target = allies.FirstOrDefault(x => !x.InFountain() && x.DistanceToPlayer() <
                                                    Player.AttackRange && x.CharacterName != "Senna" &&
                                                    !x.IsRecalling() &&
                                                    x.HealthPercent <= Menu["Q"].GetValue<MenuSlider>("HealSet").Value);
            if (target == null) return;
            var ally = from hero in ObjectManager.Get<AIHeroClient>()
                where hero.IsAlly
                select hero;

            foreach (var hero in ally.Where(x => x.CharacterName != "Senna"))
            {
                var selectedAllies = alliles.GetValue<MenuBool>(hero.CharacterName);
                if (!Q.IsReady() || !selectedAllies ||
                    Menu["Q"].GetValue<MenuSlider>("QHealM").Value > Player.ManaPercent) return;
                if (Q.IsInRange(target)) Q.Cast(target);
            }
        }

        private static void Levelup()
        {
            if (Math.Abs(Player.PercentCooldownMod) >= 0.8) return; // check if it's urf mode 

            if (Q.Level + W.Level + E.Level + R.Level >= Player.Level || Player.Level > 18) return;

            var level = new[] {0, 0, 0, 0};
            for (var i = 0; i < Player.Level; i++)
                level[SpellLevels[i] - 1] = level[SpellLevels[i] - 1] + 1;

            if (Q.Level < level[0]) Player.Spellbook.LevelSpell(SpellSlot.Q);

            if (W.Level < level[1]) Player.Spellbook.LevelSpell(SpellSlot.W);

            if (E.Level < level[2]) Player.Spellbook.LevelSpell(SpellSlot.E);
            if (R.Level < level[3]) Player.Spellbook.LevelSpell(SpellSlot.R);
        }

        private static void Baseutl()
        {
            var BU = baseutl.GetValue<MenuBool>("BaseUlt");
            var BUping = baseutl.GetValue<MenuBool>("BasePing");
            if (recallFinishTime == 0 || !R.IsReady() || !BU) return;
            var time = recallstart - Variables.GameTimeTickCount + recallFinishTime;
            var objSpawnPoint = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);
            if (objSpawnPoint == null) return;
            var timeToFountain = Player.Position.Distance(objSpawnPoint.Position) / R.Speed * 1000 + R.Delay * 1000;
            if (timeToFountain > recallFinishTime) return;
            if (!(time < timeToFountain)) return;
            if (BUping) Game.SendPing(PingCategory.OnMyWay, objSpawnPoint.Position);
            DelayAction.Add((int) time, Ischat);

            R.Cast(objSpawnPoint.Position);
        }

        private static void Ischat()
        {
            var BUBM = baseutl.GetValue<MenuBool>("BaseBm");
            if (Chatsent || !BUBM) return;
            var random = new Random();
            var text = new List<string>
            {
                "See you in base", "Noobs die in base", "Did ur recall button broke", "LOL what a place to die in",
                "Aim bot works ?? ?? kappa", "you can't hide from skills ?? ", "if I hit u send ur gf nudes",
                "you are already dead"
            };
            var num = random.Next(text.Count);
            Game.Say(text[num] + " " + sendername, true);
            Chatsent = true;
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
            var target = TargetSelector.GetTarget(4000);
            var QA = Menu["KillSteal"].GetValue<MenuBool>("Q");
            var WQA = Menu["KillSteal"].GetValue<MenuBool>("WQ");
            var WA = Menu["KillSteal"].GetValue<MenuBool>("W");
            var RA = Menu["KillSteal"].GetValue<MenuBool>("R");
            bool useBurn = Menu["KillSteal"].GetValue<MenuBool>("D");
            var Ignite = Player.GetSpellSlot("SummonerDot");
            var Spelldot = Player.Spellbook.CanUseSpell(Ignite) == SpellState.Ready;
            if (target == null || target.HasBuffOfType(BuffType.Invulnerability)) return;

            if (QA && target.Health + target.AllShield < Qdamage(target) && Q.IsReady() && Q.IsInRange(target))
                Qspell(target);
            if (WQA && target.Health + target.AllShield < Qdamage(target) && Q.IsReady() && Q2.IsInRange(target) &&
                !Q.IsInRange(target))
            {
                var wardpos = Player.Position.Extend(target.Position, 600);
                Player.Spellbook.CastSpell(SpellSlot.Trinket, wardpos);
                Qspell(target);
            }

            if (WA && W.IsReady() && W.IsInRange(target) && target.Health + target.AllShield < W.GetDamage(target))
            {
                var wpre = W.GetPrediction(target);
                if (wpre.Hitchance >= HitChance.High) W.Cast(wpre.CastPosition);
            }

            if (useBurn && Spelldot &&
                target.Health + target.AllShield < Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite) &&
                Player.Distance(target) < 600) Player.Spellbook.CastSpell(Ignite, target.Position);
            if (!RA || !target.IsValid) return;
            if (!R.IsReady() || !(target.Health + target.AllShield < R.GetDamage(target)) || W.IsInRange(target)) return;
            var rPred = R.GetPrediction(target);
            if (rPred.Hitchance >= HitChance.High) R.Cast(rPred.UnitPosition);
        }

        private static void AutoCollect()
        {
            var mists = GameObjects.AttackableUnits.Where(x => x.Name == "Barrel").OrderBy(x => x.DistanceToPlayer())
                .FirstOrDefault();
            if (mists != null) Orbwalker.Attack(mists);
        }

        #endregion
    }
}