using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
    internal class Yone
    {
        #region Basics

        private static Spell Q, Q3 , W, E, R , Flash;
        private static Menu Menu, Emotes;
        private static AIHeroClient Player => ObjectManager.Player;
        private static Font _berlinfont;
        private static int _mykills = 0 + Player.ChampionsKilled;
        private static int[] _spellLevels;
        private static float sheenTimer;

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            Menu = new Menu("MightyYone", "Mighty Yone", true);
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("Q", "Use Q in combo"),
                new MenuBool("Q3", "Use 3Q in combo"),
                new MenuBool("QA", "Auto stack Q"),
                new MenuBool("QH", "Use Q in Harass"),
                new MenuBool("QH3", "Use 3Q in Harass",false),
            };
            Menu.Add(qMenu);
            var eMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("ED", "Use E to remove debuffs"),
                new MenuSlider("ER","E range +",100,50,500)
            };
            Menu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuBool("R", "Use in Combo R"),
                new MenuKeyBind("RT", "Simi R Key", Keys.T, KeyBindType.Press),
                new MenuSlider("RH", " R || The target health is below %", 50),
                new MenuSliderButton("RF", "Use R flash Q Combo || when u can hit ",3,5,3)
            };
            Menu.Add(rMenu);

            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("W", "Use W")
            };
            Menu.Add(laneClearMenu);
            var JungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("W", "Use W")
            };

            Menu.Add(JungleClearMenu);
            var killSteal = new Menu("KS", "KillSteal")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("W", "Use W"),
                new MenuBool("R", "Use R", false)
            };
            Menu.Add(killSteal);
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 1, 0, 55),
                new MenuBool("autolevel", "Auto Level"),
                new MenuBool("UseE", "Auto E on dasher")
            };
            var itembuy = new Menu("autoitem", "Starter Item")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Dorans Blade", "none"})
            };
            Menu.Add(itembuy);
            // use emotes
            Emotes = new Menu("Emotes", "Emotes")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Center", "East", "West", "South", "North", "Mastery"}),
                new MenuBool("Kill", "Use on kill")
            };
            miscMenu.Add(Emotes);
            Menu.Add(miscMenu);
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawW", "Draw W"),
                new MenuBool("DrawE", "Draw E",false),
                new MenuBool("DrawR", "Draw R"),
                new MenuBool("Drawkillabeabilities", "Draw kill able abilities")
            };
            Menu.Add(drawMenu);


            Menu.Attach();
        }

        #endregion menu

        #region Menu Helper

       private static int ERPlus => Menu["E"].GetValue<MenuSlider>("ER").Value;
        

        #endregion

        #region Gamestart

        public Yone()
        {
            _spellLevels = new[] {1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2};
            Q = new Spell(SpellSlot.Q, 450);
            Q.SetSkillshot(0.4f, 20f,1500f,false,SkillshotType.Line);
            Q3 = new Spell(SpellSlot.Q, 950);
            Q3.SetSkillshot(0.4f, 160f,1500f,false,SkillshotType.Line);
            W = new Spell(SpellSlot.W, 600f);
            W.SetSkillshot(0.5f,600f,1500f,false,SkillshotType.Cone);
            E = new Spell(SpellSlot.E, 300f);
            E.SetSkillshot(0f, 20f, 1200f, false, SkillshotType.Line);
            R = new Spell(SpellSlot.R,1000f);
            R.SetSkillshot(0.75f, 225f, 1500f, false, SkillshotType.Line);
            CreateMenu();
            _berlinfont = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Berlin San FB Demi",
                    Height = 23,
                    Weight = FontWeight.DemiBold,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearTypeNatural
                });
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnProcessSpellCast += AIBaseClientOnOnProcessSpellCast;
            Orbwalker.OnAction += OrbwalkerOnOnAction;
            Game.OnNotify += delegate(GameNotifyEventArgs args)
            {
                if (args.EventId == GameEventId.OnReincarnate && Menu["Misc"].GetValue<MenuBool>("UseSkin"))
                    Player.SetSkin(Menu["Misc"].GetValue<MenuSlider>("setskin").Value);
            };
            AIBaseClient.OnBuffLose += delegate(AIBaseClient sender, AIBaseClientBuffLoseEventArgs args)
            {
                if (sender.IsMe)
                    if (args.Buff.Name == "sheen" || args.Buff.Name == "TrinityForce")
                        sheenTimer = Variables.GameTimeTickCount + 1.7f;
            };
           
        }

        #endregion

        #region args

        private void OrbwalkerOnOnAction(object sender, OrbwalkerActionArgs args)
        {
            if (args.Type == OrbwalkerType.AfterAttack && Orbwalker.ActiveMode == OrbwalkerMode.Combo && args.Target is AIHeroClient) CastQ2();
        }

        private static void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender,
            AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe) Game.Print(args.SData.MissileSpeed);
            
            if (sender.IsMe) Game.Print(args.SData.Name);
            if (args.Target == null) return;
            if (sender.IsMe && args.Target.IsEnemy && args.Target is AIHeroClient) Game.SendEmote(EmoteId.Laugh);
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

            var drawKill = Menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities");
            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget(2000)))
            {
                if (!enemyVisible.IsValidTarget()) continue;
                var autodmg = Autodamage(enemyVisible);
                var aa = string.Format("AA Left:" + (int) (enemyVisible.Health / autodmg));
                if (!drawKill) continue;
                if (Qdmg(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (Qdmg(enemyVisible) + Wdmg(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + W):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (Qdmg(enemyVisible) + Rdmg(enemyVisible) + Wdmg(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + W + R):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else
                    DrawText(_berlinfont, aa, (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
            }
        }

        #endregion

        #region gameupdate

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.ChampionsKilled > _mykills && Emotes.GetValue<MenuBool>("Kill"))
            {
                _mykills = Player.ChampionsKilled;
                Emote();
            }

            buyitem();
            var getskin = Menu["Misc"].GetValue<MenuSlider>("setskin").Value;
            var skin = Menu["Misc"].GetValue<MenuBool>("UseSkin");
            if (skin && Player.SkinID != getskin) Player.SetSkin(getskin);

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;
                case OrbwalkerMode.LastHit:
                    break;
            }

            KillSteal();
            if (Menu["Misc"].GetValue<MenuBool>("autolevel")) Levelup();
            if (Menu["R"].GetValue<MenuKeyBind>("RT").Active) CastR2();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
          CastQ();
          CastQ3();
          CastW();
          CastE();
          CastR();
          CastRAIO();
        }

        private static void Harass()
        {
            CastQ2();
        }
        private static void LaneClear()
        {
            var minons = GameObjects.GetMinions(Player.Position, Q.Range).Where(x => x.Health <= Qdmg(x))
                .OrderByDescending(x => x.MaxHealth).FirstOrDefault();
            if (minons == null || !Menu["LaneClear"].GetValue<MenuBool>("Q")) return;
            Q.Cast(minons);
        }

        private static void JungleClear()
        {
            var Jungle = GameObjects.GetJungles(Player.Position, Q.Range).OrderByDescending(x => x.MaxHealth)
                .ThenBy(x => x.DistanceToPlayer()).FirstOrDefault();
            if (Jungle == null || !Menu["JungleClear"].GetValue<MenuBool>("Q")) return;
            Q.Cast(Jungle);
        }

        private static void KillSteal()
        {
         
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

                return Player.Spellbook.GetSpell(SpellSlot.Q).Name == "YoneQ" ? QStage.First : QStage.Second;
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
            var target = TargetSelector.GetTarget(Q.Range - 25);
            if (target == null || _QStage != QStage.First) return;
            var truedelay = 0.4f * (1 - Math.Min((Player.AttackSpeedMod - 1) * 0.58f, 0.67f));
            if (Q.Delay > truedelay) Q.Delay = truedelay;
            if (target.DistanceToPlayer() > Player.GetRealAutoAttackRange()) Q.Cast(target); 
        }
        private static void CastQ2()
        {
            var target = TargetSelector.GetTarget(Q.Range - 25);
            if (target == null || _QStage != QStage.First) return;
            var truedelay = 0.4f * (1 - Math.Min((Player.AttackSpeedMod - 1) * 0.58f, 0.67f));
            if (Q.Delay > truedelay) Q.Delay = truedelay;
            Q.Cast(target); 
        }
        private static void CastQ3()
        {
            var target = TargetSelector.GetTarget(Q3.Range - 50);
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
            if (target == null || !R.IsReady()) return;
            var rpre = R.GetPrediction(target);
            if (rpre.Hitchance >= HitChance.VeryHigh) R.Cast(rpre.CastPosition);
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
            if (!R.IsReady() || target == null) return;
            R.Cast(target, false, false, true, 2);
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

        private static void buyitem()
        {
            var gold = Player.Gold;
            var time = Game.Time / 60;
            var item = Menu["autoitem"].GetValue<MenuList>("selectitem").SelectedValue;

            if (item != "none" && Game.MapId == GameMapId.SummonersRift)
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
                }
        }

        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor) => aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        
        private static void Levelup()
        {
            if (Math.Abs(Player.PercentCooldownMod) >= 0.8) return; // if it's urf Don't auto level 
            if ( Q.Level + W.Level + E.Level + R.Level >= Player.Level || Player.Level > 18) return;

            var level = new[] {0, 0, 0, 0};
            for (var i = 0; i < Player.Level; i++) level[_spellLevels[i] - 1] = level[_spellLevels[i] - 1] + 1;

            if (Q.Level < level[0]) Player.Spellbook.LevelSpell(SpellSlot.Q);
            if (W.Level < level[1]) Player.Spellbook.LevelSpell(SpellSlot.W);
            if (E.Level < level[2]) Player.Spellbook.LevelSpell(SpellSlot.E);
            if (R.Level < level[3]) Player.Spellbook.LevelSpell(SpellSlot.R);
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

        #endregion
    }
}