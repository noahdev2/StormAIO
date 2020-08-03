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
    internal class Zed
    {
        #region Basics

        private static Spell _q, _w, _e, _r;
        private static Menu _menu, _emotes;
        private static AIHeroClient Player => ObjectManager.Player;
        private static SystemColors _color;
        private static Font _berlinfont;
        private static int _mykills = 0 + Player.ChampionsKilled;
        private static int[] _spellLevels;
        private static Vector3 RShadowpos;
        #endregion

        #region Menu

        private static void CreateMenu()
        {
            _menu = new Menu("MightyZed", "Mighty Zed", true);
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

        public Zed()
        {
            _spellLevels = new[] {1, 2, 3, 1, 1, 4, 1, 3, 3, 1, 4, 3, 3, 2, 2, 4, 2, 2};
            _q = new Spell(SpellSlot.Q, 900);
            _q.SetSkillshot(0.25f, 50f,1700f,false,SkillshotType.Line);
            _w = new Spell(SpellSlot.W, 650); 
            _w.SetSkillshot(0f,50f,2500,false,SkillshotType.Line);
            _e = new Spell(SpellSlot.E, 290f) {Delay = 0f};
            _r = new Spell(SpellSlot.R,625);
            _r.SetTargetted(0f, float.MaxValue);
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
            AIBaseClient.OnBuffGain += AIBaseClientOnOnBuffGain;
            Game.OnNotify += delegate(GameNotifyEventArgs args)
            {
                if (args.EventId == GameEventId.OnReincarnate && _menu["Misc"].GetValue<MenuBool>("UseSkin"))
                    Player.SetSkin(_menu["Misc"].GetValue<MenuSlider>("setskin").Value);
            };
        }

        private void AIBaseClientOnOnBuffGain(AIBaseClient sender, AIBaseClientBuffGainEventArgs args)
        {
            if (sender.IsMe)
            {
                Game.Print(args.Buff.Name);
            }
        }

        #endregion

        #region args
        
        private static void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
              //  Game.Print(args.SData.Name);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_menu["Drawing"].GetValue<MenuBool>("DrawE") && _e.IsReady())
                Drawing.DrawCircle(Player.Position, _e.Range, Color.DarkCyan);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawR") && _r.IsReady())
                Drawing.DrawCircle(Player.Position,_r.Range , Color.Violet);
            
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
                else if (Qdmg(enemyVisible)+ Rdmg(enemyVisible)> enemyVisible.Health )
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
            buyitem();
            var getskin = _menu["Misc"].GetValue<MenuSlider>("setskin").Value;
            var skin = _menu["Misc"].GetValue<MenuBool>("UseSkin");
            if (skin && Player.SkinID != getskin) Player.SetSkin(getskin);
            
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
            
            KillSteal();
            if (_menu["Misc"].GetValue<MenuBool>("autolevel")) Levelup();
            if (_menu["R"].GetValue<MenuKeyBind>("RT").Active) ;
            
         
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1400);
            if (target == null) return;
            if (_q.IsReady() && WShadow != null)
            {
                var qpre = _q.GetPrediction(target);
                if (WShadow != null && WShadow.Distance(target) <= Player.Distance(target))  _q.UpdateSourcePosition(WShadow.Position);
                if (qpre.Hitchance >= HitChance.High)
                {
                    _q.Cast(target);
                }
            }

            if ( Player.Spellbook.GetSpell(SpellSlot.W).Name != "ZedW") return;
          
                _w.Cast(target);

        }
        private static void Harass()
        {
         
        }
        
        private static void LaneClear()
        {
          
        }

        private static void JungleClear()
        {
          
        }
        private static void KillSteal()
        {
            var target = GameObjects.EnemyHeroes.Where(x => 
                x.IsValidTarget(_q.Range, true, possbiletarget()));
            
          
        }

        #endregion


        #region Spell Functions

        private static void CastQ()
        {
            
        }


        private static void CastE()
        {
          
        }

        private static void CastR()
        {
         
        }
     
        #endregion

        #region damage

        private static float passivedmg(AIBaseClient t) => (float) Player.CalculateMagicDamage(t, 8 + 2 * Player.Level);

        private static float Qdmg(AIBaseClient t) => _q.IsReady() ? _q.GetDamage(t) + passivedmg(t) : 0;

        private static float Rdmg(AIHeroClient t) => (float) (_r.IsReady() ? _r.GetDamage(t) + passivedmg(t) * 3 + Player.GetAutoAttackDamage(t) : 0);

        #endregion

        #region Extra functions

        private static Vector3 possbiletarget()
        {
            var target = GameObjects.EnemyHeroes.FirstOrDefault(x => x.IsValid);
            var allshowads = new[] {WShadow.Position, RShadow.Position, Player.Position};
            foreach (var showads in allshowads.Where(x=> x.IsValid()).OrderBy(x=> x.Distance(target)))
            {
                return showads;
            }

            return Player.Position;
        }
        private static void buyitem()
        {
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
        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor) =>
        aFont.DrawText(null, aText, aPosX, aPosY, aColor);

        private static void Levelup()
        {
            if (Math.Abs(Player.PercentCooldownMod) >= 0.8) return; // if it's urf Don't auto level 
            if (_q.Level + _w.Level + _e.Level + _r.Level >= Player.Level || Player.Level > 18) return;

            var level = new[] {0, 0, 0, 0};
            for (var i = 0; i < Player.Level; i++) level[_spellLevels[i] - 1] = level[_spellLevels[i] - 1] + 1;

            if (_q.Level < level[0]) Player.Spellbook.LevelSpell(SpellSlot.Q);
            if (_w.Level < level[1]) Player.Spellbook.LevelSpell(SpellSlot.W);
            if (_e.Level < level[2]) Player.Spellbook.LevelSpell(SpellSlot.E);
            if (_r.Level < level[3]) Player.Spellbook.LevelSpell(SpellSlot.R);
        }

        #region Shawdow postions

        private static AIMinionClient WShadow => GameObjects.Get<AIMinionClient>().FirstOrDefault(x => x.IsValid && x.IsAlly  && x.HasBuff("zedwshadowbuff")  && x.Name == "Shadow");
        private static AIMinionClient RShadow => GameObjects.Get<AIMinionClient>().FirstOrDefault(x => x.IsValid && x.IsAlly  && x.HasBuff("zedrshadowbuff")  && x.Name == "Shadow");
        

        #endregion
        #region Spell recasts

        private  enum WStage
        {
            First,
            Second,
            Cooldown
        }

        private static WStage _wStage
        {
            get
            {
                if (!_w.IsReady()) return WStage.Cooldown;

                return Player.Spellbook.GetSpell(SpellSlot.W).Name == "ZedW" ? WStage.First : WStage.Second;
            }
        } 
        private  enum RStage
        {
            First,
            Second,
            Cooldown
        }

        private static RStage _RStage
        {
            get
            {
                if (!_w.IsReady()) return RStage.Cooldown;

                return Player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR" ? RStage.First : RStage.Second;
            }
        } 

        #endregion

        #region Damage

        private static float passivedmg(AIHeroClient target)
        {
            var dmglevel = 6;
            if (Player.Level >= 7) dmglevel = 8;
            if (Player.Level >= 17) dmglevel = 10;

            return (float) (target.HealthPercent <= 50 && target.HasBuff("") ? Player.CalculateMagicDamage(target, target.MaxHealth / dmglevel) : 0);
        }

        #endregion
      

        #endregion
    }
}