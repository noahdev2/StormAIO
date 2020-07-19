using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Security.Permissions;
using MightyAio.Properties;
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
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    internal class Tryndamere
    {
        #region Basics

        private static Spell _q, _w, _e, _r;
        private static Menu _menu, _emotes;
        private static AIHeroClient Player => ObjectManager.Player;
        private static SystemColors _color;
        private static Font _berlinfont;
        private static int _mykills = 0 + Player.ChampionsKilled;
        private static int[] _spellLevels;
        private static double tmdmg = 0;
        private static int LastTattack=0;
        private static int lastskin = 0;
        private static int time = 0;
        private static int lastsound = 7;
        public static SoundPlayer sounds;
        #endregion

        #region Menu

        private static void CreateMenu()
        {
            _menu = new Menu("Tryndamere", "Tryndamere", true);
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("Q", "Auto Q"),
                new MenuSlider("QH", "Use Auto Q || when your Health is below %", 20)
            };
            _menu.Add(qMenu);
            var wMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuBool("WH", "Use W in Harass")
            };
            _menu.Add(wMenu);
            var eMenu = new Menu("E", "E")
            {
                new MenuKeyBind("ET", "Use E under Turrent", Keys.T, KeyBindType.Toggle),
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("EH", "Use E in Harass"),
                new MenuSlider("ECC", "only E when crit chance is higher", 10),
                new MenuKeyBind("FEEL", "Feel Key", Keys.Z, KeyBindType.Press),
                new MenuBool("EF", "Use E in feel")
            };
            _menu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuBool("R", "Auto R"),
                new MenuSlider("RH", "Auto R || When ur health is below %", 20),
                new MenuBool("RS", "R sound"),
            };
            _menu.Add(rMenu);

            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("E", "Use E"),
                new MenuBool("EE", "Use E Only when are no Enemies around")
            };
            _menu.Add(laneClearMenu);
            var JungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                new MenuBool("E", "Use E")
            };

            _menu.Add(JungleClearMenu);
            var killSteal = new Menu("KS", "KillSteal")
            {
                new MenuBool("E", "Use E")
            };
            _menu.Add(killSteal);
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 4, 0, 55),
                new MenuBool("autolevel", "Auto Level"),
                new MenuBool("UseW", "Auto W dasher")
            };
            var itembuy = new Menu("autoitem", "Starter Item")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Dorans Blade", "Long Sword", "none"})
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
                new MenuBool("DrawW", "Draw W", false),
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("Drawkillabeabilities", "Draw kill abe abilities")
            };
            _menu.Add(drawMenu);


            _menu.Attach();
        }

        #endregion menu


        #region Gamestart

        public Tryndamere()
        {
            _spellLevels = new[] {3, 1, 1, 2, 1, 4, 1, 3, 1, 3, 3, 3, 2, 2, 2, 2, 4, 4};
            _q = new Spell(SpellSlot.Q);
            _w = new Spell(SpellSlot.W, 850) {Delay = 0.3f};
            _e = new Spell(SpellSlot.E, 660);
            _e.SetSkillshot(0f, 225f, 1000f, false, SkillshotType.Line);
            _r = new Spell(SpellSlot.R, 600);
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
            Spellbook.OnCastSpell += SpellbookOnOnCastSpell;
            AIBaseClient.OnProcessSpellCast += AIBaseClientOnOnProcessSpellCast;
            Dash.OnDash += (sender, args) =>
            {
                if (sender.IsEnemy && !sender.IsFacing(Player) && Player.IsFacing(sender))
                    if (_w.IsInRange(sender))
                        _w.Cast();
            };
            Interrupter.OnInterrupterSpell += (sender, args) => { };
        }

     

        private void SpellbookOnOnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (_r.IsReady() && args.Target != null && args.Slot == SpellSlot.R && args.Target.IsMe &&
                sender.Owner.IsEnemy && sender.Owner is AIHeroClient)
            {
               var dmg= ((AIHeroClient) sender.Owner).GetSpellDamage(Player, args.Slot);
               if (dmg * 1.5 >= Player.Health)
               {
                   _r.Cast();
               }
            } 
            if (!_r.IsReady() || args.Target == null ||
                Player.HealthPercent >= RH) return;
            if (sender.Owner.IsEnemy   && args.Target != null && args.Target.IsMe)
            {
               
                    _r.Cast();
                
            }
        }

        #endregion

        #region Etc

        private static  bool UseE => _menu["E"].GetValue<MenuKeyBind>("ET").Active;
        private static  bool Feel => _menu["E"].GetValue<MenuKeyBind>("FEEL").Active;
        private static  int RH => _menu["R"].GetValue<MenuSlider>("RH").Value;

        #endregion

        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.Slot == SpellSlot.R)
            {
                lastskin = _menu["Misc"].GetValue<MenuSlider>("setskin").Value;
                if (!_menu["R"].GetValue<MenuBool>("RS")) return;
                var dir = Resource1._1;
                var dir2 = Resource1._2;
                var dir3 = Resource1._3;
                var dir4 = Resource1._4;
                var dir5 = Resource1._5;
                var dir6 = Resource1._6;
                var random = new Random();
               var num = random.Next(6);
                
                if (lastsound == num)  num = random.Next(6);
                var alldir = new[] { dir, dir2, dir3, dir4,dir5,dir6};
                //   int num = random.Next(a);
                sounds = new SoundPlayer(alldir[num]);
                sounds.Load();
                sounds.Play();
                lastsound = num;

            }
            if (sender.IsMe && args.Slot == SpellSlot.E)
            {
                Game.SendEmote(EmoteId.Laugh);
            }
            if (sender.IsEnemy  && sender is AITurretClient && args.Target != null && sender.DistanceToPlayer() < 1400 && args.Target.IsMe && args.SData.Name == "SRUAP_Turret_Chaos1BasicAttack")
            {
                var dmg = sender.GetAutoAttackDamage(Player);
                LastTattack = Variables.GameTimeTickCount;
                if (tmdmg > 1.2) tmdmg = 1.2;
                var total = dmg * 1.1248 + (dmg * 1.1248 ) * tmdmg;
                if (total >= Player.Health && _r.IsReady())
                {
                    _r.Cast();
                }
                tmdmg += 0.4;
            }
            
            if (!_r.IsReady() || args.Target == null ||
                Player.HealthPercent >= RH) return;
            if (sender is AIHeroClient && args.Target.IsMe )
            {
                _r.Cast();
            }
            if (sender.IsMinion || sender.IsMonster && args.Target.IsMe)
            {
                var dmg = sender.GetAutoAttackDamage(Player);
                if (dmg >= Player.Health)
                {
                    _r.Cast();
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_menu["Drawing"].GetValue<MenuBool>("DrawW") && _w.IsReady())
                Drawing.DrawCircle(Player.Position, _w.Range, Color.DarkCyan);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawE") && _e.IsReady())
                Drawing.DrawCircle(Player.Position, _e.Range, Color.Violet);

            var drawKill = _menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities");
            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget(2000)))
            {
                if (!enemyVisible.IsValidTarget()) continue;
                var autodmg = Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage) +
                              Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage) *
                              Player.Crit;
                var aa = string.Format("AA Left:" + (int) (enemyVisible.Health / autodmg));
                if (!drawKill) continue;
                if (_e.GetDamage(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (E):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else
                    DrawText(_berlinfont, aa, (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
            }
            if (UseE)
                DrawText(_berlinfont, "E under turent is on",
                    (int) Drawing.WorldToScreen(Player.Position)[0] - 58,
                    (int) Drawing.WorldToScreen(Player.Position)[1] + 30, SharpDX.Color.White);
            if (!UseE)
                DrawText(_berlinfont, "E under turent is off",
                    (int) Drawing.WorldToScreen(Player.Position)[0] - 58,
                    (int) Drawing.WorldToScreen(Player.Position)[1] + 30, SharpDX.Color.White);
        }

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
                    case "Long Sword":
                    {
                        if (time < 1 && Player.InShop())
                            if (gold >= 500 && !Player.HasItem(ItemId.Long_Sword))
                                Player.BuyItem(ItemId.Long_Sword);
                        if (gold >= 150 && !Player.HasItem(ItemId.Refillable_Potion))
                            Player.BuyItem(ItemId.Refillable_Potion);
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
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
                case OrbwalkerMode.LastHit:
                    break;
            }

            if (Player.HasBuff("UndyingRage")) forfun();
            if (Feel) feel();
            
            CastQ();
            KillSteal();
            if (_menu["Misc"].GetValue<MenuBool>("autolevel")) Levelup();
            restT();
            AIBaseClient.OnBuffLose += AIBaseClientOnOnBuffLose;
        }

        private static void AIBaseClientOnOnBuffLose(AIBaseClient sender, AIBaseClientBuffLoseEventArgs args)
        {
            if (sender.IsMe && args.Buff.Name == "UndyingRage")
            {
                _menu["Misc"].GetValue<MenuSlider>("setskin").SetValue(lastskin);
              
            }
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            if (_menu["E"].GetValue<MenuBool>("EC") &&
            _menu["E"].GetValue<MenuSlider>("ECC").Value < Player.Crit * 100)  CastE();
            if (_menu["W"].GetValue<MenuBool>("WC")) CastW();
        }

        private static void Harass()
        {
            if (_menu["W"].GetValue<MenuBool>("WH")) CastW();
            if (_menu["E"].GetValue<MenuBool>("EH") &&
                _menu["E"].GetValue<MenuSlider>("ECC").Value < Player.Crit * 100)  CastE();
        }

        private static void LaneClear()
        {
            var a = _menu["LaneClear"].GetValue<MenuBool>("E");
            var b = _menu["LaneClear"].GetValue<MenuBool>("EE");
            if (!a || !_e.IsReady()) return;
            if (b)
            {
                if (Player.CountEnemyHeroesInRange(1500) != 0) return;
                var laneE = GameObjects.GetMinions(Player.Position, _e.Range + _e.Width);
                var epre = _e.GetCircularFarmLocation(laneE);
                if (epre.MinionsHit >= 2) _e.Cast(epre.Position.Extend(Player.Position,- 100));
            }
            else
            {
                        var laneE = GameObjects.GetMinions(Player.Position, _e.Range + _e.Width);
                        var epre = _e.GetCircularFarmLocation(laneE);
                        if (epre.MinionsHit >= 2) _e.Cast(epre.Position.Extend(Player.Position,- 100));
            }
        }

        private static void JungleClear()
        {
            if (!_e.IsReady() || !_menu["JungleClear"].GetValue<MenuBool>("E")) return;
            var laneE = GameObjects.GetJungles(Player.Position, _e.Range + _e.Width);
            var epre = _e.GetCircularFarmLocation(laneE);
            if (epre.MinionsHit >= 1) _e.Cast(epre.Position.Extend(Player.Position,- 100));
                
        }

        private static void feel()
        {
            Orbwalker.Move(Game.CursorPos);
            if (_e.IsReady() && _menu["E"].GetValue<MenuBool>("EF")) _e.Cast(Game.CursorPos);
        }

        #endregion


        #region Spell Functions

        private static void CastQ()
        {
            if (Player.IsRecalling()) return;
            if (!_q.IsReady() || Player.HealthPercent > _menu["Q"].GetValue<MenuSlider>("QH").Value ||
                !_menu["Q"].GetValue<MenuBool>("Q"))
                return;
            _q.Cast();
        }

        private static void CastW()
        {
            var target = TargetSelector.GetTarget(_w.Range);
            if (target == null) return;
            if (!_w.IsReady() || !_w.IsInRange(target))
                return;
            if (!target.IsFacing(Player) && Player.IsFacing(target)) _w.Cast();
        }

        private static void CastE()
        {
            var target = TargetSelector.GetTarget(_e.Range);
            if (target == null || !_e.IsInRange(target)) return;
            if (!_e.IsReady()) return;
            var pos = Vector3.Zero;

            if (target.DistanceToPlayer() <= 300)
                pos = _e.GetPrediction(target).CastPosition.Extend(Player.Position, -50);

            if (target.DistanceToPlayer() <= 400 && target.DistanceToPlayer() >= 300)
                pos = _e.GetPrediction(target).CastPosition.Extend(Player.Position, -80);

            if (target.DistanceToPlayer() <= 500 && target.DistanceToPlayer() >= 400)
                pos = _e.GetPrediction(target).CastPosition.Extend(Player.Position, -120);

            if (target.DistanceToPlayer() <= _e.Range + 50 && target.DistanceToPlayer() >= 500)
                pos = _e.GetPrediction(target).CastPosition.Extend(Player.Position, -150);

            if (pos.IsUnderEnemyTurret() && !UseE && _e.GetDamage(target) < target.Health) return;

            _e.Cast(pos);
        }

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(_e.Range);
            if (target == null) return;
            if (target.Health < _e.GetDamage(target) && !target.IsInvulnerable &&
                _menu["KS"].GetValue<MenuBool>("E")) CastE();
        }

        #endregion

        #region Extra functions

        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

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
        private static void forfun()
        {
            
            if (Variables.GameTimeTickCount - time < 400) return;
            var random = new Random();
            var text = new List<int>
            {
                1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21
            };
            var num = random.Next(text.Count);
            _menu["Misc"].GetValue<MenuSlider>("setskin").SetValue(text[num]);
            time = Variables.GameTimeTickCount;
        }
        private static void restT()
        {
            if ( Variables.GameTimeTickCount - LastTattack > 3000)
            {
                tmdmg = 0;
            }
        }
        #endregion
    }
}