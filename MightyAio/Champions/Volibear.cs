﻿using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font;

namespace MightyAio.Champions
{
    internal class Volibear
    {
        #region Basics

        private static Spell _q, _w, _e, _r;
        private static Menu _menu, _emotes, _items;
        private static AIHeroClient Player => ObjectManager.Player;
        private static float Range => ObjectManager.Player.GetRealAutoAttackRange();
        private static SystemColors _color;
        private static Font _berlinfont;
        private static bool _postAttack;
        private static int _mykills = 0 + Player.ChampionsKilled;
        private static readonly int[] Rwitdh = {300, 500, 700};
        private static int[] _spellLevels;

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            _menu = new Menu("Volibear", "Volibear", true);
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass"),
                new MenuList("QAA", "Use Q", new[] {"Before Attack", "After Attack"}),
                new MenuSlider("QM", "Mana for using Q in harass", 30)
            };
            _menu.Add(qMenu);
            var wMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuBool("WH", "Use W in Harass"),
                new MenuList("WAA", "Use W", new[] {"After Attack", "Before Attack"}),
                new MenuSlider("WM", "Mana for using W in harass", 30)
            };
            _menu.Add(wMenu);
            var eMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("EH", "Use E in Harass"),
                new MenuSlider("EM", "Mana for using E in harass", 30)
            };
            _menu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuBool("RC", "Use R in Combo"),
                new MenuSlider("RCH", "Use R in Combo when my health is below", 40)
            };
            var targets = from hero in ObjectManager.Get<AIHeroClient>()
                where hero.IsEnemy
                select hero;
            foreach (var target in targets)
                rMenu.Add(new MenuBool(target.CharacterName, "Use R on " + target.CharacterName));
            _menu.Add(rMenu);
            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("Q", "Q"),
                new MenuBool("W", "W"),
                new MenuBool("E", "E"),
                new MenuSlider("ELC", "use E in lane Clear with it can hit", 2, 1, 5),
                new MenuSlider("Mana", "Mana for Lane Clear", 40)
            };
            _menu.Add(laneClearMenu);
            var killSteal = new Menu("KS", "KillSteal")
            {
                new MenuBool("W", "Use W"),
                new MenuBool("E", "Use E")
            };
            _menu.Add(killSteal);
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 2, 0, 55),
                new MenuBool("autolevel", "Auto Level")
            };

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
                new MenuBool("DrawW", "Draw W"),
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("DrawR", "Draw R"),
                new MenuBool("Drawkillabeabilities", "Draw kill abe abilities")
            };
            _menu.Add(drawMenu);


            _menu.Attach();
        }

        #endregion menu

        #region Gamestart

        public Volibear()
        {
            _spellLevels = new[] {2, 1, 3, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3};
            _q = new Spell(SpellSlot.Q, Range);
            _w = new Spell(SpellSlot.W, 325);
            _w.SetTargetted(0.25f, int.MaxValue);
            _e = new Spell(SpellSlot.E, 1200);
            _e.SetSkillshot(1.10f, 325f, float.MaxValue, false, SkillshotType.Circle);
            _r = new Spell(SpellSlot.R, 700);
            _r.SetSkillshot(1f, Rwitdh[0], 750, false, SkillshotType.Circle);

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
            Orbwalker.OnAction += Orbwalker_OnAction;
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnProcessSpellCast += AIBaseClientOnProcessSpellCast;
            Game.OnNotify += delegate(GameNotifyEventArgs args)
            {
                if (args.EventId == GameEventId.OnReincarnate && _menu["Misc"].GetValue<MenuBool>("UseSkin"))
                    Player.SetSkin(_menu["Misc"].GetValue<MenuSlider>("setskin").Value);
            };
        }

        #endregion

        private void AIBaseClientOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            //  if (sender.IsEnemy && args.Target.IsMe && args.SData.Name == " ") _r.Cast(Game.CursorPos);

            if (args.SData.Name == "VolibearQAttack") Orbwalker.ResetAutoAttackTimer();
        }

        private void Orbwalker_OnAction(object sender, OrbwalkerActionArgs args)
        {
            if (args.Type == OrbwalkerType.AfterAttack && args.Sender.IsMe)
            {
                var orb = Orbwalker.GetTarget();
                if (orb != null) _postAttack = true;
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (_menu["Drawing"].GetValue<MenuBool>("DrawW"))
                Drawing.DrawCircle(Player.Position, _w.Range, Color.DarkCyan);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawE"))
                Drawing.DrawCircle(Player.Position, _e.Range, Color.Red);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawR"))
                Drawing.DrawCircle(Player.Position, _r.Range, Color.Firebrick);

            var drawKill = _menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities");
            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget(2000)))
            {
                if (!enemyVisible.IsValidTarget()) continue;
                var autodmg = Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage);
                var aa = string.Format("AA Left:" + (int) (enemyVisible.Health / autodmg));
                if (!drawKill) continue;
                if (_q.GetDamage(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (_q.GetDamage(enemyVisible) + _w.GetDamage(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + W):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (_q.GetDamage(enemyVisible) + _w.GetDamage(enemyVisible) + _e.GetDamage(enemyVisible) >
                         enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + W + E):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (_q.GetDamage(enemyVisible) + _w.GetDamage(enemyVisible) + _e.GetDamage(enemyVisible) +
                    _r.GetDamage(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + W + E + R):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else
                    DrawText(_berlinfont, aa, (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
            }
        }

        #region gameupdate

        private void Game_OnUpdate(EventArgs args)
        {
            if (Player.ChampionsKilled > _mykills && _emotes.GetValue<MenuBool>("Kill"))
            {
                _mykills = Player.ChampionsKilled;
                Emote();
            }

            var getskin = _menu["Misc"].GetValue<MenuSlider>("setskin").Value;
            var skin = _menu["Misc"].GetValue<MenuBool>("UseSkin");
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
            }

            KillSteal();
            if (_menu["Misc"].GetValue<MenuBool>("autolevel")) Levelup();
        }

        #endregion

        #region Orbwalker mod

        private void Combo()
        {
            var target = TargetSelector.GetTarget(900);
            var PlayerMana = Player.Level < 6 ? 0 : 100;
            if (target == null) return;
            if (_menu["E"].GetValue<MenuBool>("EC") && Player.Mana - 60 > PlayerMana)
                CastE(target);
            if (_menu["Q"].GetValue<MenuBool>("QC") && Player.Mana - 50 > PlayerMana)
                if (_menu["Q"].GetValue<MenuList>("QAA").SelectedValue == "After Attack")
                    if (_postAttack)
                    {
                        _q.Cast();
                        _postAttack = false;
                    }
                    else
                    {
                        _q.Cast();
                    }

            if (_menu["W"].GetValue<MenuBool>("WC") && _w.IsReady() && _w.IsInRange(target) &&
                Player.Mana - new[] {30, 35, 40, 45, 50}[_w.Level - 1] > PlayerMana)
                if (_menu["W"].GetValue<MenuList>("WAA").SelectedValue == "After Attack")
                    if (_postAttack)
                    {
                        _w.Cast(target);
                        _postAttack = false;
                    }
                    else
                    {
                        _w.Cast(target);
                    }

            if (_menu["R"].GetValue<MenuBool>("RC") && _r.IsReady() &&
                _menu["R"].GetValue<MenuBool>(target.CharacterName) &&
                Player.HealthPercent <= _menu["R"].GetValue<MenuSlider>("RCH").Value)
                _r.Cast(target);
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(900);
            if (target == null) return;
            if (_menu["E"].GetValue<MenuBool>("EH") && Player.ManaPercent > _menu["E"].GetValue<MenuSlider>("EM").Value)
                CastE(target);
            if (_menu["Q"].GetValue<MenuBool>("QH") && Player.ManaPercent > _menu["Q"].GetValue<MenuSlider>("QM").Value)
                if (_menu["Q"].GetValue<MenuList>("QAA").SelectedValue == "After Attack")
                    if (_postAttack)
                    {
                        _q.Cast();
                        _postAttack = false;
                    }
                    else
                    {
                        _q.Cast();
                    }

            if (_menu["W"].GetValue<MenuBool>("WH") && _w.IsReady() && _w.IsInRange(target) &&
                Player.ManaPercent > _menu["W"].GetValue<MenuSlider>("WM").Value)
                if (_menu["W"].GetValue<MenuList>("WAA").SelectedValue == "After Attack")
                    if (_postAttack)
                    {
                        _w.Cast(target);
                        _postAttack = false;
                    }
                    else
                    {
                        _w.Cast(target);
                    }
        }

        private void LaneClear()
        {
            var minons = GameObjects.GetMinions(Player.Position, _q.Range).Where(x => x.IsValid && !x.IsDead).ToList();
            bool useQ = _menu["LaneClear"].GetValue<MenuBool>("Q");
            bool useW = _menu["LaneClear"].GetValue<MenuBool>("W");
            bool useE = _menu["LaneClear"].GetValue<MenuBool>("E");
            var mana = _menu["LaneClear"].GetValue<MenuSlider>("Mana").Value;
            if (minons.Any())
                if (mana > Player.ManaPercent)
                    return;
            foreach (var minon in minons.OrderBy(x => x.DistanceToPlayer()))
            {
                var laneE = GameObjects.GetMinions(Player.Position, _e.Range + _e.Width);
                var efarmpos = _e.GetLineFarmLocation(laneE, _e.Width);
                if (useQ && _e.IsReady() &&
                    efarmpos.MinionsHit >= _menu["LaneClear"].GetValue<MenuSlider>("ELC").Value &&
                    laneE.Count >= _menu["LaneClear"].GetValue<MenuSlider>("ELC").Value &&
                    _e.IsInRange(minon)) _e.Cast(minon);

                if (useW && _w.IsReady() && _w.IsInRange(minon)) _w.Cast(minon);

                if (useE && _q.IsReady() && minon.DistanceToPlayer() < Player.GetRealAutoAttackRange()) _q.Cast();
                break;
            }

            var jgls = GameObjects.GetJungles(_q.Range).Where(x => x.IsValid && !x.IsDead).ToList();
            if (jgls.Any())
                foreach (var jgl in jgls.OrderBy(x => x.DistanceToPlayer()))
                {
                    if (useQ && _e.IsReady() && _e.IsInRange(jgl)) _e.Cast(jgl);

                    if (useW && _w.IsReady() && _w.IsInRange(jgl)) _w.Cast(jgl);

                    if (useE && _q.IsReady() && jgl.DistanceToPlayer() < Player.GetRealAutoAttackRange()) _q.Cast();
                    break;
                }
        }

        #endregion


        #region Spell Functions

        private static void CastW(AIHeroClient target)
        {
            if (!_w.IsReady() || !_w.IsInRange(target)) return;
            _w.Cast(target);
        }

        private static void CastE(AIHeroClient target)
        {
            if (!_e.IsReady()) return;
            var Epre = _e.GetPrediction(target);
            if (Epre.Hitchance >= HitChance.High) _e.Cast(Epre.CastPosition);
        }


        private void KillSteal()
        {
            var target = TargetSelector.GetTarget(_q.Range);
            if (target == null) return;
            if (target.Health < _w.GetDamage(target) && !target.IsInvulnerable && _menu["KS"].GetValue<MenuBool>("W"))
                CastW(target);
            if (target.Health < _e.GetDamage(target) && !target.IsInvulnerable && _menu["KS"].GetValue<MenuBool>("E"))
                CastE(target);
        }

        #endregion

        #region Extra functions

        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

        private static void Levelup()
        {
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