using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace MightyAio.Champions
{
    internal class Irelia
    {
        #region Starter

        private static AIHeroClient Player => ObjectManager.Player;

        private static Menu Menu, Emotes, Rlist;

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            Menu = new Menu("Irelia", "Beta Irelia", true);

            var Combo = new Menu("Combo", "Combo")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QG", "Use Q for Gapclose on Minions"),
                new MenuBool("JA", "Use Q to Jump-Around on Minions"),
                new MenuBool("SP", " ^- Only to stack passive"),
                new MenuSlider("manaStack", " ^- Stop if Mana <= X", 50),
                new MenuBool("prioMarked", "Priority Marked Enemies"),
                new MenuKeyBind("markedKey", "Only if Marked Toggle", Keys.A, KeyBindType.Toggle),
                new MenuBool("WC", "Use W in Combo", false),
                new MenuSlider("autoRelease", "Automatically release after (ms)", 100, 1, 1500),
                new MenuBool("EC", "Use E in Combo"),
                new MenuList("rUsage", "R Usage: ", new[] {"At X Health", "Killable", "Never"}),
                new MenuSlider("HPR", " ^- if <= X HP", 60),
                new MenuSlider("wasteR", "Don't waste R if Enemy HP <= X", 5),
                new MenuSlider("forceR", "Force R if Hits X", 2, 0, 5),
                new MenuKeyBind("semiR", "Semi-R", Keys.T, KeyBindType.Press)
            };
            Rlist = new Menu("RN", "Use R On");
            var targets = from hero in ObjectManager.Get<AIHeroClient>()
                where hero.IsEnemy
                select hero;
            foreach (var target in targets)
                Rlist.Add(new MenuBool(target.CharacterName, "Use R on " + target.CharacterName));
            Combo.Add(Rlist);
            Menu.Add(Combo);
            var Harass = new Menu("Harass", "Harass")
            {
                new MenuBool("QH", "Use Q in Harass"),
                new MenuBool("QG", "Use Q for Gapclose on Minions"),
                new MenuBool("WH", "Use W in Harass", false),
                new MenuBool("EH", "Use E in Harass"),
                new MenuSlider("HM", "Only Harass When Mana Percent >", 40)
            };
            Menu.Add(Harass);
            // lane clear
            var laneclear = new Menu("laneclear", "Lane Clear")
            {
                new MenuBool("Q", "Use Q to Last Hit"),
                new MenuBool("turretQ", "Don't use Under-Turret"),
                new MenuBool("qaa", "Don't Q in Auto Attack Range", false),
                new MenuSlider("QM", "Use Q when manapercent > ", 30)
            };
            Menu.Add(laneclear);
            var Jungleclear = new Menu("JungleClear", "Jungle Clear")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("QM", " ^- Only if Marked"),
                new MenuBool("E", "Use E")
            };
            Menu.Add(Jungleclear);
            var LastHit = new Menu("LastHit", "Last Hit")
            {
                new MenuBool("Q", "Use Q to Last Hit")
            };
            Menu.Add(LastHit);
            // kill steal
            var killsteal = new Menu("KillSteal", "Kill Steal")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("E", "E"),
                new MenuBool("W", "Use W")
            };
            Menu.Add(killsteal);
            var itembuy = new Menu("autoitem", "Starter Item")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Corrupting Potion", "Dorans Blade", "none"})
            };
            Menu.Add(itembuy);
            // Misc
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 18, 0, 55),
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
                new MenuBool("DrawR", "Draw R"),
                new MenuBool("Drawkillabeabilities", "Draw kill able abilities")
            };
            Menu.Add(drawMenu);

            Menu.Attach();
        }

        #endregion Menu

        #region Spells

        private static Font Berlinfont;

        private static int mykills = 0 + Player.ChampionsKilled;
        private static Spell Q, W, E, R;
        private static int[] SpellLevels;
        private static Dictionary<uint, AIBaseClient> posibleTargets = new Dictionary<uint, AIBaseClient>();
        private static Vector3 e1Pos = Vector3.Zero;
        private static Vector3 castPos = Vector3.Zero;

        private static Vector3 multiPos = Vector3.Zero;
        private static Vector3 multiPos2 = Vector3.Zero;

        private static float lastCast;
        private static float qDelay;
        private static float triesMulti;
        private static List<AIMinionClient> KillableMinions = new List<AIMinionClient>();

        private static float wHeld = 200f;
        private static float sheenTimer;

        private static float chargeW;

        #endregion Spells

        #region GameLoad

        public Irelia()
        {
            SpellLevels = new[] {1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2};
            CreateMenu();
            Q = new Spell(SpellSlot.Q, 600f);
            Q.SetTargetted(0, 1500f);
            W = new Spell(SpellSlot.W, 825f);
            W.SetCharged("IreliaW", "ireliawdefense", 800, 800, 0);
            E = new Spell(SpellSlot.E, 850f);
            E.SetSkillshot(0f, 90f, 2000f, false, SkillshotType.Line);
            R = new Spell(SpellSlot.R, 850f);
            R.SetSkillshot(0.35f, 160f, 2000f, false, SkillshotType.Line);

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
            Dash.OnDash += DashOnOnDash;
            Game.OnWndProc += GameOnOnWndProc;
            AIBaseClient.OnBuffLose += AIBaseClientOnOnBuffLose;
            GameObject.OnDelete += AIBaseClientOnOnDelete;
            Interrupter.OnInterrupterSpell += InterrupterOnOnInterrupterSpell;
            AIBaseClient.OnProcessSpellCast += AIBaseClientOnOnProcessSpellCast;
            Game.OnNotify += delegate(GameNotifyEventArgs args)
            {
                if (args.EventId == GameEventId.OnReincarnate && Menu["Misc"].GetValue<MenuBool>("UseSkin"))
                    Player.SetSkin(Menu["Misc"].GetValue<MenuSlider>("setskin").Value);
            };
        }

        #endregion

        #region args

        private void InterrupterOnOnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (E.IsReady() && args.DangerLevel == Interrupter.DangerLevel.High)
            {
                E1();
                E2();
            }
        }

        private void AIBaseClientOnOnDelete(GameObject sender, EventArgs args)
        {
            if (sender == null) return;

            if (sender.Name.Contains("E_Blades")) e1Pos = Vector3.Zero;
        }

        private void AIBaseClientOnOnBuffLose(AIBaseClient sender, AIBaseClientBuffLoseEventArgs args)
        {
            if (sender.IsMe)
                if (args.Buff.Name == "sheen" || args.Buff.Name == "TrinityForce")
                    sheenTimer = Variables.GameTimeTickCount + 1.7f;
        }

        private void GameOnOnWndProc(GameWndProcEventArgs args)
        {
            if (args.WParam == (ulong) Keys.W)
                if (args.Msg == (ulong) WindowsMessages.KEYFIRST)
                    wHeld = Variables.GameTimeTickCount;
        }

        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;

            if (args.Slot == SpellSlot.E)
                e1Pos = args.End.DistanceToPlayer() <= E.Range
                    ? args.End
                    : Player.Position.Extend(args.End, E.Range);

            if (args.Slot == SpellSlot.W) chargeW = Variables.GameTimeTickCount;
        }

        private void DashOnOnDash(AIBaseClient sender, Dash.DashArgs args)
        {
            if (Player.IsDead || !sender.IsEnemy) return;

            if (e1Pos.IsZero && Player.GetSpell(SpellSlot.E).Name == "IreliaE2")
                if (args.EndPos.DistanceToPlayer() <= E.Range)
                {
                    E.Cast(args.EndPos);
                    e1Pos = Vector3.Zero;
                }
        }


        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Menu["Drawing"].GetValue<MenuBool>("DrawQ");
            var drawR = Menu["Drawing"].GetValue<MenuBool>("DrawW");
            var drawE = Menu["Drawing"].GetValue<MenuBool>("DrawR");
            var drawKill = Menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities");
            var p = Player.Position;

            if (drawQ && Q.IsReady())
                Drawing.DrawCircle(p, Q.Range, Color.Purple);
            if (drawE && E.IsReady()) Drawing.DrawCircle(p, E.Range, Color.Purple);
            if (drawR && R.IsReady()) Drawing.DrawCircle(p, R.Range, Color.Purple);

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
                        if (Qdamage(enemyVisible) > enemyVisible.Health)
                            DrawText(Berlinfont, "Killable Skills (Q):",
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                        else if (Qdamage(enemyVisible) + W.GetDamage(enemyVisible) > enemyVisible.Health)
                            DrawText(Berlinfont, "Killable Skills (Q + W):",
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                        else
                            DrawText(Berlinfont, aa, (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                    }
                }
        }

        #endregion GameLoad

        #region Update

        private static void GameOnOnUpdate(EventArgs args)
        {
            if (Player.IsRecalling() || Player.IsDead) return;
            if (Player.ChampionsKilled > mykills && Emotes.GetValue<MenuBool>("Kill"))
            {
                mykills = Player.ChampionsKilled;
                Emote();
            }

            var gold = Player.Gold;
            var time = Game.Time / 60;
            var item = Menu["autoitem"].GetValue<MenuList>("selectitem").SelectedValue;

            if (item != "none" && Game.MapId == GameMapId.SummonersRift)  {
                switch (item)
                {
                    case "Dorans Blade":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 500 && !Player.HasItem(ItemId.Dorans_Blade))
                                Player.BuyItem(ItemId.Dorans_Blade);
                            if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                                Player.BuyItem(ItemId.Health_Potion);
                        }

                        break;
                    }
                    case "Corrupting Potion":
                    {
                        if (time < 1 && Player.InShop())
                            if (gold >= 500 && !Player.HasItem(ItemId.Corrupting_Potion))
                                Player.BuyItem(ItemId.Corrupting_Potion);
                        break;
                    }
                }
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
                    LastHit();
                    break;
            }

            AutoW();
            Killsteal();
            if (Menu["Misc"].GetValue<MenuBool>("autolevel")) Levelup();
        }

        #endregion Update


        #region Orbwalker mod

        private static void LaneClear()
        {
            MinionList();
            var minions = KillableMinions;
            if (!minions.Any() ) return ;
            foreach (var minion in minions)
            {
               if( minion.Health > Qdamage(minion)) return;
               if (!Menu["laneclear"].GetValue<MenuBool>("Q") ||
                   Menu["laneclear"].GetValue<MenuSlider>("QM").Value > Player.ManaPercent) return;
                Q.CastOnUnit(minion);
            }
               
        }

        private static void LastHit()
        {
            MinionList();
            var minions = KillableMinions;
            if (!minions.Any() ) return ;
            foreach (var minion in minions)
            {
                if( minion.Health > Qdamage(minion)) return;
                if (!Menu["LastHit"].GetValue<MenuBool>("Q")) return;
                Q.CastOnUnit(minion);
            }
        }

        private static void JungleClear()
        {
            if (Menu["JungleClear"].GetValue<MenuBool>("E") && E.IsReady())
            {
                var minionAll = GameObjects.Jungle.Where(x => x.IsValidTarget(E.Range))
                    .OrderBy(z => z.DistanceToPlayer())
                    .FirstOrDefault();
                if (minionAll == null) return;

                E.Cast(minionAll.Position);
            }
            if (Menu["JungleClear"].GetValue<MenuBool>("Q") && Q.IsReady())
            {

                if (Menu["JungleClear"].GetValue<MenuBool>("QM"))
                {
                    var minion = GameObjects.Jungle
                        .Where(x => x.IsValidTarget(Q.Range) && x.HasBuff("ireliamark"))
                        .OrderByDescending(z => z.MaxHealth).FirstOrDefault();
                    if (minion != null) Q.CastOnUnit(minion);
                    var minionAll = GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range))
                        .OrderByDescending(z => z.MaxHealth)
                        .FirstOrDefault();
                    if (minionAll != null && Qdamage(minionAll) * 0.75 > minionAll.Health) Q.CastOnUnit(minionAll);
                }
                else
                {
                    var minion = GameObjects.Jungle
                        .Where(x => x.IsValidTarget(Q.Range) && x.HasBuff("ireliamark"))
                        .OrderByDescending(z => z.MaxHealth).FirstOrDefault();
                    if (minion != null) Q.CastOnUnit(minion);

                    var minionAll = GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range))
                        .OrderByDescending(z => z.MaxHealth)
                        .FirstOrDefault();
                    if (minionAll != null && Qdamage(minionAll) * 0.75 > minionAll.Health) Q.CastOnUnit(minionAll);
                }
            }

     
        }

        private static void Combo()
        {
            if (Menu["Combo"].GetValue<MenuBool>("QC")) castQ();
            if (Menu["Combo"].GetValue<MenuBool>("EC") && !Player.IsDashing())
            {
                E1();
                E2();
            }

            if (Menu["Combo"].GetValue<MenuBool>("QG")) Qgap();
            if (Menu["Combo"].GetValue<MenuBool>("WC")) CastW();
            CastR();
        }


        private static void Harass()
        {
            if (Player.ManaPercent < Menu["Harass"].GetValue<MenuSlider>("HM").Value) return;
            if (Menu["Harass"].GetValue<MenuBool>("QH")) castQ();
            if (Menu["Harass"].GetValue<MenuBool>("EH") && !Player.IsDashing())
            {
                E1();
                E2();
            }

            if (Menu["Harass"].GetValue<MenuBool>("QG")) Qgap();
            if (Menu["Harass"].GetValue<MenuBool>("WH")) CastW();
        }

        #endregion

        #region spell Functions

        private static void castQ()
        {
            if (!Q.IsReady()) return;
            if (qDelay < Variables.GameTimeTickCount)
            {
                if (Menu["Combo"].GetValue<MenuBool>("prioMarked"))
                {
                    var enemy = GameObjects.EnemyHeroes.FirstOrDefault(x =>
                        x.IsValidTarget(Q.Range) && x.HasBuff("ireliamark"));
                    if (enemy != null)
                    {
                        Q.CastOnUnit(enemy);
                        qDelay = Variables.GameTimeTickCount + 0.5f;
                    }
                }

                var target = TargetSelector.GetTarget(Q.Range);
                if (target != null)
                {
                    if (!Menu["Combo"].GetValue<MenuKeyBind>("markedKey").Active)
                        if (lastCast < Variables.GameTimeTickCount)
                        {
                            Q.CastOnUnit(target);
                            qDelay = Variables.GameTimeTickCount + 0.5f;
                        }

                    if (target.Health < Qdamage(target) + Player.TotalAttackDamage && Player.CountEnemyHeroesInRange(1000) == 1)
                        if (lastCast < Variables.GameTimeTickCount)
                        {
                            Q.CastOnUnit(target);
                            qDelay = Variables.GameTimeTickCount + 0.5f;
                        }

                    if (Menu["Combo"].GetValue<MenuKeyBind>("markedKey").Active && target.HasBuff("ireliamark"))
                    {
                        Q.CastOnUnit(target);
                        qDelay = Variables.GameTimeTickCount + 0.5f;
                    }

                    if (Menu["Combo"].GetValue<MenuBool>("JA") && e1Pos.IsZero)
                    {
                        if (Menu["Combo"].GetValue<MenuBool>("SP") &&
                            (!Player.HasBuff("ireliapassivestacks") ||
                             Player.Buffs.Count(x => x.Name == "ireliapassivestacks") < 4))
                        {
                            var bestJump = BestJump(target);
                            if (bestJump != null && bestJump.Health < Qdamage(bestJump)) Q.CastOnUnit(bestJump);
                        }

                        if (!Menu["Combo"].GetValue<MenuBool>("SP"))
                        {
                            var bestJump = BestJump(target);
                            if (bestJump != null && bestJump.Health < Qdamage(bestJump)) Q.CastOnUnit(bestJump);
                        }
                    }
                }
            }
        }

        private static void Qgap()
        {
            if (!Q.IsReady()) return;

            var targetGap = TargetSelector.GetTarget(Q.Range * 2);
            if (targetGap != null)
            {
                var bestMinion = BestGap(targetGap);
                if (bestMinion != null && Player.ManaPercent >= 50)
                    if (bestMinion.Distance(targetGap) < targetGap.DistanceToPlayer() &&
                        targetGap.DistanceToPlayer() >= 250 && bestMinion.Health < Qdamage(bestMinion))
                        Q.CastOnUnit(bestMinion);
            }
        }

        private static void E1()
        {
            if (E.IsReady())
            {
                var targetE = TargetSelector.GetTarget(E.Range - 150);
                if (targetE != null)
                {
                    MultiE(targetE);
                    if (targetE.DistanceToPlayer() <= E.Range &&
                        Variables.GameTimeTickCount - E.LastCastAttemptT > 1000 &&
                        Player.GetSpell(SpellSlot.E).Name == "IreliaE" &&
                        lastCast < Variables.GameTimeTickCount &&
                        triesMulti < Variables.GameTimeTickCount)
                    {
                        var pathStartPos = targetE.Path[0];
                        var pathEndPos = targetE.Path[targetE.Path.Count - 1];
                        var pathNorm = (pathEndPos - pathStartPos).Normalized();
                        var tempPred = PredictedPosition(targetE, 1.2f);

                        if (targetE.Path.Count <= 1)
                        {
                            var cast1 = Player.Position +
                                        (targetE.Position - Player.Position).Normalized() * 900;
                            if (cast1.DistanceToPlayer() <= 1000)
                            {
                                E.Cast(cast1);
                                e1Pos = cast1;
                                lastCast = Variables.GameTimeTickCount + 1;
                                return;
                            }
                        }

                        if (targetE.Path.Count > 1)
                            if (tempPred != Vector3.Zero)
                            {
                                var dist1 = tempPred.DistanceToPlayer();
                                if (dist1 <= 900)
                                {
                                    var dist2 = targetE.DistanceToPlayer();
                                    if (dist1 < dist2) pathNorm *= -1;

                                    var cast2 = RayDistance(targetE.Position,
                                        pathNorm,
                                        Player.Position,
                                        900);
                                    if (cast2.DistanceToPlayer() <= 1000)
                                    {
                                        E.Cast(cast2);
                                        e1Pos = cast2;
                                    }
                                }
                            }
                    }
                }
            }
        }

        private static void E2()
        {
            if (E.IsReady())
            {
                var targetE = TargetSelector.GetTarget(E.Range);
                if (targetE != null)
                    if (!targetE.HasBuff("ireliamark") && Player.GetSpell(SpellSlot.E).Name == "IreliaE2" &&
                        triesMulti < Variables.GameTimeTickCount)
                        if (e1Pos != Vector3.Zero)
                        {
                            
                            var short1 = false;
                            var short2 = false;

                            E.Delay = 0.25f + targetE.DistanceToPlayer() / 2000f;
                            E.UpdateSourcePosition(e1Pos, Player.Position);

                            var predictions = E.GetPrediction(targetE);
                            if (predictions.Hitchance < HitChance.Medium) return;
                            var predPos1 = predictions.CastPosition;
                            if (predPos1.DistanceToPlayer() <= E.Range )
                            {
                                var tempCastPos = Vector3.Zero;
                                var start = e1Pos.ToVector2();
                                var end = predPos1.ToVector2();
    
                                var projection = Player.Position.ToVector2().ProjectOn(start, end);
                                if (projection.SegmentPoint != Vector2.Zero)
                                {
                                    var closest = projection.SegmentPoint.ToVector3();

                                    if (closest.DistanceToPlayer() > E.Range ||
                                        predPos1.Distance(e1Pos) > closest.Distance(e1Pos) ||
                                        closest.Distance(e1Pos) < targetE.MoveSpeed * 0.625 * 1.5)
                                    {
                                        short1 = true;

                                        var pathNormE = (predPos1 - e1Pos).Normalized();
                                        var extendPos =
                                            e1Pos +
                                            pathNormE *
                                            (predPos1.Distance(e1Pos) + targetE.MoveSpeed * 0.625f * 1.5f);

                                        if (extendPos.DistanceToPlayer() < E.Range)
                                            tempCastPos = extendPos;
                                        else
                                            tempCastPos =
                                                RayDistance(e1Pos, pathNormE,
                                                    Player.Position, E.Range);
                                    }
                                    else
                                    {
                                        tempCastPos = closest;
                                    }
                                }

                                if (tempCastPos != Vector3.Zero)
                                {
                                    E.Delay = 0.25f + tempCastPos.DistanceToPlayer() / 2000f;
                                    E.Speed = float.MaxValue;

                                    if (projection.SegmentPoint != Vector2.Zero)
                                    {
                                        castPos = Vector3.Zero;

                                        var closest = projection.SegmentPoint.ToVector3();

                                        if (closest.DistanceToPlayer() > E.Range ||
                                            predPos1.Distance(e1Pos) > closest.Distance(e1Pos) ||
                                            closest.Distance(e1Pos) < targetE.MoveSpeed * 0.625 * 1.5)
                                        {
                                            short2 = true;
                                            var pathNormE = (predPos1 - e1Pos).Normalized();
                                            var extendPos =
                                                e1Pos +
                                                pathNormE *
                                                (predPos1.Distance(e1Pos) + targetE.MoveSpeed * 0.625f * 1.5f);
                                            if (extendPos.DistanceToPlayer() < E.Range)
                                                castPos = extendPos;
                                            else
                                                castPos = RayDistance(
                                                    e1Pos,
                                                    pathNormE,
                                                    Player.Position,
                                                    E.Range);
                                        }
                                        else
                                        {
                                            castPos = closest;
                                        }

                                        if (short1 == short2 && castPos != Vector3.Zero &&
                                            castPos.DistanceToPlayer() <= E.Range)
                                        {
                                            E.Cast(castPos);
                                            e1Pos = Vector3.Zero;
                                        }
                                    }
                                }
                            }
                        }
            }
        }

        private static void AutoW()
        {
            var target = TargetSelector.GetTarget(W.Range);
            if (target == null) W.ShootChargedSpell(Player.Position);
            if (target != null)
            {
                var prediction = W.GetPrediction(target);
                if (Player.HasBuff("ireliawdefense"))
                {
                    if (Variables.GameTimeTickCount - wHeld >= 0 && Variables.GameTimeTickCount - wHeld <= 0.3)
                        if (Variables.GameTimeTickCount - chargeW > 1.4f)
                            if (prediction.Hitchance != HitChance.OutOfRange)
                                W.ShootChargedSpell(prediction.CastPosition);

                    if (Variables.GameTimeTickCount - wHeld >= 1)
                        if (Variables.GameTimeTickCount - chargeW >
                            Menu["Combo"].GetValue<MenuSlider>("autoRelease").Value / 1000f)
                            if (prediction.Hitchance != HitChance.OutOfRange)
                                W.ShootChargedSpell(prediction.CastPosition);

                    if (target.DistanceToPlayer() >= W.Range - 100)
                        if (prediction.Hitchance != HitChance.OutOfRange)
                            W.ShootChargedSpell(prediction.CastPosition);
                }
            }
        }


        private static void CastW()
        {
            if (!e1Pos.IsZero) return;

            var target = TargetSelector.GetTarget(W.Range - 150);
            if (target != null && !Q.IsReady())
                if (!Player.HasBuff("ireliawdefense"))
                    W.StartCharging(target.Position);
        }

        private static void CastR()
        {
            var Rmode = Menu["Combo"].GetValue<MenuList>("rUsage").SelectedValue;
            if (Rmode == "Never") return;

            var target = TargetSelector.GetTarget(R.Range);
            if (target == null) return;

            if (!Rlist.GetValue<MenuBool>(target.CharacterName)) return;

            if (target.Position.CountEnemyHeroesInRange(400) >= Menu["Combo"].GetValue<MenuSlider>("forceR").Value)
                R.Cast(target);

            if (Menu["Combo"].GetValue<MenuSlider>("wasteR").Value >= target.HealthPercent) return;
            // At X Health
            if (Rmode == "At X Health")
                if (target.HealthPercent <= Menu["Combo"].GetValue<MenuSlider>("HPR").Value)
                    R.Cast(target);

            // If Killable
            if (Rmode == "Killable")
                if (target.Health <=
                    E.GetDamage(target) + Qdamage(target) + R.GetDamage(target) * 2 + W.GetDamage(target))
                    R.Cast(target);
        }

        #endregion

        #region Damages

        private static float Qdamage(AIBaseClient target)
        {
            float a = 0;
            var damage = 5 + 20 * (Q.Level - 1) ;
            var extra = Player.TotalAttackDamage * 0.6;
            var total = damage + extra;
            if (target is AIMinionClient) total += 45 + 15 * (Q.Level - 1);

            total += Sheen(target);
            if (Player.Buffs.Count(x => x.Name == "ireliapassivestacks") > 4) a = PassiveDamage(target);
            return (float) Player.CalculateDamage(target, DamageType.Physical, total) + a;
        }

        private static float PassiveDamage(AIBaseClient target)
        {
            var ireliaPassiveDamage =
                (3.235f +
                 0.765f * Player.Level +
                 0.04f * Player.PercentBonusPhysicalDamageMod) *
                Player.Buffs.Count(x => x.Name == "ireliapassivestacks");

            return (float) Player.CalculateDamage(target, DamageType.Magical, ireliaPassiveDamage);
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

        private static AIMinionClient BestJump(AIBaseClient target)
        {
            var bestJump = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) &&
                                                               x.Distance(target) < Q.Range &&
                                                               !x.Position.IsUnderEnemyTurret())
                .OrderBy(x => x.Health).ToList();
            return bestJump.FirstOrDefault();
        }

        private static AIMinionClient BestGap(AIBaseClient target)
        {
            var bestJump = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) &&
                                                               x.HasBuff("ireliamark"))
                .OrderBy(x => x.Health).ToList();

            return bestJump.OrderBy(x => x.Distance(target)).FirstOrDefault();
        }

        private static void MinionList()
        {
            if (Menu["laneclear"].GetValue<MenuBool>("turretQ"))
                KillableMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) &&
                                                                      !x.Position.IsUnderEnemyTurret() &&
                                                                      x.DistanceToPlayer() >
                                                                      (Menu["laneclear"].GetValue<MenuBool>("qaa")
                                                                          ? 250
                                                                          : 0))
                    .OrderBy(x => x.Health)
                    .ToList();
            else
                KillableMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) &&
                                                                      x.DistanceToPlayer() >
                                                                      (Menu["laneclear"].GetValue<MenuBool>("qaa")
                                                                          ? 250
                                                                          : 0)).OrderBy(x => x.Health)
                    .ToList();
        }

        private static void MultiE(AIBaseClient target)
        {
            if (!E.IsReady() || target == null) return;

            var prediction = E.GetPrediction(target);
            if (Player.GetSpell(SpellSlot.E).Name == "IreliaE" && target.DistanceToPlayer() <= E.Range)
            {
                var best = bestTarget(target);
                if (best != null)
                {
                    var prediction2 = E.GetPrediction(best);
                    var EPOS = prediction.CastPosition +
                               (prediction2.CastPosition - prediction.CastPosition).Normalized() *
                               (prediction.CastPosition.Distance(prediction2.CastPosition) + 275);
                    if (!EPOS.IsZero)
                    {
                        E.Cast(EPOS);
                        triesMulti = Variables.GameTimeTickCount + 1f;
                        multiPos = EPOS;
                    }
                }
            }

            if (multiPos != Vector3.Zero && Player.GetSpell(SpellSlot.E).Name == "IreliaE2")
            {
                var EPOS = multiPos +
                           (prediction.CastPosition - multiPos).Normalized() *
                           (multiPos.Distance(prediction.CastPosition) + 275);
                if (!EPOS.IsZero)
                {
                    E.Cast(EPOS);
                    multiPos2 = EPOS;
                    multiPos = Vector3.Zero;
                }
            }
        }

        private static AIBaseClient bestTarget(AIBaseClient target)
        {
            AIBaseClient bestTarget = null;
            var lastEnemyCount = 0;
            var list = new List<Vector3> {Player.Position, Vector3.Zero, target.Position};
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x =>
                x.NetworkId != target.NetworkId && x.IsValidTarget(E.Range)))
            {
                var pred = SpellPrediction.GetPrediction(enemy,
                    0.25f,
                    90f,
                    2000f,
                    CollisionObjects.Heroes);
                list[1] = pred.CastPosition;
                if (pred.CollisionObjects != null)
                {
                    var count = pred.CollisionObjects.Count(x => !x.IsMinion || !x.IsMonster && x.IsEnemy);

                    if (lastEnemyCount < count)
                    {
                        lastEnemyCount = count;
                        bestTarget = enemy;
                    }
                }
            }

            return bestTarget;
        }

        private static Vector3 RayDistance(Vector3 start, Vector3 path, Vector3 center, float dist)
        {
            var a = start.X - center.X;
            var b = start.Y - center.Y;
            var c = start.Z - center.Z;
            var x = path.X;
            var y = path.Y;
            var z = path.Z;

            var n1 = a * x + b * y + c * z;

            var n2 = Math.Pow(z, 2) * Math.Pow(dist, 2) -
                     Math.Pow(a, 2) * Math.Pow(z, 2) -
                     Math.Pow(b, 2) * Math.Pow(z, 2) +
                     2 * a * c * x * z +
                     2 * b * c * y * z +
                     2 * a * b * x * y +
                     Math.Pow(dist, 2) * Math.Pow(x, 2) +
                     Math.Pow(dist, 2) * Math.Pow(y, 2) -
                     Math.Pow(a, 2) * Math.Pow(y, 2) -
                     Math.Pow(b, 2) * Math.Pow(x, 2) -
                     Math.Pow(c, 2) * Math.Pow(x, 2) -
                     Math.Pow(c, 2) * Math.Pow(y, 2);
            var n3 = Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2);

            var r1 = -(n1 + Math.Sqrt(n2)) / n3;
            var r2 = -(n1 - Math.Sqrt(n2)) / n3;
            var r = Math.Max(r1, r2);

            return start + path * (float) r;
        }

        private static Vector3 PredictedPosition(AIBaseClient target, float time)
        {
            if (target.Buffs.Any(b =>
                (b.IsStun || b.IsCharm || b.IsAsleep || b.IsRoot || b.IsFear || b.IsFlee || b.IsKnockback ||
                 b.IsSuppression || b.IsSlow || b.IsTaunt) && b.EndTime <= time)) return target.Position;

            var distance = target.MoveSpeed * (time + Game.Ping / 1000f);
            if (target.Path[target.Path.Count - 1].Distance(target.Position) <= distance)
                distance = target.Path[target.Path.Count - 1].Distance(target.Position);

            return target.Position.Extend(target.Path[target.Path.Count - 1], target.IsMoving ? distance : 0);
        }

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
            var target = TargetSelector.GetTarget(W.Range);
            if (target == null) return;
            if (target.HasBuffOfType(BuffType.Invulnerability) || target.HasBuff("UndyingRage")) return;
            if (Qdamage(target) >= target.Health)
            {
                if (!Menu["KillSteal"].GetValue<MenuBool>("Q")) return;
                Q.CastOnUnit(target);
            }

            if (W.GetDamage(target) >= target.Health && !Q.IsInRange(target))
            {
                if (!Menu["KillSteal"].GetValue<MenuBool>("W")) return;
                W.StartCharging(target.Position);
                W.ShootChargedSpell(target.Position);
            }

            if (E.GetDamage(target) >= target.Health)
            {
                if (Menu["KillSteal"].GetValue<MenuBool>("E"))
                {
                    E1();
                    E2();
                }
            }
        }

        #endregion
    }
}