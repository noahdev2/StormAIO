﻿﻿using System;
using System.Drawing;
using System.Linq;
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
    internal class Akali
    {
        #region Basics

        private static Spell Q, W, E, E2,E3, R,R2;
        private static Menu _menu, _emotes, _items;
        private static AIHeroClient Player => ObjectManager.Player;
        private static SystemColors _color;
        private static Font _berlinfont;
        private static int _mykills = 0 + Player.ChampionsKilled;
        private static int[] _spellLevels;
        private static bool HasfirstE => Player.Spellbook.GetSpell(SpellSlot.E).Name == "AkaliE";
        private static bool HasSecondE => Player.Spellbook.GetSpell(SpellSlot.E).Name == "AkaliEb"; 
        private static bool HasfirstR => Player.Spellbook.GetSpell(SpellSlot.R).Name == "AkaliR";
        private static bool HasRecastR => Player.Spellbook.GetSpell(SpellSlot.R).Name == "AkaliRb";
        #endregion

        #region Menu

        private static void CreateMenu()
        {
            _menu = new Menu("Akali", "Akali", true);
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass"),
            };
            _menu.Add(qMenu);
            var wMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuSlider("WU", "Only Use W in Combo When Energy % < ", 50,0,200)
            };
            _menu.Add(wMenu);
            var eMenu = new Menu("E", "E")
            {
                new MenuBool("E1", "Use first E in Combo"),
                new MenuBool("E2", "Use second E in Combo"),
            };
            _menu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuBool("R3", "Use R  in combo "),
                new MenuBool("R", "Use first R  in combo "),
                new MenuBool("R2", "Use second R in combo  "),
            };
            _menu.Add(rMenu);

            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("Q","Use Q"),
                new MenuSlider("QE", "Only Use when Energy >",90,0,200),
            };
            _menu.Add(laneClearMenu);
            var killSteal = new Menu("KS", "KillSteal")
            {    
                new MenuBool("Q", "Q"),
                new MenuBool("E1", "Use first E"),
                new MenuBool("E2", "Use Second E"),
                new MenuBool("R1", "Use first R"),
                new MenuBool("R2", "Use second R"),
                new MenuBool("G", "Use Gunblade"),
            };
            _menu.Add(killSteal);
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 13, 0, 55),
                new MenuBool("autolevel", "Auto Level"),
            };
            var itembuy = new Menu("autoitem", "Starter Item")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Dorans Ring", "Dorans Shield","Long Sword", "none"})
            };
            _menu.Add(itembuy);
            _items = new Menu("Items", "Items")
            {
                new MenuSliderButton("UG","Use GunBlade in Combo || when target health is below %",60,10,100)
            };
            miscMenu.Add(_items);
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
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("DrawR", "Draw R"),
                new MenuBool("DrawR2", "Draw Second R",false),
                new MenuBool("Drawkillabeabilities", "Draw kill abe abilities")
            };
            _menu.Add(drawMenu);


            _menu.Attach();
        }

        #endregion menu

        #region Gamestart

        public Akali()
        {
            _spellLevels = new[] {1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2};
            Q = new Spell(SpellSlot.Q,500);
            Q.SetSkillshot(0.25f, 70f, 1200f, false, SkillshotType.Cone);
            W = new Spell(SpellSlot.W, 250) {Delay = 0.3f};
            W.SetSkillshot(0.3f, 350f, 1200f, false, SkillshotType.Circle);
            E = new Spell(SpellSlot.E, 825);
            E.SetSkillshot(0.25f, 70f, 1200, true, SkillshotType.Line);  
            E3 = new Spell(SpellSlot.E, 825);
            E3.SetSkillshot(0.4f, 70f, 1200, false, SkillshotType.Line);
            E2 = new Spell(SpellSlot.E, 25000) {Delay = 0.125f};
            R = new Spell(SpellSlot.R, 675);
            R.SetTargetted(0.3f,float.MaxValue);
            R2 = new Spell(SpellSlot.R,750);
            R2.SetSkillshot(0.125f,80f,float.MaxValue,false,SkillshotType.Line);
            CreateMenu();
            _berlinfont = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Berlin San FB Demi",
                    Height = 17,
                    Weight = FontWeight.DemiBold,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearTypeNatural
                });
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnProcessSpellCast += AIBaseClientOnOnProcessSpellCast;
            Game.OnNotify += delegate(GameNotifyEventArgs args)
            {
                if (args.EventId == GameEventId.OnReincarnate && _menu["Misc"].GetValue<MenuBool>("UseSkin"))
                    Player.SetSkin(_menu["Misc"].GetValue<MenuSlider>("setskin").Value);
            };
            
        }

       

        #endregion

      
        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender is AITurretClient && args.Target.IsMe && W.IsReady() && !E.IsReady())
            {
                W.Cast(Player.Position);
            }
            if (sender is AITurretClient && args.Target.IsMe && E.IsReady() && Player.CountEnemyHeroesInRange(2000) == 0 && HasfirstE)
            {
                var turrents = GameObjects.EnemyTurrets.FirstOrDefault(x => x.IsValid && x.DistanceToPlayer() < 1200);
                E3.Cast(turrents);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_menu["Drawing"].GetValue<MenuBool>("DrawQ") && Q.IsReady())
                Drawing.DrawCircle(Player.Position, Q.Range, Color.DarkCyan);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawE") && E.IsReady())
                Drawing.DrawCircle(Player.Position, E.Range, Color.Violet);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawR") && R.IsReady())
                Drawing.DrawCircle(Player.Position, R.Range, Color.Firebrick);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawR2") && R.IsReady())
                Drawing.DrawCircle(Player.Position, R2.Range, Color.Firebrick);

            var drawKill = _menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities");
            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget(2000)))
            {
                if (!enemyVisible.IsValidTarget()) continue;
                var autodmg = Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage);
                var aa = string.Format("AA Left:" + (int) (enemyVisible.Health / autodmg));
                if (!drawKill) continue;
                if (Q.GetDamage(enemyVisible) + passivedmg(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + passive)",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (Q.GetDamage(enemyVisible) + Edmg(enemyVisible) +
                    passivedmg(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + E + passive)",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (Q.GetDamage(enemyVisible) + Edmg(enemyVisible)*2 +
                    passivedmg(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + E + E + passive)",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (Q.GetDamage(enemyVisible) + Edmg(enemyVisible)*2 +
                    bothRdmg(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + E + E + R first)",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (Q.GetDamage(enemyVisible) + Edmg(enemyVisible)*2 +
                    passivedmg(enemyVisible) + bothRdmg(enemyVisible,1) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + E + E + Second R)",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (Q.GetDamage(enemyVisible) + Edmg(enemyVisible)*2 +
                    passivedmg(enemyVisible) + bothRdmg(enemyVisible,2) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + E + E + both R)",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else
                    DrawText(_berlinfont, aa, (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
            }
        }

        #region gameupdate

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling()) return;
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
                    case "Dorans Ring":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 500 && !Player.HasItem(ItemId.Dorans_Ring)) Player.BuyItem(ItemId.Dorans_Ring);
                            if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                                Player.BuyItem(ItemId.Health_Potion);
                        }

                        break;
                    }
                    case "Dorans Shield":
                    {
                        if (time < 1 && Player.InShop())
                            if (gold >= 500 && !Player.HasItem(ItemId.Long_Sword))
                                Player.BuyItem(ItemId.Long_Sword);
                        if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                            Player.BuyItem(ItemId.Health_Potion);
                        break;
                    }
                    case "Long Sword":
                    {
                        if (time < 1 && Player.InShop())
                            if (gold >= 500 && !Player.HasItem(ItemId.Dorans_Shield))
                                Player.BuyItem(ItemId.Dorans_Shield);
                        if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                            Player.BuyItem(ItemId.Health_Potion);
                        break;
                    }
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
                    case OrbwalkerMode.LastHit:
                        break;
                }
            
                
            KillSteal();
            if (_menu["Misc"].GetValue<MenuBool>("autolevel")) Levelup();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            var R3 = _menu["R"].GetValue<MenuBool>("R3");
            if (_menu["E"].GetValue<MenuBool>("E2")) CastE2();
            var target = TargetSelector.GetTarget(E.Range);
            if (target == null) return;
            if (_menu["R"].GetValue<MenuBool>("R") && R3) CastR();
            if (_menu["R"].GetValue<MenuBool>("R2") && R3) CastR2();
            if (_menu["Q"].GetValue<MenuBool>("QC") ) { CastQ(); }
             if (_menu["W"].GetValue<MenuBool>("WC") && Player.Mana < _menu["W"].GetValue<MenuSlider>("WU").Value) CastW(target);
            if (_menu["E"].GetValue<MenuBool>("E1") )  CastE();
            if (!_items.GetValue<MenuSliderButton>("UG").Enabled) return;
            if (_items.GetValue<MenuSliderButton>("UG").ActiveValue < target.HealthPercent) return;
            if (Player.CanUseItem((int) ItemId.Hextech_Gunblade) &&
                target.Distance(Player) <= 700 ) Player.UseItem(3146, target);
            if (Player.CanUseItem((int) ItemId.Bilgewater_Cutlass) && target.Distance(Player) <= 550 ) 
                Player.UseItem((int) ItemId.Bilgewater_Cutlass, target);
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range);
            if (target == null) return;
            if (_menu["Q"].GetValue<MenuBool>("QH") ) { CastQ(); }
        }

        private static void LaneClear()
        {
        if (!_menu["LaneClear"].GetValue<MenuBool>("Q").Enabled ||
            Player.Mana <= _menu["LaneClear"].GetValue<MenuSlider>("QE").Value) return;
            var minons = GameObjects.GetMinions(Player.Position, Q.Range)
                .Where(x => x.IsValid && !x.IsDead ).ToList();
            if (minons.Any())

                foreach (var minon in minons)
                {
                    var Lane = Q.GetCircularFarmLocation(minons);
                    if (Lane.Position.IsValid() && Lane.MinionsHit >= 1)
                    {
                        Q.Cast(Lane.Position);
                        return;
                    }
                }
            
        }
        
        #endregion


        #region Spell Functions

        private static void CastQ()
        {
            var target = TargetSelector.GetTarget(Q.Range);
            if (target == null) return;
            if (!Q.IsReady() || target.DistanceToPlayer() > Q.Range || Player.IsDashing() )
                return;
            Q.Cast(target);
        }

      
        private static void CastW(AIHeroClient target)
        {
            if (!W.IsReady() || !W.IsInRange(target))
                return;
            W.Cast(Player.Position);
        }

        private static void CastE()
        {
            var target = TargetSelector.GetTarget(E.Range);
            if (target == null) return;
            if (!E.IsReady() || !HasfirstE || Player.IsDashing()) return;
            if (!target.IsMoving || target.IsWindingUp)
            {

                var a =  E.GetPrediction(target);
                if (a.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield)) E.Cast(a.UnitPosition);
                return;
            }
            var Epre = E.GetPrediction(target);
            if ( Epre.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield) && E.IsInRange(target))
                E.Cast(Epre.CastPosition);
        }  
        
        private static void CastE2()
        {
            var target = TargetSelector.GetTarget(E2.Range);
            if (target == null) return;
            if (!E.IsReady() || !HasSecondE || !target.HasBuff("AkaliEMis")) return;
            E2.Cast(target);
        }

        private static void CastR()
        {
            var target = TargetSelector.GetTarget(R.Range);
            if (target == null) return;
            if (!R.IsReady() || !HasfirstR)
             return;
            if (R.IsInRange(target) && !target.HasBuffOfType(BuffType.SpellShield)) R.Cast(target);
        }
        private static void CastR2()
        {
            var target = TargetSelector.GetTarget(R2.Range);
            if (target == null) return;
            if (!R.IsReady() || !HasRecastR || !R2.IsInRange(target))
                return;
            if (!target.IsMoving || target.IsWindingUp)
            {

                var a =  R2.GetPrediction(target);
                if (a.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield) && !target.HasBuffOfType(BuffType.Invulnerability)) 
                    R2.Cast(a.UnitPosition);
                return;
            }
            var Rpre = R2.GetPrediction(target);
            if (Rpre.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield) && !target.HasBuffOfType(BuffType.Invulnerability)) 
                R2.Cast(Rpre.UnitPosition);
        }

        private static void KillSteal()
        {
            var e2target = TargetSelector.GetTarget(E2.Range);
            if (e2target != null)
                if (e2target.Health + e2target.AllShield < Edmg(e2target) + passivedmg(e2target) + Q.GetDamage(e2target) && !e2target.IsInvulnerable  && _menu["KS"].GetValue<MenuBool>("E2"))
                {
                    CastE2();
                }
            
            var target = TargetSelector.GetTarget(E.Range);
            if (target == null) return;
            if (target.Health < Q.GetDamage(target) && !target.IsInvulnerable  && _menu["KS"].GetValue<MenuBool>("Q"))
            {
                CastQ();
            }
            if (target.Health + target.AllShield < Edmg(target) && !target.IsInvulnerable && !target.HasBuffOfType(BuffType.SpellShield) && _menu["KS"].GetValue<MenuBool>("E1"))
            {
               CastE();
            }
          
            if (target.Health + target.AllShield < bothRdmg(target,0) && !target.IsInvulnerable  && _menu["KS"].GetValue<MenuBool>("R1"))
            {
                CastR();
            }
            if (target.Health + target.AllShield < bothRdmg(target,1) && !target.IsInvulnerable  && _menu["KS"].GetValue<MenuBool>("R2"))
            {
                CastR2();
            }
            if (target.Health + target.AllShield < bothRdmg(target,1) && !target.IsInvulnerable  && _menu["KS"].GetValue<MenuBool>("R2"))
            {
                CastR2();
            }
            var gunbladedmg = Player.CalculateMagicDamage(target, 170 + 4.588 * Player.Level + Player.TotalMagicalDamage * 0.3);
            if (Player.CanUseItem((int) ItemId.Hextech_Gunblade) &&
                target.Distance(Player) <= 700 && target.Health + target.AllShield < gunbladedmg && _menu["KS"].GetValue<MenuBool>("G")) Player.UseItem(3146, target);
            if (Player.CanUseItem((int) ItemId.Bilgewater_Cutlass) && target.Distance(Player) <= 550 &&
                target.Health + target.AllShield < Player.CalculateMagicDamage(target, 100) && _menu["KS"].GetValue<MenuBool>("G")) Player.UseItem((int) ItemId.Bilgewater_Cutlass, target);

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
            var qLevel = Q.Level;
            var wLevel = W.Level;
            var eLevel = E.Level;
            var rLevel = R.Level;

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
            Game.SendEmote(EmoteId.Laugh);
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

        #region Damages
        // sdk damage is outdated 
      
        private static float bothRdmg(AIHeroClient target, int index = 0)
        {
            if (R.Level == 0) return 0;
            switch (index)
            {
                case 0:
                    var R1damage = 125f + 100f * (R.Level - 1);
                    var adbounus = Player.TotalAttackDamage - Player.BaseAttackDamage;
                    R1damage += adbounus * 0.5f;
                    return (float) Player.CalculateDamage(target, DamageType.Physical, R1damage);
                
                case 1:
                    float R2damage   = 75f + 70f *(R.Level - 1);
                    var missingHealthPercent = (1 - (target.Health / target.MaxHealth)) * 100;
                    var totalIncreasement = 1 + ((2.86f * missingHealthPercent) / 100);
                    var RDmg = (R2damage + 0.3 * Player.TotalMagicalDamage) * totalIncreasement;
              
          
                    R2damage = (float) RDmg;
                    return (float) Player.CalculateDamage(target, DamageType.Physical, R2damage );
                case 2:
                    float R11damage = 125f + 100f * (R.Level - 1);
                    var ad1bounus = Player.TotalAttackDamage - Player.BaseAttackDamage;
                    R11damage += ad1bounus * 0.5f;
                    float R22damage = 75f + 70f * (R.Level - 1);
                    var m1issingHealthPercent = (1 - (target.Health / target.MaxHealth)) * 100;
                    var t1otalIncreasement = 1 + ((2.86f * m1issingHealthPercent) / 100);
                    var R2Dmg = (R22damage + 0.3 * Player.TotalMagicalDamage) * t1otalIncreasement;
                    
                    return (float) ((float) Player.CalculateDamage(target, DamageType.Physical, R11damage  ) +  Player.CalculateDamage(target, DamageType.Magical, R2Dmg  ));
            }

            return 0;
        }
        private static float Edmg(AIHeroClient target)
        {
            if (E.Level >= 1)
            {
                float damage = 50 + 35 * (E.Level - 1);
                damage += Player.TotalAttackDamage * 0.35f + Player.TotalMagicalDamage * 0.5f;
                return (float) Player.CalculateDamage(target, DamageType.Magical, damage);
            }

            return 0;
        }

        private static float passivedmg(AIHeroClient target)
        {
            var damage = 0;
            var bounsAd = (Player.TotalAttackDamage - Player.BaseAttackDamage) * 0.60f;
            if (Player.Level <= 7)
            {
                damage = 39 + 3 * Player.Level;
            }
            if (Player.Level >= 8)
            {
                damage = 39 + 9 * Player.Level;
            }
            if (Player.Level >= 13)
            {
                damage = 39 + 15 * Player.Level;
            }

            var extradamage = bounsAd + Player.TotalMagicalDamage * 0.5f;
            return (float) Player.CalculateDamage(target,DamageType.Magical,damage + extradamage);
        }
        #endregion
    }
}