using System;
using System.Drawing;
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
using Font = SharpDX.Direct3D9.Font;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace MightyAio.Champions
{
    internal class Warwick
    {
        #region Basics

        private static Spell _q, _w, _e, _r;
        private static Menu _menu, _emotes;
        private static AIHeroClient Player => ObjectManager.Player;
        private static SystemColors _color;
        private static Font _berlinfont;
        private static int _mykills = 0 + Player.ChampionsKilled;
        private static int[] _spellLevels;
        private static float MyLastSpeed = Player.MoveSpeed;

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            _menu = new Menu("Warwick", "Warwick", true);
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("Q", "Use Q in combo"),
                new MenuBool("QA", "Auto Q on dasher in combo mode only")
            };
            _menu.Add(qMenu);
            var eMenu = new Menu("E", "E")
            {

                new MenuBool("EC", "Use E in Combo"),
            };
            _menu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuBool("R", "Use in Combo R"),
                new MenuBool("RTT", "Use R if target is under tower",false),
                new MenuKeyBind("RT", "simi R Key", Keys.T, KeyBindType.Press),
                new MenuSlider("RH", " R || When ur target health is below %", 50)
            };
            _menu.Add(rMenu);

            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("Q", "Use Q"),
            };
            _menu.Add(laneClearMenu);
            var JungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("E", "Use auto E"),
            };

            _menu.Add(JungleClearMenu);
            var killSteal = new Menu("KS", "KillSteal")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("R", "Use R",false)
            };
            _menu.Add(killSteal);
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 4, 0, 55),
                new MenuBool("autolevel", "Auto Level"),
                new MenuBool("UseE", "Auto E on dasher")
            };
            var itembuy = new Menu("autoitem", "Starter Item")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Hunter's Machete", "none"})
            };
            _menu.Add(itembuy);
            // use emotes
            _emotes = new Menu("Emotes", "Emotes")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Center", "East", "West", "South", "North", "Mastery"}),
                new MenuBool("Kill", "Use on kill")
            };
            miscMenu.Add(_emotes);
            _menu.Add(miscMenu);
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("DrawR", "Draw R"),
                new MenuBool("Drawkillabeabilities", "Draw kill able abilities")
            };
            _menu.Add(drawMenu);


            _menu.Attach();
        }

        #endregion menu


        #region Gamestart

        public Warwick()
        {
            _spellLevels = new[] {1, 2, 3, 2, 2, 4, 1, 1, 1, 1, 4, 3, 3, 3, 3, 4, 2, 2};
            _q = new Spell(SpellSlot.Q, 350);
            _q.SetTargetted(0f, 2000f);
            _w = new Spell(SpellSlot.W, 4000) {Delay = 0.5f};
            _e = new Spell(SpellSlot.E, 375) {Delay = 0f};
            _e.SetSkillshot(0f, 225f, 1000f, false, SkillshotType.Line);
            _r = new Spell(SpellSlot.R, 675);
            _r.SetSkillshot(0.1f, 100f, float.MaxValue, false, SkillshotType.Line);
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
            Dash.OnDash += (sender, args) =>
            {
                if (!_menu["Q"].GetValue<MenuBool>("QA")) return;
                if (sender.IsEnemy)
                    CastQ();
            };
            Interrupter.OnInterrupterSpell += (sender, args) => { };
        }

       
        #endregion

        #region args

        private void OrbwalkerOnOnAction(object sender, OrbwalkerActionArgs args)
        {
            if (Player.HasBuff("warwickrsound")) args.Process = false;
            if (args.Type == OrbwalkerType.AfterAttack && Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                CastQ();
            }
            if (args.Type == OrbwalkerType.AfterAttack && Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
            {
                JungleClear();
            }
        }

        private static void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender,
            AIBaseClientProcessSpellCastEventArgs args)
        {
            if (args.Target == null) return;
            if (sender.IsMe && args.Target.IsEnemy && args.Target is AIHeroClient) Game.SendEmote(EmoteId.Laugh);
            if (!args.Target.IsMe) return;
            // Cast E to reduce damage from jgl camps
            if (Player.HasBuff("WarwickE"))
                return; // it looks like both E cast and recast has the same name so I have to use if hasbuff 
            if (sender.IsMonster && Player.CountEnemyHeroesInRange(1000) == 0) _e.Cast();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_menu["Drawing"].GetValue<MenuBool>("DrawE") && _e.IsReady())
                Drawing.DrawCircle(Player.Position, _e.Range, Color.DarkCyan);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawR") && _r.IsReady())
                Drawing.DrawCircle(Player.Position, Rrange(), Color.Violet);

            var drawKill = _menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities");
            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget(2000)))
            {
                if (!enemyVisible.IsValidTarget()) continue;
                var autodmg = Player.GetAutoAttackDamage(enemyVisible) + passivedmg(enemyVisible);
                var aa = string.Format("AA Left:" + (int) (enemyVisible.Health / autodmg));
                if (!drawKill) continue;
                if (Qdmg(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (Qdmg(enemyVisible) > enemyVisible.Health + Rdmg(enemyVisible))
                    DrawText(_berlinfont, "Killable Skills (R + Q):",
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
            if (Player.ChampionsKilled > _mykills && _emotes.GetValue<MenuBool>("Kill"))
            {
                _mykills = Player.ChampionsKilled;
                Emote();
            }

            var gold = Player.Gold;
            var time = Game.Time / 60;
            var item = _menu["autoitem"].GetValue<MenuList>("selectitem").SelectedValue;

            if (item != "none" && Game.MapId == GameMapId.SummonersRift)
                switch (item)
                {
                    case "Hunter's Machete":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 500 && !Player.HasItem(ItemId.Hunters_Machete))
                                Player.BuyItem(ItemId.Hunters_Machete);
                            if (gold >= 150 && !Player.HasItem(ItemId.Refillable_Potion))
                                Player.BuyItem(ItemId.Refillable_Potion);
                        }

                        break;
                    }
                }

            var getskin = _menu["Misc"].GetValue<MenuSlider>("setskin").Value;
            var skin = _menu["Misc"].GetValue<MenuBool>("UseSkin");
            if (skin && Player.SkinID != getskin) Player.SetSkin(getskin);

            Orbwalker.AttackState = true;
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;
                case OrbwalkerMode.LastHit:
                    break;
            }

            var a = Variables.GameTimeTickCount;
            if (a + 250 >= Variables.GameTimeTickCount) MyLastSpeed = Player.MoveSpeed;
            KillSteal();
            if (_menu["Misc"].GetValue<MenuBool>("autolevel")) Levelup();
            if (_menu["R"].GetValue<MenuKeyBind>("RT").Active) CastR2();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            CastE();
            if (_menu["R"].GetValue<MenuBool>("R")) CastR();
        }
        
        private static void LaneClear()
        {
            var minons = GameObjects.GetMinions(Player.Position, _q.Range).Where(x => x.Health <= Qdmg(x))
                .OrderByDescending(x => x.MaxHealth).FirstOrDefault();
            if (minons == null || !_menu["LaneClear"].GetValue<MenuBool>("Q")) return;
            _q.Cast(minons);
        }

        private static void JungleClear()
        {
            var Jungle = GameObjects.GetJungles(Player.Position, _q.Range).OrderByDescending(x => x.MaxHealth)
                .ThenBy(x => x.DistanceToPlayer()).FirstOrDefault();
            if (Jungle == null || !_menu["JungleClear"].GetValue<MenuBool>("Q")) return;
            _q.Cast(Jungle);
        }
        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(3000);
            if (target == null) return;
            if (Qdmg(target)  >= target.Health + target.AllShield && _menu["KS"].GetValue<MenuBool>("Q") && _q.IsInRange(target)) _q.Cast(target);
            if (Rdmg(target) >= target.Health + target.AllShield && _menu["KS"].GetValue<MenuBool>("R"))
            {
                target = TargetSelector.GetTarget(Rrange());
                if (target == null || !_r.IsReady()) return;
                _r.Range = Rrange();
                var rpre = _r.GetPrediction(target);
                if (rpre.Hitchance >= HitChance.Medium) _r.Cast(rpre.CastPosition);
            }
        }

        #endregion


        #region Spell Functions

        private static void CastQ()
        {
            var target = TargetSelector.GetTarget(_q.Range);
            if (target == null) return;
            if (!_q.IsReady() || !_menu["Q"].GetValue<MenuBool>("Q")) return;
            _q.Cast(target);
        }


        private static void CastE()
        {
            var target = TargetSelector.GetTarget(_e.Range);
            if (target == null || !_e.IsInRange(target) || !_menu["E"].GetValue<MenuBool>("EC")) return;
            if (!_e.IsReady()) return;
            if (target.IsFacing(Player) && !Player.HasBuff("WarwickE"))
            {
                _e.Cast();
                return;
            }
            _e.Cast();
        }

        private static void CastR()
        {
            var target = TargetSelector.GetTarget(Rrange());
            if (target == null || !_r.IsReady()) return;
            if (target.IsUnderEnemyTurret() && !_menu["R"].GetValue<MenuBool>("RTT")) return;
            _r.Range = Rrange();
            var rpre = _r.GetPrediction(target);
            if (rpre.Hitchance >= HitChance.Medium &&
                target.HealthPercent <= _menu["R"].GetValue<MenuSlider>("RH").Value) _r.Cast(rpre.CastPosition);
        }
        private static void CastR2()
        {
            var target = TargetSelector.GetTarget(Rrange());
            if (target == null || !_r.IsReady()) return;
            _r.Range = Rrange();
            var rpre = _r.GetPrediction(target);
            if (rpre.Hitchance >= HitChance.Medium) _r.Cast(rpre.CastPosition);
        }

       
        #endregion

        #region damage

        private static float passivedmg(AIBaseClient t) => (float) Player.CalculateMagicDamage(t, 8 + 2 * Player.Level);

        private static float Qdmg(AIBaseClient t) => _q.IsReady() ? _q.GetDamage(t) + passivedmg(t) : 0;

        private static float Rdmg(AIHeroClient t) => (float) (_r.IsReady() ? _q.GetDamage(t) + passivedmg(t) * 3 + Player.GetAutoAttackDamage(t) : 0);

        #endregion

        #region Extra functions

        private static float Rrange()
        {
            var range = MyLastSpeed * 2.50;
            if (range <= _r.Range) return _r.Range;

            return (float) ((float) range * 0.85);
        }

        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor) =>
        aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        

        private static void Levelup()
        {
            if (Math.Abs(Player.PercentCooldownMod) >= 0.8) return; // if it's urf Don't auto level 
            var qLevel = _q.Level;
            var wLevel = _w.Level;
            var eLevel = _e.Level;
            var rLevel = _r.Level;

            if (qLevel + wLevel + eLevel + rLevel >= Player.Level || Player.Level > 18) return;

            var level = new[] {0, 0, 0, 0};
            for (var i = 0; i < Player.Level; i++) level[_spellLevels[i] - 1] = level[_spellLevels[i] - 1] + 1;

            if (qLevel < level[0]) Player.Spellbook.LevelSpell(SpellSlot.Q);
            if (wLevel < level[1]) Player.Spellbook.LevelSpell(SpellSlot.W);
            if (eLevel < level[2]) Player.Spellbook.LevelSpell(SpellSlot.E);
            if (rLevel < level[3]) Player.Spellbook.LevelSpell(SpellSlot.R);
        }

        private static void Emote()
        {
            var b = _emotes.GetValue<MenuList>("selectitem").SelectedValue;
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